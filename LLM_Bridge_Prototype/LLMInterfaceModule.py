"""
LLM Interface Module - Phase 1 Implementation
Implements Perplexity's "Intuitive Proposer" role in the neuro-symbolic architecture.

Based on research from:
- arXiv:2502.11269v1 (Neuro-symbolic AI hybrid systems)
- arXiv:2407.08516v1 (LLM_RAG techniques)
- Wei et al., 2022 (Chain-of-Thought prompting)
- arXiv:2310.12773 (LLM API best practices)
"""

import json
import re
import logging
from typing import Dict, List, Tuple, Optional, Any
from dataclasses import dataclass
from enum import Enum
import openai
from datetime import datetime

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class QueryType(Enum):
    """Types of queries the LLM can process"""
    ASSERTION = "assertion"          # "John is tall"
    QUERY = "query"                 # "Is John tall?"
    COMPLEX_QUERY = "complex_query" # "Who are all tall people?"
    INFERENCE = "inference"         # "If John is tall and tall people play basketball..."
    DEFINITION = "definition"       # "What does 'tall' mean?"

@dataclass
class LLMProposal:
    """Represents a proposal from the LLM with confidence metrics"""
    query_type: QueryType
    original_text: str
    uks_operation: str
    entities: List[str]
    relationships: List[Tuple[str, str, str]]  # (subject, predicate, object)
    confidence_score: float  # 0.0 to 1.0
    reasoning_chain: List[str]  # Chain-of-thought steps
    timestamp: datetime
    requires_validation: bool

class LLMInterfaceModule:
    """
    Main interface for LLM-UKS communication implementing the Intuitive Proposer pattern.
    Handles natural language understanding and preliminary hypothesis generation.
    """
    
    def __init__(self, api_key: str = None, model: str = "gpt-4"):
        """
        Initialize the LLM Interface Module
        
        Args:
            api_key: OpenAI API key (if None, expects OPENAI_API_KEY env var)
            model: Model to use for LLM operations
        """
        self.model = model
        if api_key:
            openai.api_key = api_key
        
        # Entity recognition patterns for basic disambiguation
        self.entity_patterns = {
            'person': r'\b([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\b(?=\s+(?:is|was|has|have|will|can|could|should))',
            'location': r'\b(?:in|at|from|to)\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)\b',
            'concept': r'\b(?:the concept of|the idea of|meaning of)\s+([a-z]+(?:\s+[a-z]+)*)\b'
        }
        
        # Confidence scoring weights
        self.confidence_weights = {
            'entity_clarity': 0.3,
            'relationship_clarity': 0.3,
            'logical_consistency': 0.2,
            'specificity': 0.2
        }
    
    def process_natural_language_query(self, text: str) -> LLMProposal:
        """
        Main entry point for processing natural language queries.
        Implements Chain-of-Thought reasoning for better UKS translation.
        
        Args:
            text: Natural language input
            
        Returns:
            LLMProposal with UKS operation and confidence metrics
        """
        logger.info(f"Processing query: {text}")
        
        # Step 1: Query type classification
        query_type = self._classify_query_type(text)
        
        # Step 2: Entity extraction and disambiguation
        entities = self._extract_entities(text)
        
        # Step 3: Relationship extraction
        relationships = self._extract_relationships(text, entities)
        
        # Step 4: Generate reasoning chain using Chain-of-Thought
        reasoning_chain = self._generate_reasoning_chain(text, query_type, entities, relationships)
        
        # Step 5: Translate to UKS operation
        uks_operation = self._translate_to_uks_operation(text, query_type, entities, relationships)
        
        # Step 6: Calculate confidence score
        confidence_score = self._calculate_confidence(text, entities, relationships, reasoning_chain)
        
        # Step 7: Determine if validation is required
        requires_validation = confidence_score < 0.7 or query_type == QueryType.COMPLEX_QUERY
        
        proposal = LLMProposal(
            query_type=query_type,
            original_text=text,
            uks_operation=uks_operation,
            entities=entities,
            relationships=relationships,
            confidence_score=confidence_score,
            reasoning_chain=reasoning_chain,
            timestamp=datetime.now(),
            requires_validation=requires_validation
        )
        
        logger.info(f"Generated proposal with confidence: {confidence_score:.3f}")
        return proposal
    
    def _classify_query_type(self, text: str) -> QueryType:
        """Classify the type of query using pattern matching and LLM assistance"""
        text_lower = text.lower().strip()
        
        # Simple pattern-based classification
        if text_lower.endswith('?'):
            if any(word in text_lower for word in ['who', 'what', 'where', 'when', 'how', 'which']):
                return QueryType.COMPLEX_QUERY
            else:
                return QueryType.QUERY
        elif 'if' in text_lower and ('then' in text_lower or 'would' in text_lower):
            return QueryType.INFERENCE
        elif any(phrase in text_lower for phrase in ['what is', 'what does', 'define', 'meaning of']):
            return QueryType.DEFINITION
        else:
            return QueryType.ASSERTION
    
    def _extract_entities(self, text: str) -> List[str]:
        """Extract entities using pattern matching and basic NER"""
        entities = []
        
        # Apply entity patterns
        for entity_type, pattern in self.entity_patterns.items():
            matches = re.findall(pattern, text, re.IGNORECASE)
            entities.extend(matches)
        
        # Additional simple entity extraction for proper nouns
        proper_nouns = re.findall(r'\b[A-Z][a-z]+\b', text)
        entities.extend(proper_nouns)
        
        # Remove duplicates while preserving order
        seen = set()
        unique_entities = []
        for entity in entities:
            if entity.lower() not in seen:
                seen.add(entity.lower())
                unique_entities.append(entity)
        
        return unique_entities
    
    def _extract_relationships(self, text: str, entities: List[str]) -> List[Tuple[str, str, str]]:
        """Extract relationships between entities"""
        relationships = []
        
        # Simple relationship patterns
        relationship_patterns = [
            (r'(\w+)\s+is\s+(\w+)', 'is'),
            (r'(\w+)\s+has\s+(\w+)', 'has'),
            (r'(\w+)\s+lives\s+in\s+(\w+)', 'livesIn'),
            (r'(\w+)\s+works\s+at\s+(\w+)', 'worksAt'),
            (r'(\w+)\s+knows\s+(\w+)', 'knows'),
            (r'(\w+)\s+likes\s+(\w+)', 'likes'),
        ]
        
        for pattern, predicate in relationship_patterns:
            matches = re.findall(pattern, text, re.IGNORECASE)
            for match in matches:
                subject, obj = match
                relationships.append((subject, predicate, obj))
        
        return relationships
    
    def _generate_reasoning_chain(self, text: str, query_type: QueryType, 
                                entities: List[str], relationships: List[Tuple[str, str, str]]) -> List[str]:
        """Generate Chain-of-Thought reasoning steps"""
        reasoning_chain = []
        
        reasoning_chain.append(f"1. Identified query type: {query_type.value}")
        
        if entities:
            reasoning_chain.append(f"2. Extracted entities: {', '.join(entities)}")
        
        if relationships:
            rel_strings = [f"{s} {p} {o}" for s, p, o in relationships]
            reasoning_chain.append(f"3. Identified relationships: {'; '.join(rel_strings)}")
        
        # Add query-specific reasoning
        if query_type == QueryType.ASSERTION:
            reasoning_chain.append("4. This appears to be a factual statement to be added to knowledge base")
        elif query_type == QueryType.QUERY:
            reasoning_chain.append("4. This is a question that requires knowledge retrieval")
        elif query_type == QueryType.COMPLEX_QUERY:
            reasoning_chain.append("4. This is a complex query requiring inference or aggregation")
        elif query_type == QueryType.INFERENCE:
            reasoning_chain.append("4. This involves logical reasoning over existing knowledge")
        elif query_type == QueryType.DEFINITION:
            reasoning_chain.append("4. This requests the definition or meaning of a concept")
        
        reasoning_chain.append("5. Translating to appropriate UKS operation")
        
        return reasoning_chain
    
    def _translate_to_uks_operation(self, text: str, query_type: QueryType, 
                                  entities: List[str], relationships: List[Tuple[str, str, str]]) -> str:
        """Translate natural language to UKS operation format"""
        
        if query_type == QueryType.ASSERTION:
            if relationships:
                # Create belief assertions
                operations = []
                for subj, pred, obj in relationships:
                    operations.append(f"CreateBelief('{subj}', '{pred}', '{obj}', confidence=0.8)")
                return "; ".join(operations)
            else:
                # Simple entity assertion
                if entities:
                    return f"CreateEntity('{entities[0]}', type='unknown')"
        
        elif query_type == QueryType.QUERY:
            if relationships:
                subj, pred, obj = relationships[0]
                return f"QueryBelief('{subj}', '{pred}', '{obj}')"
            elif entities:
                return f"QueryEntity('{entities[0]}')"
        
        elif query_type == QueryType.COMPLEX_QUERY:
            # For complex queries, we'll need inference
            if entities:
                return f"InferenceQuery(entities={entities}, relationships={relationships})"
        
        elif query_type == QueryType.DEFINITION:
            if entities:
                return f"GetDefinition('{entities[0]}')"
        
        # Fallback
        return f"GeneralQuery('{text}')"
    
    def _calculate_confidence(self, text: str, entities: List[str], 
                            relationships: List[Tuple[str, str, str]], 
                            reasoning_chain: List[str]) -> float:
        """Calculate confidence score based on multiple factors"""
        
        # Entity clarity: How well-defined are the entities?
        entity_clarity = min(1.0, len(entities) * 0.3) if entities else 0.0
        
        # Relationship clarity: How clear are the relationships?
        relationship_clarity = min(1.0, len(relationships) * 0.4) if relationships else 0.0
        
        # Logical consistency: Basic checks for contradictions
        logical_consistency = 1.0  # Start high, reduce for issues
        if any(word in text.lower() for word in ['maybe', 'perhaps', 'might', 'possibly']):
            logical_consistency -= 0.3
        
        # Specificity: More specific queries get higher confidence
        specificity = 1.0 - (text.count('?') > 1) * 0.2  # Penalize multiple questions
        specificity -= text.lower().count('or') * 0.1  # Penalize ambiguity
        
        # Weighted combination
        confidence = (
            entity_clarity * self.confidence_weights['entity_clarity'] +
            relationship_clarity * self.confidence_weights['relationship_clarity'] +
            logical_consistency * self.confidence_weights['logical_consistency'] +
            specificity * self.confidence_weights['specificity']
        )
        
        return max(0.0, min(1.0, confidence))
    
    def enhance_with_llm_analysis(self, proposal: LLMProposal) -> LLMProposal:
        """
        Enhance the proposal using LLM analysis for complex cases.
        This method can be called when initial confidence is low.
        """
        try:
            prompt = self._create_enhancement_prompt(proposal)
            
            response = openai.ChatCompletion.create(
                model=self.model,
                messages=[
                    {"role": "system", "content": "You are an expert in knowledge representation and reasoning."},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.1,
                max_tokens=500
            )
            
            enhanced_analysis = response.choices[0].message.content
            proposal.reasoning_chain.append(f"LLM Enhancement: {enhanced_analysis}")
            
            # Potentially adjust confidence based on LLM analysis
            if "high confidence" in enhanced_analysis.lower():
                proposal.confidence_score = min(1.0, proposal.confidence_score + 0.2)
            elif "low confidence" in enhanced_analysis.lower():
                proposal.confidence_score = max(0.0, proposal.confidence_score - 0.2)
            
        except Exception as e:
            logger.warning(f"LLM enhancement failed: {e}")
            proposal.reasoning_chain.append(f"LLM Enhancement failed: {str(e)}")
        
        return proposal
    
    def _create_enhancement_prompt(self, proposal: LLMProposal) -> str:
        """Create a prompt for LLM enhancement of the proposal"""
        return f"""
        Analyze this natural language to knowledge representation translation:
        
        Original text: "{proposal.original_text}"
        Query type: {proposal.query_type.value}
        Extracted entities: {proposal.entities}
        Extracted relationships: {proposal.relationships}
        UKS operation: {proposal.uks_operation}
        Current confidence: {proposal.confidence_score:.3f}
        
        Please evaluate:
        1. Are the entities correctly identified?
        2. Are the relationships accurately captured?
        3. Is the UKS operation appropriate?
        4. What's your confidence in this translation?
        5. Any suggestions for improvement?
        
        Provide a brief analysis with your confidence assessment.
        """

# Utility functions for testing and integration
def create_test_interface() -> LLMInterfaceModule:
    """Create a test instance of the LLM interface"""
    return LLMInterfaceModule(model="gpt-3.5-turbo")  # Use cheaper model for testing

def demo_queries() -> List[str]:
    """Sample queries for testing the interface"""
    return [
        "John is tall",
        "Is Mary a teacher?",
        "Who are all the students in the computer science department?",
        "If someone is a student and they study hard, then they will succeed",
        "What does 'intelligence' mean?",
        "Paris is the capital of France",
        "Does Alice know Bob?"
    ]

if __name__ == "__main__":
    # Quick test of the module
    interface = create_test_interface()
    
    test_queries = [
        "John is a teacher",
        "Is Mary tall?",
        "What does happiness mean?"
    ]
    
    for query in test_queries:
        print(f"\n--- Processing: {query} ---")
        proposal = interface.process_natural_language_query(query)
        print(f"Query Type: {proposal.query_type.value}")
        print(f"Entities: {proposal.entities}")
        print(f"UKS Operation: {proposal.uks_operation}")
        print(f"Confidence: {proposal.confidence_score:.3f}")
        print(f"Reasoning: {'; '.join(proposal.reasoning_chain)}")