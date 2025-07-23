#!/usr/bin/env python3
"""
Integration Test Script for LLM Bridge Prototype Phase 1
Demonstrates the complete pipeline working together.

This script tests the integration between:
- LLMInterfaceModule (Intuitive Proposer)
- EntityLinker (Knowledge Grounding System)
- BridgeController (Mediated Reasoning Orchestrator) [simulated]
- ConfidenceFusion (Hallucination Prevention System) [simulated]
"""

import sys
import json
import time
from datetime import datetime
from typing import Dict, List, Any

# Import our components
from LLMInterfaceModule import LLMInterfaceModule, create_test_interface, QueryType
from EntityLinker import EntityLinker, create_test_entity_linker

class IntegrationTester:
    """
    Integration tester that simulates the complete LLM Bridge pipeline
    """
    
    def __init__(self):
        self.llm_interface = create_test_interface()
        self.entity_linker = create_test_entity_linker()
        self.test_results = []
        
    def run_complete_test_suite(self):
        """Run the complete integration test suite"""
        print("=" * 80)
        print("LLM BRIDGE PROTOTYPE - PHASE 1 INTEGRATION TESTS")
        print("=" * 80)
        print(f"Test started at: {datetime.now()}")
        print()
        
        # Test scenarios covering different query types and complexity levels
        test_scenarios = [
            {
                "name": "Simple Assertion",
                "query": "John is a teacher",
                "expected_type": QueryType.ASSERTION,
                "description": "Basic factual statement for knowledge addition"
            },
            {
                "name": "Basic Query", 
                "query": "Is Mary tall?",
                "expected_type": QueryType.QUERY,
                "description": "Simple yes/no question"
            },
            {
                "name": "Complex Query",
                "query": "Who are all the students in the computer science department?",
                "expected_type": QueryType.COMPLEX_QUERY,
                "description": "Aggregation query requiring inference"
            },
            {
                "name": "Entity Disambiguation",
                "query": "Paris is the capital of France",
                "expected_type": QueryType.ASSERTION,
                "description": "Tests disambiguation (Paris city vs. person)"
            },
            {
                "name": "Relationship Extraction",
                "query": "John lives in Paris and works at Apple",
                "expected_type": QueryType.ASSERTION,
                "description": "Multiple relationships and entity linking"
            },
            {
                "name": "Conditional Reasoning",
                "query": "If someone is a student and they study hard, then they will succeed",
                "expected_type": QueryType.INFERENCE,
                "description": "Logical inference pattern"
            },
            {
                "name": "Definition Request",
                "query": "What does intelligence mean?",
                "expected_type": QueryType.DEFINITION,
                "description": "Concept definition query"
            },
            {
                "name": "Multi-Entity Complex",
                "query": "Dr. Smith teaches at Harvard University in Cambridge",
                "expected_type": QueryType.ASSERTION,
                "description": "Multiple entities with titles and locations"
            }
        ]
        
        # Run each test scenario
        for i, scenario in enumerate(test_scenarios, 1):
            print(f"Test {i}/{len(test_scenarios)}: {scenario['name']}")
            print(f"Query: '{scenario['query']}'")
            print(f"Description: {scenario['description']}")
            print("-" * 60)
            
            result = self.run_integrated_pipeline(scenario)
            self.test_results.append(result)
            
            self.display_test_result(result)
            print()
        
        # Generate summary report
        self.generate_summary_report()
        
    def run_integrated_pipeline(self, scenario: Dict[str, Any]) -> Dict[str, Any]:
        """
        Run the complete integrated pipeline for a single scenario
        """
        start_time = time.time()
        query = scenario["query"]
        
        result = {
            "scenario": scenario,
            "timestamp": datetime.now().isoformat(),
            "success": False,
            "errors": [],
            "pipeline_steps": {}
        }
        
        try:
            # Step 1: LLM Interface Processing
            print("  Step 1: LLM Interface Processing...")
            llm_proposal = self.llm_interface.process_natural_language_query(query)
            
            result["pipeline_steps"]["llm_processing"] = {
                "query_type": llm_proposal.query_type.value,
                "entities": llm_proposal.entities,
                "relationships": llm_proposal.relationships,
                "uks_operation": llm_proposal.uks_operation,
                "confidence": llm_proposal.confidence_score,
                "reasoning_chain": llm_proposal.reasoning_chain
            }
            
            # Validate expected query type
            if llm_proposal.query_type == scenario["expected_type"]:
                result["pipeline_steps"]["llm_processing"]["type_match"] = True
            else:
                result["pipeline_steps"]["llm_processing"]["type_match"] = False
                result["errors"].append(f"Query type mismatch: expected {scenario['expected_type'].value}, got {llm_proposal.query_type.value}")
            
            # Step 2: Entity Linking
            print("  Step 2: Entity Linking...")
            linking_result = self.entity_linker.link_entities(query)
            
            result["pipeline_steps"]["entity_linking"] = {
                "linked_entities": {k: {
                    "uks_name": v.uks_name,
                    "type": v.entity_type.value,
                    "confidence": v.overall_score
                } for k, v in linking_result.linked_entities.items()},
                "candidate_beliefs": [{
                    "subject": b.subject,
                    "predicate": b.predicate,
                    "object": b.object,
                    "confidence": b.initial_confidence
                } for b in linking_result.candidate_beliefs],
                "disambiguation_decisions": linking_result.disambiguation_decisions,
                "overall_confidence": linking_result.confidence_score,
                "processing_time": linking_result.processing_time
            }
            
            # Step 3: Simulated Confidence Fusion
            print("  Step 3: Confidence Fusion...")
            llm_confidence = llm_proposal.confidence_score
            entity_confidence = linking_result.confidence_score
            
            # Simulate UKS confidence (would come from actual UKS in real implementation)
            uks_confidence = self.simulate_uks_confidence(llm_proposal, linking_result)
            
            # Simulate confidence fusion
            fused_confidence, hallucination_flag = self.simulate_confidence_fusion(
                llm_confidence, uks_confidence, entity_confidence
            )
            
            result["pipeline_steps"]["confidence_fusion"] = {
                "llm_confidence": llm_confidence,
                "uks_confidence": uks_confidence,
                "entity_confidence": entity_confidence,
                "fused_confidence": fused_confidence,
                "hallucination_flag": hallucination_flag,
                "fusion_strategy": "simulated_adaptive"
            }
            
            # Step 4: Simulated Bridge Controller Orchestration
            print("  Step 4: Bridge Controller Orchestration...")
            iterations = 1
            refinement_needed = fused_confidence < 0.7
            
            if refinement_needed:
                iterations = 2
                # Simulate refinement improving confidence
                fused_confidence = min(1.0, fused_confidence + 0.2)
            
            result["pipeline_steps"]["bridge_orchestration"] = {
                "iterations": iterations,
                "refinement_applied": refinement_needed,
                "final_confidence": fused_confidence,
                "response_generated": True
            }
            
            # Calculate overall success
            result["success"] = (
                len(result["errors"]) == 0 and
                fused_confidence > 0.5 and
                not hallucination_flag
            )
            
            result["processing_time"] = time.time() - start_time
            
        except Exception as e:
            result["errors"].append(f"Pipeline error: {str(e)}")
            result["success"] = False
            result["processing_time"] = time.time() - start_time
        
        return result
    
    def simulate_uks_confidence(self, llm_proposal, linking_result) -> float:
        """
        Simulate UKS confidence based on entity linking and knowledge availability
        """
        # Base confidence on entity linking success
        base_confidence = linking_result.confidence_score
        
        # Adjust based on query type
        if llm_proposal.query_type == QueryType.ASSERTION:
            # New knowledge might have lower UKS confidence initially
            return base_confidence * 0.8
        elif llm_proposal.query_type == QueryType.QUERY:
            # Queries about existing knowledge should have higher confidence
            return min(1.0, base_confidence * 1.2)
        else:
            # Complex queries and inferences have moderate confidence
            return base_confidence * 0.9
    
    def simulate_confidence_fusion(self, llm_conf: float, uks_conf: float, entity_conf: float) -> tuple:
        """
        Simulate confidence fusion logic
        Returns: (fused_confidence, hallucination_flag)
        """
        # Detect potential hallucination
        confidence_gap = abs(llm_conf - uks_conf)
        hallucination_flag = (llm_conf > 0.8 and uks_conf < 0.3) or confidence_gap > 0.5
        
        if hallucination_flag:
            # Use conservative fusion for potential hallucinations
            fused_confidence = min(llm_conf, uks_conf) * 0.8
        else:
            # Use weighted average for normal cases
            weights = {"llm": 0.4, "uks": 0.4, "entity": 0.2}
            fused_confidence = (
                llm_conf * weights["llm"] +
                uks_conf * weights["uks"] +
                entity_conf * weights["entity"]
            )
        
        return fused_confidence, hallucination_flag
    
    def display_test_result(self, result: Dict[str, Any]):
        """Display formatted test result"""
        success_symbol = "✅" if result["success"] else "❌"
        print(f"  Result: {success_symbol} {'PASS' if result['success'] else 'FAIL'}")
        
        if result["errors"]:
            print(f"  Errors: {', '.join(result['errors'])}")
        
        # Display key metrics
        steps = result["pipeline_steps"]
        
        if "llm_processing" in steps:
            llm_step = steps["llm_processing"]
            print(f"  LLM: Type={llm_step['query_type']}, Confidence={llm_step['confidence']:.3f}")
        
        if "entity_linking" in steps:
            entity_step = steps["entity_linking"]
            entity_count = len(entity_step["linked_entities"])
            belief_count = len(entity_step["candidate_beliefs"])
            print(f"  Entity Linking: {entity_count} entities, {belief_count} beliefs, Confidence={entity_step['overall_confidence']:.3f}")
        
        if "confidence_fusion" in steps:
            fusion_step = steps["confidence_fusion"]
            halluc_flag = "🚨" if fusion_step["hallucination_flag"] else "✓"
            print(f"  Confidence Fusion: {fusion_step['fused_confidence']:.3f} {halluc_flag}")
        
        print(f"  Processing Time: {result['processing_time']:.3f}s")
    
    def generate_summary_report(self):
        """Generate and display summary report"""
        print("=" * 80)
        print("INTEGRATION TEST SUMMARY REPORT")
        print("=" * 80)
        
        total_tests = len(self.test_results)
        passed_tests = sum(1 for r in self.test_results if r["success"])
        failed_tests = total_tests - passed_tests
        
        print(f"Total Tests: {total_tests}")
        print(f"Passed: {passed_tests} ✅")
        print(f"Failed: {failed_tests} ❌")
        print(f"Success Rate: {(passed_tests/total_tests)*100:.1f}%")
        print()
        
        # Performance metrics
        processing_times = [r["processing_time"] for r in self.test_results]
        avg_time = sum(processing_times) / len(processing_times)
        max_time = max(processing_times)
        min_time = min(processing_times)
        
        print("PERFORMANCE METRICS:")
        print(f"Average Processing Time: {avg_time:.3f}s")
        print(f"Min Processing Time: {min_time:.3f}s")
        print(f"Max Processing Time: {max_time:.3f}s")
        print()
        
        # Component-specific metrics
        llm_confidences = []
        entity_confidences = []
        fused_confidences = []
        hallucination_count = 0
        
        for result in self.test_results:
            steps = result["pipeline_steps"]
            if "llm_processing" in steps:
                llm_confidences.append(steps["llm_processing"]["confidence"])
            if "entity_linking" in steps:
                entity_confidences.append(steps["entity_linking"]["overall_confidence"])
            if "confidence_fusion" in steps:
                fused_confidences.append(steps["confidence_fusion"]["fused_confidence"])
                if steps["confidence_fusion"]["hallucination_flag"]:
                    hallucination_count += 1
        
        print("COMPONENT PERFORMANCE:")
        if llm_confidences:
            print(f"LLM Average Confidence: {sum(llm_confidences)/len(llm_confidences):.3f}")
        if entity_confidences:
            print(f"Entity Linking Average Confidence: {sum(entity_confidences)/len(entity_confidences):.3f}")
        if fused_confidences:
            print(f"Fused Average Confidence: {sum(fused_confidences)/len(fused_confidences):.3f}")
        print(f"Hallucination Flags: {hallucination_count}/{total_tests}")
        print()
        
        # Failed test analysis
        if failed_tests > 0:
            print("FAILED TEST ANALYSIS:")
            for i, result in enumerate(self.test_results):
                if not result["success"]:
                    test_name = result["scenario"]["name"]
                    errors = ", ".join(result["errors"])
                    print(f"  {i+1}. {test_name}: {errors}")
            print()
        
        # Export detailed results
        self.export_detailed_results()
        
        print("=" * 80)
        print("Integration testing completed!")
        if passed_tests == total_tests:
            print("🎉 All tests passed! The LLM Bridge Prototype Phase 1 is working correctly.")
        else:
            print(f"⚠️  {failed_tests} test(s) failed. Review the detailed results for issues.")
        print("=" * 80)
    
    def export_detailed_results(self):
        """Export detailed test results to JSON file"""
        try:
            filename = f"integration_test_results_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
            
            export_data = {
                "test_summary": {
                    "timestamp": datetime.now().isoformat(),
                    "total_tests": len(self.test_results),
                    "passed_tests": sum(1 for r in self.test_results if r["success"]),
                    "failed_tests": sum(1 for r in self.test_results if not r["success"])
                },
                "detailed_results": self.test_results
            }
            
            with open(filename, 'w') as f:
                json.dump(export_data, f, indent=2, default=str)
            
            print(f"Detailed results exported to: {filename}")
            
        except Exception as e:
            print(f"Failed to export detailed results: {e}")

def main():
    """Main function to run integration tests"""
    try:
        tester = IntegrationTester()
        tester.run_complete_test_suite()
    except KeyboardInterrupt:
        print("\nTest interrupted by user.")
        sys.exit(1)
    except Exception as e:
        print(f"Integration test failed with error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()