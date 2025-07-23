"""
Entity Linker - Phase 1 Implementation
Solves the symbol grounding problem by mapping natural language concepts to UKS entities.

Based on research from:
- Symbol grounding problem in knowledge representation
- Entity disambiguation techniques in neuro-symbolic AI
- Candidate belief systems for knowledge augmentation
- Context-aware entity resolution
"""

import json
import re
import logging
from typing import Dict, List, Tuple, Optional, Set, Any
from dataclasses import dataclass, field
from enum import Enum
from datetime import datetime
import difflib
from collections import defaultdict
import hashlib

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class EntityType(Enum):
    """Types of entities that can be linked"""
    PERSON = "person"
    LOCATION = "location"
    ORGANIZATION = "organization"
    CONCEPT = "concept"
    EVENT = "event"
    TEMPORAL = "temporal"
    UNKNOWN = "unknown"

class DisambiguationStrategy(Enum):
    """Strategies for entity disambiguation"""
    CONTEXT_BASED = "context_based"
    FREQUENCY_BASED = "frequency_based"
    RELATIONSHIP_BASED = "relationship_based"
    HYBRID = "hybrid"

@dataclass
class EntityCandidate:
    """Represents a candidate entity match from UKS"""
    uks_name: str
    entity_type: EntityType
    confidence_score: float
    disambiguation_features: Dict[str, Any]
    context_matches: List[str]
    relationship_strength: float
    frequency_score: float
    
    def __post_init__(self):
        # Calculate overall score
        self.overall_score = (
            self.confidence_score * 0.4 +
            self.relationship_strength * 0.3 +
            self.frequency_score * 0.3
        )

@dataclass
class CandidateBelief:
    """Represents a candidate belief for new knowledge"""
    subject: str
    predicate: str
    object: str
    initial_confidence: float
    source: str
    evidence: List[str]
    requires_validation: bool
    created_at: datetime
    disambiguation_context: Dict[str, Any]
    
    def __post_init__(self):
        if not hasattr(self, 'created_at') or self.created_at is None:
            self.created_at = datetime.now()

@dataclass
class LinkingResult:
    """Result of entity linking process"""
    original_text: str
    linked_entities: Dict[str, EntityCandidate]
    candidate_beliefs: List[CandidateBelief]
    disambiguation_decisions: List[Dict[str, Any]]
    confidence_score: float
    processing_time: float
    warnings: List[str] = field(default_factory=list)

class EntityLinker:
    """
    Main entity linking component that maps natural language concepts to UKS entities.
    Handles disambiguation and creates candidate beliefs for new knowledge.
    """
    
    def __init__(self, uks_interface=None):
        """
        Initialize the Entity Linker
        
        Args:
            uks_interface: Interface to UKS system (injected dependency)
        """
        self.uks_interface = uks_interface
        
        # Entity recognition patterns
        self.entity_patterns = {
            EntityType.PERSON: [
                r'\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+)+)\b(?=\s+(?:is|was|has|have|will|can|said|thinks))',
                r'\b(?:Mr\.|Mrs\.|Dr\.|Prof\.)\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\b',
                r'\b([A-Z][a-z]+)\s+(?:said|thinks|believes|knows|likes|dislikes)\b'
            ],
            EntityType.LOCATION: [
                r'\b(?:in|at|from|to|near|around)\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\b',
                r'\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\s+(?:city|town|country|state|province)\b',
                r'\bthe\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\s+(?:River|Mountain|Lake|Sea|Ocean)\b'
            ],
            EntityType.ORGANIZATION: [
                r'\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\s+(?:Corporation|Corp|Company|Inc|University|College)\b',
                r'\bthe\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\s+(?:Department|Ministry|Agency)\b'
            ],
            EntityType.CONCEPT: [
                r'\b(?:the concept of|the idea of|meaning of)\s+([a-z]+(?:\s+[a-z]+)*)\b',
                r'\b([a-z]+(?:\s+[a-z]+)*)\s+(?:concept|principle|theory|philosophy)\b'
            ]
        }
        
        # Disambiguation weights
        self.disambiguation_weights = {
            'context_similarity': 0.4,
            'relationship_strength': 0.3,
            'frequency_score': 0.2,
            'type_consistency': 0.1
        }
        
        # Known ambiguous entities (examples)
        self.ambiguous_entities = {
            'paris': ['Paris, France', 'Paris, Texas', 'Paris Hilton'],
            'washington': ['Washington, D.C.', 'Washington State', 'George Washington'],
            'apple': ['Apple Inc.', 'apple fruit', 'Apple Records'],
            'mercury': ['Mercury planet', 'mercury element', 'Mercury Roman god']
        }
        
        # Entity type indicators
        self.type_indicators = {
            EntityType.PERSON: ['person', 'individual', 'human', 'people', 'someone', 'he', 'she'],
            EntityType.LOCATION: ['place', 'location', 'where', 'city', 'country', 'there'],
            EntityType.ORGANIZATION: ['company', 'organization', 'institution', 'group'],
            EntityType.CONCEPT: ['concept', 'idea', 'principle', 'theory', 'notion']
        }
        
        # Cache for entity lookups
        self.entity_cache = {}
        self.relationship_cache = {}
        
    def link_entities(self, text: str, context: Dict[str, Any] = None) -> LinkingResult:
        """
        Main method to link entities in text to UKS entities
        
        Args:
            text: Natural language text to process
            context: Additional context for disambiguation
            
        Returns:
            LinkingResult with linked entities and candidate beliefs
        """
        start_time = datetime.now()
        logger.info(f"Starting entity linking for: {text}")
        
        if context is None:
            context = {}
        
        # Step 1: Extract potential entities
        potential_entities = self._extract_potential_entities(text)
        
        # Step 2: Find UKS candidates for each entity
        entity_candidates = {}
        for entity_text, entity_type in potential_entities.items():
            candidates = self._find_uks_candidates(entity_text, entity_type, context)
            if candidates:
                entity_candidates[entity_text] = candidates
        
        # Step 3: Disambiguate entities
        linked_entities = {}
        disambiguation_decisions = []
        
        for entity_text, candidates in entity_candidates.items():
            best_candidate, decision = self._disambiguate_entity(
                entity_text, candidates, text, context
            )
            linked_entities[entity_text] = best_candidate
            disambiguation_decisions.append(decision)
        
        # Step 4: Create candidate beliefs for new knowledge
        candidate_beliefs = self._create_candidate_beliefs(text, linked_entities, context)
        
        # Step 5: Calculate overall confidence
        confidence_score = self._calculate_linking_confidence(linked_entities, candidate_beliefs)
        
        processing_time = (datetime.now() - start_time).total_seconds()
        
        result = LinkingResult(
            original_text=text,
            linked_entities=linked_entities,
            candidate_beliefs=candidate_beliefs,
            disambiguation_decisions=disambiguation_decisions,
            confidence_score=confidence_score,
            processing_time=processing_time
        )
        
        logger.info(f"Entity linking completed in {processing_time:.3f}s with confidence {confidence_score:.3f}")
        return result
    
    def _extract_potential_entities(self, text: str) -> Dict[str, EntityType]:
        """Extract potential entities from text using patterns"""
        potential_entities = {}
        
        # Apply patterns for each entity type
        for entity_type, patterns in self.entity_patterns.items():
            for pattern in patterns:
                matches = re.findall(pattern, text, re.IGNORECASE)
                for match in matches:
                    # Handle tuple matches from complex patterns
                    if isinstance(match, tuple):
                        match = ' '.join(match).strip()
                    
                    if match and len(match.strip()) > 1:
                        entity_text = match.strip()
                        # If entity already found with different type, use most specific
                        if entity_text.lower() not in [e.lower() for e in potential_entities.keys()]:
                            potential_entities[entity_text] = entity_type
        
        # Also extract proper nouns as potential entities
        proper_nouns = re.findall(r'\b[A-Z][a-z]+(?:\s+[A-Z][a-z]+)*\b', text)
        for noun in proper_nouns:
            if noun.lower() not in [e.lower() for e in potential_entities.keys()]:
                # Try to infer type from context
                entity_type = self._infer_entity_type(noun, text)
                potential_entities[noun] = entity_type
        
        return potential_entities
    
    def _infer_entity_type(self, entity: str, context: str) -> EntityType:
        """Infer entity type from context"""
        context_lower = context.lower()
        entity_lower = entity.lower()
        
        # Check for type indicators in context
        for entity_type, indicators in self.type_indicators.items():
            for indicator in indicators:
                if indicator in context_lower:
                    return entity_type
        
        # Check for specific patterns around the entity
        entity_context = self._get_entity_context(entity, context, window=10)
        
        # Person indicators
        if any(word in entity_context for word in ['said', 'thinks', 'believes', 'mr', 'mrs', 'dr']):
            return EntityType.PERSON
        
        # Location indicators  
        if any(word in entity_context for word in ['in', 'at', 'from', 'to', 'city', 'country']):
            return EntityType.LOCATION
        
        # Default to unknown
        return EntityType.UNKNOWN
    
    def _get_entity_context(self, entity: str, text: str, window: int = 5) -> str:
        """Get context words around an entity"""
        words = text.split()
        entity_words = entity.split()
        
        # Find entity position
        for i in range(len(words) - len(entity_words) + 1):
            if ' '.join(words[i:i+len(entity_words)]).lower() == entity.lower():
                start = max(0, i - window)
                end = min(len(words), i + len(entity_words) + window)
                return ' '.join(words[start:end]).lower()
        
        return text.lower()
    
    def _find_uks_candidates(self, entity_text: str, entity_type: EntityType, 
                           context: Dict[str, Any]) -> List[EntityCandidate]:
        """Find candidate entities in UKS"""
        candidates = []
        
        # Check cache first
        cache_key = f"{entity_text.lower()}:{entity_type.value}"
        if cache_key in self.entity_cache:
            return self.entity_cache[cache_key]
        
        try:
            # Exact match search
            exact_matches = self._search_uks_exact(entity_text)
            for match in exact_matches:
                candidate = self._create_entity_candidate(
                    match, entity_type, entity_text, context, match_type="exact"
                )
                candidates.append(candidate)
            
            # Fuzzy match search if no exact matches
            if not exact_matches:
                fuzzy_matches = self._search_uks_fuzzy(entity_text, threshold=0.8)
                for match in fuzzy_matches:
                    candidate = self._create_entity_candidate(
                        match, entity_type, entity_text, context, match_type="fuzzy"
                    )
                    candidates.append(candidate)
            
            # Sort by overall score
            candidates.sort(key=lambda x: x.overall_score, reverse=True)
            
            # Cache results
            self.entity_cache[cache_key] = candidates[:5]  # Keep top 5
            
        except Exception as e:
            logger.warning(f"Error finding UKS candidates for '{entity_text}': {e}")
        
        return candidates
    
    def _search_uks_exact(self, entity_text: str) -> List[Dict[str, Any]]:
        """Search UKS for exact entity matches"""
        if not self.uks_interface:
            # Simulate UKS response for testing
            return self._simulate_uks_search(entity_text, exact=True)
        
        # Real UKS interface call would go here
        try:
            results = self.uks_interface.search_entities_exact(entity_text)
            return results
        except Exception as e:
            logger.warning(f"UKS exact search failed: {e}")
            return []
    
    def _search_uks_fuzzy(self, entity_text: str, threshold: float = 0.8) -> List[Dict[str, Any]]:
        """Search UKS for fuzzy entity matches"""
        if not self.uks_interface:
            # Simulate UKS response for testing
            return self._simulate_uks_search(entity_text, exact=False)
        
        # Real UKS interface call would go here
        try:
            results = self.uks_interface.search_entities_fuzzy(entity_text, threshold)
            return results
        except Exception as e:
            logger.warning(f"UKS fuzzy search failed: {e}")
            return []
    
    def _simulate_uks_search(self, entity_text: str, exact: bool = True) -> List[Dict[str, Any]]:
        """Simulate UKS search results for testing"""
        entity_lower = entity_text.lower()
        
        # Simulate some known entities
        simulated_entities = {
            'paris': [
                {'name': 'Paris, France', 'type': 'location', 'relationships': 15, 'frequency': 100},
                {'name': 'Paris, Texas', 'type': 'location', 'relationships': 5, 'frequency': 10},
                {'name': 'Paris Hilton', 'type': 'person', 'relationships': 8, 'frequency': 25}
            ],
            'john': [
                {'name': 'John Smith', 'type': 'person', 'relationships': 12, 'frequency': 50},
                {'name': 'John Doe', 'type': 'person', 'relationships': 3, 'frequency': 15}
            ],
            'apple': [
                {'name': 'Apple Inc.', 'type': 'organization', 'relationships': 20, 'frequency': 80},
                {'name': 'apple fruit', 'type': 'concept', 'relationships': 10, 'frequency': 30}
            ]
        }
        
        if exact and entity_lower in simulated_entities:
            return simulated_entities[entity_lower]
        elif not exact:
            # Fuzzy matching simulation
            matches = []
            for key, entities in simulated_entities.items():
                similarity = difflib.SequenceMatcher(None, entity_lower, key).ratio()
                if similarity > 0.6:
                    for entity in entities:
                        entity['similarity'] = similarity
                        matches.append(entity)
            return matches
        
        return []
    
    def _create_entity_candidate(self, uks_match: Dict[str, Any], entity_type: EntityType,
                               original_text: str, context: Dict[str, Any], 
                               match_type: str) -> EntityCandidate:
        """Create an EntityCandidate from UKS match"""
        
        # Calculate confidence based on match type and similarity
        base_confidence = 1.0 if match_type == "exact" else uks_match.get('similarity', 0.8)
        
        # Calculate relationship strength
        relationship_count = uks_match.get('relationships', 0)
        relationship_strength = min(1.0, relationship_count / 20.0)  # Normalize to 0-1
        
        # Calculate frequency score
        frequency = uks_match.get('frequency', 1)
        frequency_score = min(1.0, frequency / 100.0)  # Normalize to 0-1
        
        # Extract context matches
        context_matches = self._find_context_matches(original_text, uks_match, context)
        
        # Disambiguation features
        disambiguation_features = {
            'match_type': match_type,
            'uks_type': uks_match.get('type', 'unknown'),
            'relationship_count': relationship_count,
            'frequency': frequency,
            'context_indicators': context_matches
        }
        
        return EntityCandidate(
            uks_name=uks_match['name'],
            entity_type=EntityType(uks_match.get('type', 'unknown')) if uks_match.get('type') in [e.value for e in EntityType] else EntityType.UNKNOWN,
            confidence_score=base_confidence,
            disambiguation_features=disambiguation_features,
            context_matches=context_matches,
            relationship_strength=relationship_strength,
            frequency_score=frequency_score
        )
    
    def _find_context_matches(self, original_text: str, uks_match: Dict[str, Any], 
                            context: Dict[str, Any]) -> List[str]:
        """Find context clues that support this entity match"""
        matches = []
        
        # Check if entity type matches expected type from context
        entity_type = uks_match.get('type', '')
        if entity_type in original_text.lower():
            matches.append(f"type_mention:{entity_type}")
        
        # Check for related entities mentioned in context
        if 'related_entities' in context:
            for related in context['related_entities']:
                if related.lower() in original_text.lower():
                    matches.append(f"related_entity:{related}")
        
        return matches
    
    def _disambiguate_entity(self, entity_text: str, candidates: List[EntityCandidate],
                           full_text: str, context: Dict[str, Any]) -> Tuple[EntityCandidate, Dict[str, Any]]:
        """Disambiguate between multiple entity candidates"""
        
        if len(candidates) == 1:
            return candidates[0], {
                'entity': entity_text,
                'strategy': 'single_candidate',
                'confidence': candidates[0].overall_score,
                'alternatives': 0
            }
        
        # Apply disambiguation strategies
        scores = []
        for candidate in candidates:
            score = self._calculate_disambiguation_score(candidate, entity_text, full_text, context)
            scores.append((candidate, score))
        
        # Sort by score
        scores.sort(key=lambda x: x[1], reverse=True)
        best_candidate, best_score = scores[0]
        
        # Create decision record
        decision = {
            'entity': entity_text,
            'strategy': 'hybrid_scoring',
            'chosen': best_candidate.uks_name,
            'confidence': best_score,
            'alternatives': len(candidates) - 1,
            'score_breakdown': {
                'context_similarity': best_candidate.confidence_score,
                'relationship_strength': best_candidate.relationship_strength,
                'frequency_score': best_candidate.frequency_score
            }
        }
        
        return best_candidate, decision
    
    def _calculate_disambiguation_score(self, candidate: EntityCandidate, entity_text: str,
                                      full_text: str, context: Dict[str, Any]) -> float:
        """Calculate disambiguation score for a candidate"""
        
        # Context similarity (check if type matches context indicators)
        context_similarity = 0.5  # Base score
        entity_context = self._get_entity_context(entity_text, full_text)
        
        type_indicators = self.type_indicators.get(candidate.entity_type, [])
        for indicator in type_indicators:
            if indicator in entity_context:
                context_similarity += 0.1
        
        context_similarity = min(1.0, context_similarity)
        
        # Type consistency (does the candidate type match inferred type?)
        inferred_type = self._infer_entity_type(entity_text, full_text)
        type_consistency = 1.0 if candidate.entity_type == inferred_type else 0.5
        
        # Weighted combination
        final_score = (
            context_similarity * self.disambiguation_weights['context_similarity'] +
            candidate.relationship_strength * self.disambiguation_weights['relationship_strength'] +
            candidate.frequency_score * self.disambiguation_weights['frequency_score'] +
            type_consistency * self.disambiguation_weights['type_consistency']
        )
        
        return final_score
    
    def _create_candidate_beliefs(self, text: str, linked_entities: Dict[str, EntityCandidate],
                                context: Dict[str, Any]) -> List[CandidateBelief]:
        """Create candidate beliefs for new knowledge"""
        candidate_beliefs = []
        
        # Extract relationships from text
        relationships = self._extract_relationships(text, linked_entities)
        
        for subj, pred, obj in relationships:
            # Check if this is new knowledge
            if self._is_new_knowledge(subj, pred, obj):
                # Determine confidence based on entity linking confidence
                subject_conf = linked_entities.get(subj, type('obj', (object,), {'overall_score': 0.5})).overall_score
                object_conf = linked_entities.get(obj, type('obj', (object,), {'overall_score': 0.5})).overall_score
                initial_confidence = min(subject_conf, object_conf) * 0.8  # Conservative
                
                belief = CandidateBelief(
                    subject=subj,
                    predicate=pred,
                    object=obj,
                    initial_confidence=initial_confidence,
                    source="natural_language_input",
                    evidence=[text],
                    requires_validation=initial_confidence < 0.7,
                    created_at=datetime.now(),
                    disambiguation_context={
                        'original_text': text,
                        'linked_entities': {k: v.uks_name for k, v in linked_entities.items()},
                        'context': context
                    }
                )
                candidate_beliefs.append(belief)
        
        return candidate_beliefs
    
    def _extract_relationships(self, text: str, linked_entities: Dict[str, EntityCandidate]) -> List[Tuple[str, str, str]]:
        """Extract relationships from text using linked entities"""
        relationships = []
        
        # Simple relationship patterns
        relationship_patterns = [
            (r'(\w+)\s+is\s+(?:a|an)?\s*(\w+)', 'is'),
            (r'(\w+)\s+has\s+(?:a|an)?\s*(\w+)', 'has'),
            (r'(\w+)\s+lives\s+in\s+(\w+)', 'livesIn'),
            (r'(\w+)\s+works\s+at\s+(\w+)', 'worksAt'),
            (r'(\w+)\s+knows\s+(\w+)', 'knows'),
            (r'(\w+)\s+likes\s+(\w+)', 'likes'),
            (r'(\w+)\s+born\s+in\s+(\w+)', 'bornIn'),
            (r'(\w+)\s+located\s+in\s+(\w+)', 'locatedIn'),
        ]
        
        for pattern, predicate in relationship_patterns:
            matches = re.findall(pattern, text, re.IGNORECASE)
            for match in matches:
                subject, obj = match
                # Check if both subject and object are linked entities
                if subject in linked_entities or obj in linked_entities:
                    relationships.append((subject, predicate, obj))
        
        return relationships
    
    def _is_new_knowledge(self, subject: str, predicate: str, obj: str) -> bool:
        """Check if this represents new knowledge not in UKS"""
        if not self.uks_interface:
            return True  # Assume new for testing
        
        # Check if relationship already exists in UKS
        try:
            exists = self.uks_interface.check_relationship_exists(subject, predicate, obj)
            return not exists
        except Exception as e:
            logger.warning(f"Error checking existing knowledge: {e}")
            return True  # Assume new if check fails
    
    def _calculate_linking_confidence(self, linked_entities: Dict[str, EntityCandidate],
                                    candidate_beliefs: List[CandidateBelief]) -> float:
        """Calculate overall confidence in the linking process"""
        if not linked_entities:
            return 0.0
        
        # Average confidence of linked entities
        entity_confidences = [entity.overall_score for entity in linked_entities.values()]
        avg_entity_confidence = sum(entity_confidences) / len(entity_confidences)
        
        # Penalty for low-confidence candidate beliefs
        belief_penalty = 0.0
        if candidate_beliefs:
            low_conf_beliefs = [b for b in candidate_beliefs if b.initial_confidence < 0.5]
            belief_penalty = len(low_conf_beliefs) / len(candidate_beliefs) * 0.2
        
        return max(0.0, avg_entity_confidence - belief_penalty)
    
    # Utility methods for integration and testing
    def get_entity_statistics(self) -> Dict[str, Any]:
        """Get statistics about entity linking performance"""
        return {
            'cache_size': len(self.entity_cache),
            'cached_entities': list(self.entity_cache.keys()),
            'disambiguation_weights': self.disambiguation_weights,
            'supported_entity_types': [e.value for e in EntityType]
        }
    
    def clear_cache(self):
        """Clear entity and relationship caches"""
        self.entity_cache.clear()
        self.relationship_cache.clear()
        logger.info("Entity linking caches cleared")
    
    def export_linking_results(self, results: List[LinkingResult], filepath: str):
        """Export linking results for analysis"""
        try:
            export_data = []
            for result in results:
                export_data.append({
                    'original_text': result.original_text,
                    'linked_entities': {k: {
                        'uks_name': v.uks_name,
                        'type': v.entity_type.value,
                        'confidence': v.overall_score
                    } for k, v in result.linked_entities.items()},
                    'candidate_beliefs': [{
                        'subject': b.subject,
                        'predicate': b.predicate,
                        'object': b.object,
                        'confidence': b.initial_confidence,
                        'requires_validation': b.requires_validation
                    } for b in result.candidate_beliefs],
                    'overall_confidence': result.confidence_score,
                    'processing_time': result.processing_time
                })
            
            with open(filepath, 'w') as f:
                json.dump(export_data, f, indent=2, default=str)
            
            logger.info(f"Linking results exported to {filepath}")
        except Exception as e:
            logger.error(f"Failed to export linking results: {e}")

# Testing and utility functions
def create_test_entity_linker():
    """Create a test instance of the entity linker"""
    return EntityLinker(uks_interface=None)  # No UKS for testing

def demo_entity_linking():
    """Demonstrate entity linking capabilities"""
    linker = create_test_entity_linker()
    
    test_texts = [
        "John lives in Paris and works at Apple",
        "Paris is the capital of France",
        "Dr. Smith teaches at Harvard University",
        "The concept of intelligence is complex",
        "Apple launched a new product in California"
    ]
    
    results = []
    for text in test_texts:
        print(f"\n--- Linking entities in: {text} ---")
        result = linker.link_entities(text)
        results.append(result)
        
        print(f"Linked entities ({len(result.linked_entities)}):")
        for entity_text, candidate in result.linked_entities.items():
            print(f"  '{entity_text}' -> '{candidate.uks_name}' ({candidate.entity_type.value}, {candidate.overall_score:.3f})")
        
        print(f"Candidate beliefs ({len(result.candidate_beliefs)}):")
        for belief in result.candidate_beliefs:
            print(f"  {belief.subject} {belief.predicate} {belief.object} (conf: {belief.initial_confidence:.3f})")
        
        print(f"Overall confidence: {result.confidence_score:.3f}")
    
    return results

if __name__ == "__main__":
    # Run demonstration
    demo_entity_linking()