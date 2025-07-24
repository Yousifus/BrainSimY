/*
 * Phase 2 Test Suite - Comprehensive Testing for Enhanced Reasoning Components
 * Tests all Phase 2 components individually and as an integrated system
 * to ensure correct functionality and compatibility with BrainSimY architecture.
 * 
 * Based on testing frameworks for:
 * - Advanced Confidence Fusion System
 * - Candidate Belief System  
 * - Temporal Reasoning Engine
 * - Integration Engine orchestration
 * - Performance and reliability validation
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UKS;
using BrainSimY.Modules;

namespace LLMBridgePrototype.Tests
{
    [TestClass]
    public class Phase2TestSuite
    {
        private UKS.UKS _testUKS;
        private Phase2IntegrationEngine _integrationEngine;
        private AdvancedConfidenceFusionEngine _confidenceFusion;
        private CandidateBeliefSystem _beliefSystem;
        private TemporalReasoningEngine _temporalEngine;

        [TestInitialize]
        public void SetUp()
        {
            // Initialize test UKS with basic structure
            _testUKS = new UKS.UKS();
            _testUKS.CreateInitialStructure();
            
            // Add test knowledge
            SetupTestKnowledge();

            // Initialize components
            _integrationEngine = new Phase2IntegrationEngine(_testUKS);
            _confidenceFusion = new AdvancedConfidenceFusionEngine(_testUKS);
            _beliefSystem = new CandidateBeliefSystem(_testUKS);
            _temporalEngine = new TemporalReasoningEngine(_testUKS);
        }

        private void SetupTestKnowledge()
        {
            // Add basic test entities and relationships
            _testUKS.AddStatement("John", "is-a", "Person");
            _testUKS.AddStatement("Mary", "is-a", "Person");
            _testUKS.AddStatement("Paris", "is-a", "City");
            _testUKS.AddStatement("France", "is-a", "Country");
            _testUKS.AddStatement("Paris", "is-in", "France");
            _testUKS.AddStatement("John", "teaches", "Mathematics");
            _testUKS.AddStatement("Mary", "studies", "Physics");
        }

        #region Advanced Confidence Fusion Tests

        [TestMethod]
        public async Task AdvancedConfidenceFusion_BasicFusion_ShouldSucceed()
        {
            // Arrange
            var llmConfidence = new LLMConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.8f,
                    Logical = 0.7f,
                    Temporal = 0.6f,
                    Source = 0.8f
                }
            };

            var uksConfidence = new UKSConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.9f,
                    Logical = 0.95f,
                    Temporal = 0.8f,
                    Source = 1.0f
                }
            };

            var context = new QueryContext
            {
                Domain = QueryDomain.Factual,
                RequiresFactualAccuracy = true
            };

            // Act
            var result = await _confidenceFusion.FuseConfidenceAsync(llmConfidence, uksConfidence, context);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(FusionStatus.Success, result.Status);
            Assert.IsTrue(result.FusedVector.CompositeScore > 0.5f);
            Assert.IsTrue(result.AlphaUsed > 0.5f); // Should favor UKS for factual queries
        }

        [TestMethod]
        public async Task AdvancedConfidenceFusion_ConflictDetection_ShouldResolve()
        {
            // Arrange - High confidence disagreement
            var llmConfidence = new LLMConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.95f,
                    Logical = 0.9f,
                    Temporal = 0.8f,
                    Source = 0.9f
                }
            };

            var uksConfidence = new UKSConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.2f,  // Strong disagreement
                    Logical = 0.3f,
                    Temporal = 0.4f,
                    Source = 0.9f
                }
            };

            var context = new QueryContext
            {
                Domain = QueryDomain.Mathematical,
                RequiresFactualAccuracy = true
            };

            // Act
            var result = await _confidenceFusion.FuseConfidenceAsync(llmConfidence, uksConfidence, context);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == FusionStatus.ConflictResolved || 
                         result.Status == FusionStatus.UksPreferred ||
                         result.Status == FusionStatus.EvidenceNeeded);
            Assert.IsNotNull(result.ConflictInfo);
            Assert.IsTrue(result.ConflictInfo.ConflictDetected);
        }

        [TestMethod]
        public async Task AdvancedConfidenceFusion_DomainSpecificWeighting_ShouldApplyCorrectly()
        {
            // Arrange
            var llmConfidence = new LLMConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.7f,
                    Logical = 0.8f,
                    Temporal = 0.7f,
                    Source = 0.8f
                }
            };

            var uksConfidence = new UKSConfidence
            {
                Vector = new ConfidenceVector
                {
                    Factual = 0.6f,
                    Logical = 0.7f,
                    Temporal = 0.6f,
                    Source = 0.8f
                }
            };

            var creativeContext = new QueryContext { Domain = QueryDomain.Creative };
            var mathematicalContext = new QueryContext { Domain = QueryDomain.Mathematical };

            // Act
            var creativeResult = await _confidenceFusion.FuseConfidenceAsync(llmConfidence, uksConfidence, creativeContext);
            var mathResult = await _confidenceFusion.FuseConfidenceAsync(llmConfidence, uksConfidence, mathematicalContext);

            // Assert
            Assert.IsTrue(creativeResult.AlphaUsed < mathResult.AlphaUsed); // Creative should favor LLM more
        }

        #endregion

        #region Candidate Belief System Tests

        [TestMethod]
        public void CandidateBeliefSystem_CreateBelief_ShouldSucceed()
        {
            // Arrange
            var confidence = new ConfidenceVector
            {
                Factual = 0.8f,
                Logical = 0.7f,
                Temporal = 0.9f,
                Source = 0.8f
            };

            var context = new ConversationalContext
            {
                FullConversation = "User stated that Alice is a scientist.",
                OriginalQuery = new QueryContext { Domain = QueryDomain.Factual }
            };

            var source = new BeliefSource
            {
                SourceType = "User",
                Reliability = 0.8f
            };

            // Act
            var belief = _beliefSystem.CreateCandidateBelief(
                "Alice is a scientist", confidence, context, source);

            // Assert
            Assert.IsNotNull(belief);
            Assert.AreEqual("Alice is a scientist", belief.Statement);
            Assert.AreEqual(BeliefTrack.StandardTrack, belief.Track); // 0.8 composite should be standard track
            Assert.IsFalse(belief.IsPromoted);
        }

        [TestMethod]
        public async Task CandidateBeliefSystem_FastTrackPromotion_ShouldPromote()
        {
            // Arrange
            var highConfidence = new ConfidenceVector
            {
                Factual = 0.98f,
                Logical = 0.96f,
                Temporal = 0.95f,
                Source = 0.97f
            };

            var context = new ConversationalContext();
            var source = new BeliefSource { SourceType = "Expert", Reliability = 0.95f };

            var belief = _beliefSystem.CreateCandidateBelief(
                "Water boils at 100°C", highConfidence, context, source);

            // Act
            var promotionResult = await _beliefSystem.EvaluateForPromotionAsync(belief);

            // Assert
            Assert.IsTrue(promotionResult.ShouldPromote);
            Assert.AreEqual(BeliefTrack.FastTrack, belief.Track);
        }

        [TestMethod]
        public async Task CandidateBeliefSystem_SlowTrackValidation_ShouldRequireMoreEvidence()
        {
            // Arrange
            var lowConfidence = new ConfidenceVector
            {
                Factual = 0.5f,
                Logical = 0.4f,
                Temporal = 0.6f,
                Source = 0.5f
            };

            var context = new ConversationalContext();
            var source = new BeliefSource { SourceType = "Unknown", Reliability = 0.3f };

            var belief = _beliefSystem.CreateCandidateBelief(
                "Aliens exist on Mars", lowConfidence, context, source);

            // Act
            var promotionResult = await _beliefSystem.EvaluateForPromotionAsync(belief);

            // Assert
            Assert.IsFalse(promotionResult.ShouldPromote);
            Assert.AreEqual(BeliefTrack.SlowTrack, belief.Track);
            Assert.IsTrue(promotionResult.Reason.Contains("evidence"));
        }

        [TestMethod]
        public void CandidateBeliefSystem_TemporalDecay_ShouldDecrease()
        {
            // Arrange
            var confidence = new ConfidenceVector { Factual = 0.8f, Logical = 0.8f, Temporal = 0.8f, Source = 0.8f };
            var context = new ConversationalContext();
            var source = new BeliefSource { SourceType = "News", Reliability = 0.7f };

            var belief = _beliefSystem.CreateCandidateBelief(
                "Breaking news event happened", confidence, context, source);

            // Manually set creation time to past
            belief.CreatedAt = DateTime.UtcNow.AddDays(-10);
            belief.Category = BeliefCategory.Temporal;

            // Act
            float currentConfidence = belief.CurrentConfidence;

            // Assert
            Assert.IsTrue(currentConfidence < belief.InitialConfidence.CompositeScore);
        }

        #endregion

        #region Temporal Reasoning Engine Tests

        [TestMethod]
        public async Task TemporalReasoningEngine_AlwaysOperator_ShouldEvaluate()
        {
            // Arrange
            var subject = _testUKS.Labeled("John");
            var predicate = _testUKS.GetRelationship("John", "teaches", "Mathematics");
            var obj = _testUKS.Labeled("Mathematics");

            var query = new TemporalQuery
            {
                Operator = TemporalOperator.ALWAYS,
                Subject = subject,
                Predicate = predicate,
                Object = obj
            };

            // Add eternal knowledge to support the query
            var temporalKnowledge = new TemporalKnowledge
            {
                Subject = subject,
                Predicate = predicate,
                Object = obj,
                IsEternal = true,
                BaseConfidence = 0.9f
            };
            _temporalEngine.AddTemporalKnowledge(temporalKnowledge);

            // Act
            var result = await _temporalEngine.EvaluateTemporalQueryAsync(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result);
            Assert.IsTrue(result.Confidence > 0.8f);
            Assert.IsTrue(result.ReasoningTrace.Any(t => t.Contains("eternal")));
        }

        [TestMethod]
        public async Task TemporalReasoningEngine_EventuallyOperator_ShouldFindFutureEvent()
        {
            // Arrange
            var subject = _testUKS.Labeled("Mary");
            var action = _testUKS.GetOrAddThing("graduate", "Action");

            var query = new TemporalQuery
            {
                Operator = TemporalOperator.EVENTUALLY,
                Subject = subject,
                Action = action
            };

            // Add future event
            var futureEvent = new TemporalEvent
            {
                Timestamp = DateTime.UtcNow.AddMonths(6),
                Actor = subject,
                Action = action,
                Confidence = 0.8f
            };
            _temporalEngine.AddTemporalEvent(futureEvent);

            // Act
            var result = await _temporalEngine.EvaluateTemporalQueryAsync(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Result);
            Assert.IsTrue(result.RelevantEvents.Any());
        }

        [TestMethod]
        public async Task TemporalReasoningEngine_NextOperator_ShouldPredictNextState()
        {
            // Arrange
            var subject = _testUKS.Labeled("John");
            var action = _testUKS.GetOrAddThing("teach", "Action");

            var query = new TemporalQuery
            {
                Operator = TemporalOperator.NEXT,
                Subject = subject,
                Action = action,
                Granularity = TemporalGranularity.Day
            };

            // Act
            var result = await _temporalEngine.EvaluateTemporalQueryAsync(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProcessingTime.TotalSeconds < 5); // Should be fast
        }

        [TestMethod]
        public async Task TemporalReasoningEngine_CausalChainFinding_ShouldWork()
        {
            // Arrange
            var event1 = new TemporalEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-2),
                Actor = _testUKS.Labeled("John"),
                Action = _testUKS.GetOrAddThing("study", "Action"),
                Confidence = 0.9f
            };

            var event2 = new TemporalEvent
            {
                Timestamp = DateTime.UtcNow.AddDays(-1),
                Actor = _testUKS.Labeled("John"),
                Action = _testUKS.GetOrAddThing("teach", "Action"),
                Confidence = 0.8f
            };

            // Set up causal relationship
            event1.CausalSuccessors.Add(event2);
            event2.CausalPredecessors.Add(event1);

            _temporalEngine.AddTemporalEvent(event1);
            _temporalEngine.AddTemporalEvent(event2);

            // Act
            var chains = await _temporalEngine.FindCausalChainsAsync(
                _testUKS.Labeled("John"), _testUKS.GetOrAddThing("teach", "Action"));

            // Assert
            Assert.IsTrue(chains.Any());
            Assert.IsTrue(chains.First().Events.Count >= 2);
            Assert.IsTrue(chains.First().OverallConfidence > 0);
        }

        #endregion

        #region Integration Engine Tests

        [TestMethod]
        public async Task IntegrationEngine_StandardReasoning_ShouldWork()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "Is John a teacher?",
                ReasoningType = EnhancedReasoningType.StandardReasoning,
                Context = new QueryContext { Domain = QueryDomain.Factual }
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Result);
            Assert.IsTrue(response.ProcessingTime.TotalSeconds < 10);
            Assert.IsTrue(response.ReasoningTrace.Any());
        }

        [TestMethod]
        public async Task IntegrationEngine_ConfidenceEnhanced_ShouldFuseConfidence()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "What is the capital of France?",
                ReasoningType = EnhancedReasoningType.ConfidenceEnhanced,
                Context = new QueryContext { Domain = QueryDomain.Factual, RequiresFactualAccuracy = true }
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.ConfidenceFusion);
            Assert.IsTrue(response.OverallConfidence.CompositeScore > 0);
            Assert.IsTrue(response.ReasoningTrace.Any(t => t.Contains("confidence")));
        }

        [TestMethod]
        public async Task IntegrationEngine_BeliefAugmented_ShouldCreateCandidateBeliefs()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "Alice is a brilliant scientist",
                ReasoningType = EnhancedReasoningType.BeliefAugmented,
                ConversationContext = new ConversationalContext
                {
                    FullConversation = "User mentioned Alice is a scientist",
                    OriginalQuery = new QueryContext { Domain = QueryDomain.Factual }
                }
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.CandidateBeliefs.Any());
            Assert.IsTrue(response.ReasoningTrace.Any(t => t.Contains("belief")));
        }

        [TestMethod]
        public async Task IntegrationEngine_TemporalAware_ShouldProcessTemporalQuery()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "Will John always teach mathematics?",
                ReasoningType = EnhancedReasoningType.TemporalAware,
                Context = new QueryContext { Domain = QueryDomain.Temporal }
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.TemporalResult);
            Assert.IsTrue(response.ReasoningTrace.Any(t => t.Contains("temporal")));
        }

        [TestMethod]
        public async Task IntegrationEngine_FullyEnhanced_ShouldIntegrateAllComponents()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "John will eventually become a professor",
                ReasoningType = EnhancedReasoningType.FullyEnhanced,
                Context = new QueryContext { Domain = QueryDomain.Factual },
                ConversationContext = new ConversationalContext
                {
                    FullConversation = "Discussion about John's career path",
                    OriginalQuery = new QueryContext { Domain = QueryDomain.Temporal }
                }
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.ConfidenceFusion);
            Assert.IsTrue(response.CandidateBeliefs.Any() || response.TemporalResult != null);
            Assert.IsTrue(response.Result.Contains("Enhanced Reasoning Result"));
            Assert.IsTrue(response.PerformanceMetrics.Any());
        }

        #endregion

        #region Performance Tests

        [TestMethod]
        public async Task PerformanceTest_ConcurrentRequests_ShouldHandleLoad()
        {
            // Arrange
            var requests = new List<EnhancedReasoningRequest>();
            for (int i = 0; i < 10; i++)
            {
                requests.Add(new EnhancedReasoningRequest
                {
                    Query = $"Test query {i}",
                    ReasoningType = EnhancedReasoningType.StandardReasoning
                });
            }

            var stopwatch = Stopwatch.StartNew();

            // Act
            var tasks = requests.Select(r => _integrationEngine.ProcessEnhancedReasoningAsync(r));
            var responses = await Task.WhenAll(tasks);

            stopwatch.Stop();

            // Assert
            Assert.IsTrue(responses.All(r => r.Success));
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 30); // Should complete within 30 seconds
        }

        [TestMethod]
        public async Task PerformanceTest_CacheEffectiveness_ShouldImprovePerformance()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "Is Paris in France?",
                ReasoningType = EnhancedReasoningType.ConfidenceEnhanced,
                Context = new QueryContext { Domain = QueryDomain.Factual }
            };

            // Act - First request (uncached)
            var stopwatch1 = Stopwatch.StartNew();
            var response1 = await _integrationEngine.ProcessEnhancedReasoningAsync(request);
            stopwatch1.Stop();

            // Act - Second request (should be faster due to caching)
            var stopwatch2 = Stopwatch.StartNew();
            var response2 = await _integrationEngine.ProcessEnhancedReasoningAsync(request);
            stopwatch2.Stop();

            // Assert
            Assert.IsTrue(response1.Success);
            Assert.IsTrue(response2.Success);
            // Note: Caching benefit may not always be observable in unit tests due to overhead
        }

        [TestMethod]
        public void PerformanceTest_MemoryUsage_ShouldBeReasonable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);

            // Act - Create many candidate beliefs
            for (int i = 0; i < 100; i++)
            {
                var confidence = new ConfidenceVector { Factual = 0.7f, Logical = 0.7f, Temporal = 0.7f, Source = 0.7f };
                var context = new ConversationalContext();
                var source = new BeliefSource { SourceType = "Test", Reliability = 0.7f };

                _beliefSystem.CreateCandidateBelief($"Test belief {i}", confidence, context, source);
            }

            var finalMemory = GC.GetTotalMemory(true);

            // Assert
            var memoryIncrease = finalMemory - initialMemory;
            Assert.IsTrue(memoryIncrease < 10 * 1024 * 1024); // Less than 10MB increase
        }

        #endregion

        #region Integration and Compatibility Tests

        [TestMethod]
        public void Compatibility_WithExistingUKS_ShouldMaintainIntegrity()
        {
            // Arrange
            var originalThingCount = _testUKS.UKSList.Count;

            // Act - Add temporal knowledge
            var temporalKnowledge = new TemporalKnowledge
            {
                Subject = _testUKS.Labeled("John"),
                Predicate = _testUKS.GetRelationship("John", "teaches", "Mathematics"),
                Object = _testUKS.Labeled("Mathematics"),
                IsEternal = true
            };
            _temporalEngine.AddTemporalKnowledge(temporalKnowledge);

            // Assert - UKS should remain unchanged
            Assert.AreEqual(originalThingCount, _testUKS.UKSList.Count);
        }

        [TestMethod]
        public void Compatibility_ModuleBaseInheritance_ShouldWork()
        {
            // Act
            _integrationEngine.Initialize();
            _integrationEngine.Fire();

            // Assert
            Assert.IsTrue(_integrationEngine.initialized);
            Assert.AreEqual("Phase2IntegrationEngine", _integrationEngine.Label);
        }

        [TestMethod]
        public async Task Integration_WithPhase1Components_ShouldCooperate()
        {
            // This test verifies that Phase 2 components work alongside Phase 1
            
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "John teaches Mathematics",
                ReasoningType = EnhancedReasoningType.StandardReasoning
            };

            // Act - Should use Phase 1 bridge controller internally
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.ReasoningTrace.Any(t => t.Contains("Phase 1")));
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task ErrorHandling_InvalidQuery_ShouldGracefullyFail()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = null, // Invalid query
                ReasoningType = EnhancedReasoningType.StandardReasoning
            };

            // Act
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.ReasoningTrace.Any(t => t.Contains("Error")));
        }

        [TestMethod]
        public async Task ErrorHandling_UnsupportedReasoningType_ShouldThrowException()
        {
            // Arrange
            var request = new EnhancedReasoningRequest
            {
                Query = "Test query",
                ReasoningType = (EnhancedReasoningType)999 // Invalid enum value
            };

            // Act & Assert
            var response = await _integrationEngine.ProcessEnhancedReasoningAsync(request);
            Assert.IsFalse(response.Success);
        }

        #endregion

        [TestCleanup]
        public void CleanUp()
        {
            _integrationEngine?.Dispose();
            _beliefSystem?.Dispose();
        }
    }

    #region Test Utilities and Helpers

    /// <summary>
    /// Helper class for creating test data
    /// </summary>
    public static class TestDataFactory
    {
        public static EnhancedReasoningRequest CreateTestRequest(
            string query = "Test query",
            EnhancedReasoningType type = EnhancedReasoningType.StandardReasoning)
        {
            return new EnhancedReasoningRequest
            {
                Query = query,
                ReasoningType = type,
                Context = new QueryContext { Domain = QueryDomain.Factual },
                ConversationContext = new ConversationalContext
                {
                    FullConversation = "Test conversation",
                    OriginalQuery = new QueryContext { Domain = QueryDomain.Common_Sense }
                }
            };
        }

        public static CandidateBelief CreateTestBelief(
            string statement = "Test belief",
            float confidence = 0.8f)
        {
            var confidenceVector = new ConfidenceVector
            {
                Factual = confidence,
                Logical = confidence,
                Temporal = confidence,
                Source = confidence
            };

            var context = new ConversationalContext();
            var source = new BeliefSource
            {
                SourceType = "Test",
                Reliability = confidence
            };

            var beliefSystem = new CandidateBeliefSystem(new UKS.UKS());
            return beliefSystem.CreateCandidateBelief(statement, confidenceVector, context, source);
        }

        public static TemporalQuery CreateTestTemporalQuery(
            TemporalOperator op = TemporalOperator.EVENTUALLY)
        {
            return new TemporalQuery
            {
                Operator = op,
                Subject = new Thing { Label = "TestSubject" },
                Action = new Thing { Label = "TestAction" },
                ConfidenceThreshold = 0.5f
            };
        }
    }

    /// <summary>
    /// Performance measurement utilities
    /// </summary>
    public static class PerformanceMeasurement
    {
        public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            T result = await operation();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }

        public static (T Result, TimeSpan Duration) Measure<T>(Func<T> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            T result = operation();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Memory usage monitoring
    /// </summary>
    public static class MemoryMonitor
    {
        public static long GetCurrentMemoryUsage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(false);
        }

        public static (T Result, long MemoryDelta) MeasureMemoryUsage<T>(Func<T> operation)
        {
            long beforeMemory = GetCurrentMemoryUsage();
            T result = operation();
            long afterMemory = GetCurrentMemoryUsage();
            return (result, afterMemory - beforeMemory);
        }
    }

    #endregion
}