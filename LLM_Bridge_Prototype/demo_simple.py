#!/usr/bin/env python3
"""
Simple Demo Script for LLM Bridge Prototype Phase 1
Demonstrates core components without external dependencies.
"""

import sys
import re
from datetime import datetime
from typing import Dict, List, Tuple
from enum import Enum

# Simplified QueryType enum (copied from main module to avoid dependencies)
class QueryType(Enum):
    ASSERTION = "assertion"
    QUERY = "query"
    COMPLEX_QUERY = "complex_query"
    INFERENCE = "inference"
    DEFINITION = "definition"

class SimplifiedLLMInterface:
    """Simplified LLM Interface for demonstration without OpenAI dependency"""
    
    def process_query(self, text: str) -> Dict:
        """Process natural language query"""
        print(f"🧠 LLM Processing: '{text}'")
        
        # Simple query type classification
        text_lower = text.lower().strip()
        if text_lower.endswith('?'):
            if any(word in text_lower for word in ['who', 'what', 'where', 'when', 'how']):
                query_type = QueryType.COMPLEX_QUERY
            else:
                query_type = QueryType.QUERY
        elif 'if' in text_lower and ('then' in text_lower or 'would' in text_lower):
            query_type = QueryType.INFERENCE
        elif any(phrase in text_lower for phrase in ['what is', 'what does', 'define']):
            query_type = QueryType.DEFINITION
        else:
            query_type = QueryType.ASSERTION
        
        # Extract entities (simple approach)
        entities = re.findall(r'\b[A-Z][a-z]+\b', text)
        
        # Extract relationships
        relationships = []
        relationship_patterns = [
            (r'(\w+)\s+is\s+(\w+)', 'is'),
            (r'(\w+)\s+lives\s+in\s+(\w+)', 'livesIn'),
            (r'(\w+)\s+works\s+at\s+(\w+)', 'worksAt'),
        ]
        
        for pattern, predicate in relationship_patterns:
            matches = re.findall(pattern, text, re.IGNORECASE)
            for match in matches:
                relationships.append((match[0], predicate, match[1]))
        
        # Generate UKS operation
        if query_type == QueryType.ASSERTION and relationships:
            uks_operation = f"CreateBelief('{relationships[0][0]}', '{relationships[0][1]}', '{relationships[0][2]}')"
        elif query_type == QueryType.QUERY and entities:
            uks_operation = f"QueryEntity('{entities[0]}')"
        else:
            uks_operation = f"GeneralQuery('{text}')"
        
        # Calculate confidence (simplified)
        confidence = 0.8 if entities and (relationships or query_type == QueryType.QUERY) else 0.6
        
        result = {
            'query_type': query_type,
            'entities': entities,
            'relationships': relationships,
            'uks_operation': uks_operation,
            'confidence': confidence,
            'reasoning': [
                f"Identified query type: {query_type.value}",
                f"Found {len(entities)} entities: {', '.join(entities)}" if entities else "No entities found",
                f"Found {len(relationships)} relationships" if relationships else "No relationships found",
                f"Generated UKS operation: {uks_operation}"
            ]
        }
        
        print(f"   ├─ Query Type: {query_type.value}")
        print(f"   ├─ Entities: {entities}")
        print(f"   ├─ UKS Operation: {uks_operation}")
        print(f"   └─ Confidence: {confidence:.3f}")
        
        return result

class SimplifiedEntityLinker:
    """Simplified Entity Linker for demonstration"""
    
    def __init__(self):
        # Simulated knowledge base
        self.known_entities = {
            'john': [{'name': 'John Smith', 'type': 'person', 'confidence': 0.9}],
            'mary': [{'name': 'Mary Johnson', 'type': 'person', 'confidence': 0.8}],
            'paris': [
                {'name': 'Paris, France', 'type': 'location', 'confidence': 0.95},
                {'name': 'Paris Hilton', 'type': 'person', 'confidence': 0.7}
            ],
            'apple': [
                {'name': 'Apple Inc.', 'type': 'organization', 'confidence': 0.9},
                {'name': 'apple fruit', 'type': 'concept', 'confidence': 0.6}
            ]
        }
    
    def link_entities(self, text: str, extracted_entities: List[str]) -> Dict:
        """Link entities to knowledge base"""
        print(f"🔗 Entity Linking: {extracted_entities}")
        
        linked_entities = {}
        candidate_beliefs = []
        
        for entity in extracted_entities:
            entity_lower = entity.lower()
            if entity_lower in self.known_entities:
                candidates = self.known_entities[entity_lower]
                
                # Simple disambiguation - pick highest confidence
                best_candidate = max(candidates, key=lambda x: x['confidence'])
                linked_entities[entity] = best_candidate
                
                print(f"   ├─ '{entity}' → '{best_candidate['name']}' ({best_candidate['type']})")
            else:
                # Create candidate belief for new entity
                candidate_beliefs.append({
                    'subject': entity,
                    'predicate': 'is',
                    'object': 'unknown',
                    'confidence': 0.5
                })
                print(f"   ├─ '{entity}' → New entity (candidate)")
        
        overall_confidence = sum(e['confidence'] for e in linked_entities.values()) / max(len(linked_entities), 1)
        
        result = {
            'linked_entities': linked_entities,
            'candidate_beliefs': candidate_beliefs,
            'confidence': overall_confidence
        }
        
        print(f"   └─ Overall Confidence: {overall_confidence:.3f}")
        return result

class SimplifiedConfidenceFusion:
    """Simplified Confidence Fusion for demonstration"""
    
    def fuse_confidences(self, llm_conf: float, uks_conf: float, entity_conf: float) -> Dict:
        """Fuse multiple confidence sources"""
        print(f"⚖️  Confidence Fusion: LLM={llm_conf:.3f}, UKS={uks_conf:.3f}, Entity={entity_conf:.3f}")
        
        # Detect potential hallucination
        confidence_gap = abs(llm_conf - uks_conf)
        hallucination_flag = (llm_conf > 0.8 and uks_conf < 0.3) or confidence_gap > 0.5
        
        if hallucination_flag:
            # Conservative fusion
            fused_confidence = min(llm_conf, uks_conf) * 0.8
            strategy = "conservative (hallucination detected)"
            print(f"   ⚠️  Potential hallucination detected! Confidence gap: {confidence_gap:.3f}")
        else:
            # Weighted average
            weights = {'llm': 0.4, 'uks': 0.4, 'entity': 0.2}
            fused_confidence = (
                llm_conf * weights['llm'] +
                uks_conf * weights['uks'] +
                entity_conf * weights['entity']
            )
            strategy = "weighted_average"
        
        result = {
            'fused_confidence': fused_confidence,
            'hallucination_flag': hallucination_flag,
            'strategy': strategy,
            'confidence_gap': confidence_gap
        }
        
        status_symbol = "🚨" if hallucination_flag else "✅"
        print(f"   ├─ Strategy: {strategy}")
        print(f"   ├─ Fused Confidence: {fused_confidence:.3f} {status_symbol}")
        print(f"   └─ Hallucination Flag: {hallucination_flag}")
        
        return result

class SimplifiedBridgeController:
    """Simplified Bridge Controller orchestrating the complete pipeline"""
    
    def __init__(self):
        self.llm_interface = SimplifiedLLMInterface()
        self.entity_linker = SimplifiedEntityLinker()
        self.confidence_fusion = SimplifiedConfidenceFusion()
    
    def process_query(self, query: str) -> Dict:
        """Process query through complete pipeline"""
        print(f"\n{'='*60}")
        print(f"🚀 Processing Query: '{query}'")
        print(f"{'='*60}")
        
        # Step 1: LLM Processing
        llm_result = self.llm_interface.process_query(query)
        
        # Step 2: Entity Linking
        entity_result = self.entity_linker.link_entities(query, llm_result['entities'])
        
        # Step 3: Simulate UKS Response
        print(f"🗃️  UKS Processing: {llm_result['uks_operation']}")
        # Simulate UKS confidence based on entity linking success
        uks_confidence = entity_result['confidence'] * 0.9
        print(f"   └─ UKS Confidence: {uks_confidence:.3f}")
        
        # Step 4: Confidence Fusion
        fusion_result = self.confidence_fusion.fuse_confidences(
            llm_result['confidence'],
            uks_confidence,
            entity_result['confidence']
        )
        
        # Step 5: Generate Response
        print(f"💬 Response Generation:")
        if fusion_result['fused_confidence'] > 0.7:
            response_quality = "High confidence response"
            response = f"✅ Based on my analysis with {fusion_result['fused_confidence']:.1%} confidence"
        elif fusion_result['hallucination_flag']:
            response_quality = "Potential hallucination detected"
            response = f"⚠️  Low confidence ({fusion_result['fused_confidence']:.1%}) - may require validation"
        else:
            response_quality = "Moderate confidence response"
            response = f"📊 Moderate confidence ({fusion_result['fused_confidence']:.1%}) response"
        
        print(f"   ├─ Quality: {response_quality}")
        print(f"   └─ Response: {response}")
        
        return {
            'query': query,
            'llm_result': llm_result,
            'entity_result': entity_result,
            'fusion_result': fusion_result,
            'response': response,
            'success': not fusion_result['hallucination_flag'] and fusion_result['fused_confidence'] > 0.5
        }

def run_demo():
    """Run the complete demonstration"""
    print("🎯 LLM BRIDGE PROTOTYPE - PHASE 1 DEMONSTRATION")
    print("=" * 80)
    print("This demo shows the core components working together:")
    print("├─ 🧠 LLM Interface Module (Intuitive Proposer)")
    print("├─ 🔗 Entity Linker (Knowledge Grounding)")
    print("├─ ⚖️  Confidence Fusion (Hallucination Prevention)")
    print("└─ 🚀 Bridge Controller (Mediated Reasoning Orchestrator)")
    print()
    
    controller = SimplifiedBridgeController()
    
    # Test scenarios
    test_queries = [
        "John is a teacher",
        "Mary lives in Paris",
        "Is John tall?",
        "Apple makes computers",
        "Who are all the students?",
        "What does intelligence mean?"
    ]
    
    results = []
    
    for i, query in enumerate(test_queries, 1):
        result = controller.process_query(query)
        results.append(result)
        
        if i < len(test_queries):
            input("\n⏸️  Press Enter to continue to next test...")
    
    # Summary
    print(f"\n{'='*60}")
    print("📊 DEMONSTRATION SUMMARY")
    print(f"{'='*60}")
    
    successful = sum(1 for r in results if r['success'])
    total = len(results)
    
    print(f"Total Queries Processed: {total}")
    print(f"Successful: {successful} ✅")
    print(f"Issues Detected: {total - successful} ⚠️")
    print(f"Success Rate: {(successful/total)*100:.1f}%")
    
    # Show confidence distribution
    confidences = [r['fusion_result']['fused_confidence'] for r in results]
    avg_confidence = sum(confidences) / len(confidences)
    print(f"Average Confidence: {avg_confidence:.3f}")
    
    hallucinations = sum(1 for r in results if r['fusion_result']['hallucination_flag'])
    print(f"Hallucination Flags: {hallucinations}/{total}")
    
    print(f"\n🎉 Phase 1 LLM Bridge Prototype demonstration completed!")
    print("Components are working together successfully and ready for integration with BrainSimY.")

if __name__ == "__main__":
    try:
        run_demo()
    except KeyboardInterrupt:
        print("\n\n⏹️  Demo stopped by user.")
        sys.exit(0)
    except Exception as e:
        print(f"\n❌ Demo failed: {e}")
        sys.exit(1)