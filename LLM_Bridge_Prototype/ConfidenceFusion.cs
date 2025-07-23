/*
 * Confidence Fusion - Phase 1 Implementation
 * Prevents LLM hallucinations through dual validation of statistical and logical confidence.
 * 
 * Based on research from:
 * - Neuro-symbolic confidence fusion mechanisms
 * - Hallucination prevention in hybrid AI systems
 * - Candidate belief promotion systems for learning
 * - Multi-source confidence aggregation techniques
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using BrainSimY.Modules;

namespace LLMBridgePrototype
{
    /// <summary>
    /// Types of confidence sources in the hybrid system
    /// </summary>
    public enum ConfidenceSource
    {
        LLM_Statistical = 0,
        UKS_Logical = 1,
        EntityLinker = 2,
        ContextualFactors = 3,
        HistoricalPerformance = 4
    }
    
    /// <summary>
    /// Confidence fusion strategies
    /// </summary>
    public enum FusionStrategy
    {
        Multiplication = 0,      // Conservative: LLM × UKS
        WeightedAverage = 1,     // Balanced: weighted combination
        MinimumBound = 2,        // Very conservative: min(LLM, UKS)
        BayesianFusion = 3,      // Probabilistic: Bayesian inference
        AdaptiveFusion = 4       // Context-dependent strategy selection
    }
    
    /// <summary>
    /// Result of confidence fusion process
    /// </summary>
    public class ConfidenceFusionResult
    {
        public double FusedConfidence { get; set; }
        public Dictionary<ConfidenceSource, double> SourceConfidences { get; set; }
        public FusionStrategy StrategyUsed { get; set; }
        public bool HallucinationFlag { get; set; }
        public string HallucinationReason { get; set; }
        public List<string> WarningMessages { get; set; }
        public Dictionary<string, object> DebugInfo { get; set; }
        public DateTime ProcessedAt { get; set; }
        
        public ConfidenceFusionResult()
        {
            SourceConfidences = new Dictionary<ConfidenceSource, double>();
            WarningMessages = new List<string>();
            DebugInfo = new Dictionary<string, object>();
            ProcessedAt = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Represents a candidate belief for promotion to UKS
    /// </summary>
    public class BeliefPromotion
    {
        public string Subject { get; set; }
        public string Predicate { get; set; }
        public string Object { get; set; }
        public double PromotionThreshold { get; set; }
        public double CurrentConfidence { get; set; }
        public bool ReadyForPromotion { get; set; }
        public List<string> ValidationEvidence { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastValidated { get; set; }
        
        public BeliefPromotion()
        {
            ValidationEvidence = new List<string>();
            CreatedAt = DateTime.Now;
            LastValidated = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Main confidence fusion component that combines multiple confidence sources
    /// to prevent hallucinations and validate knowledge claims.
    /// </summary>
    public class ConfidenceFusion : ModuleBase
    {
        private const double DEFAULT_HALLUCINATION_THRESHOLD = 0.3;
        private const double DEFAULT_PROMOTION_THRESHOLD = 0.8;
        private const double CONFIDENCE_GAP_THRESHOLD = 0.4;
        
        // Fusion strategy weights
        private readonly Dictionary<ConfidenceSource, double> defaultWeights = new Dictionary<ConfidenceSource, double>
        {
            { ConfidenceSource.LLM_Statistical, 0.4 },
            { ConfidenceSource.UKS_Logical, 0.4 },
            { ConfidenceSource.EntityLinker, 0.1 },
            { ConfidenceSource.ContextualFactors, 0.1 }
        };
        
        // Configuration parameters
        private double hallucinationThreshold;
        private double promotionThreshold;
        private FusionStrategy defaultStrategy;
        private Dictionary<ConfidenceSource, double> fusionWeights;
        
        // State management
        private Dictionary<string, ConfidenceFusionResult> fusionHistory;
        private List<BeliefPromotion> candidatePromotions;
        private Dictionary<string, double> performanceMetrics;
        
        public ConfidenceFusion()
        {
            hallucinationThreshold = DEFAULT_HALLUCINATION_THRESHOLD;
            promotionThreshold = DEFAULT_PROMOTION_THRESHOLD;
            defaultStrategy = FusionStrategy.AdaptiveFusion;
            fusionWeights = new Dictionary<ConfidenceSource, double>(defaultWeights);
            
            fusionHistory = new Dictionary<string, ConfidenceFusionResult>();
            candidatePromotions = new List<BeliefPromotion>();
            performanceMetrics = new Dictionary<string, double>();
        }
        
        /// <summary>
        /// Initialize the confidence fusion component
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            LoadConfiguration();
            InitializePerformanceMetrics();
            LogMessage("ConfidenceFusion initialized successfully");
        }
        
        /// <summary>
        /// Main confidence fusion method - combines multiple confidence sources
        /// </summary>
        /// <param name="llmConfidence">Statistical confidence from LLM (0.0-1.0)</param>
        /// <param name="uksConfidence">Logical confidence from UKS (0.0-1.0)</param>
        /// <param name="contextualFactors">Additional contextual information</param>
        /// <returns>Fused confidence score and analysis</returns>
        public ConfidenceFusionResult FuseConfidences(double llmConfidence, double uksConfidence, 
                                                    Dictionary<string, double> contextualFactors = null)
        {
            var sessionId = Guid.NewGuid().ToString();
            LogMessage($"Starting confidence fusion: LLM={llmConfidence:F3}, UKS={uksConfidence:F3}");
            
            var result = new ConfidenceFusionResult();
            
            try
            {
                // Step 1: Validate input confidences
                ValidateInputConfidences(llmConfidence, uksConfidence, result);
                
                // Step 2: Collect all confidence sources
                CollectConfidenceSources(llmConfidence, uksConfidence, contextualFactors, result);
                
                // Step 3: Detect potential hallucinations
                DetectHallucinations(result);
                
                // Step 4: Select fusion strategy
                var strategy = SelectFusionStrategy(result);
                result.StrategyUsed = strategy;
                
                // Step 5: Fuse confidences using selected strategy
                result.FusedConfidence = ApplyFusionStrategy(strategy, result.SourceConfidences, contextualFactors);
                
                // Step 6: Post-process and validate result
                PostProcessResult(result);
                
                // Step 7: Update performance metrics
                UpdatePerformanceMetrics(result);
                
                // Store in history
                fusionHistory[sessionId] = result;
                
                LogMessage($"Confidence fusion completed: {result.FusedConfidence:F3} (strategy: {strategy})");
                return result;
            }
            catch (Exception ex)
            {
                LogMessage($"Error in confidence fusion: {ex.Message}");
                result.FusedConfidence = Math.Min(llmConfidence, uksConfidence) * 0.5; // Conservative fallback
                result.WarningMessages.Add($"Fusion failed, using conservative fallback: {ex.Message}");
                return result;
            }
        }
        
        /// <summary>
        /// Overloaded method with simpler signature
        /// </summary>
        public double FuseConfidences(double llmConfidence, double uksConfidence)
        {
            var result = FuseConfidences(llmConfidence, uksConfidence, null);
            return result.FusedConfidence;
        }
        
        /// <summary>
        /// Validate input confidence values
        /// </summary>
        private void ValidateInputConfidences(double llmConfidence, double uksConfidence, ConfidenceFusionResult result)
        {
            if (llmConfidence < 0.0 || llmConfidence > 1.0)
            {
                result.WarningMessages.Add($"LLM confidence out of range: {llmConfidence}");
                llmConfidence = Math.Max(0.0, Math.Min(1.0, llmConfidence));
            }
            
            if (uksConfidence < 0.0 || uksConfidence > 1.0)
            {
                result.WarningMessages.Add($"UKS confidence out of range: {uksConfidence}");
                uksConfidence = Math.Max(0.0, Math.Min(1.0, uksConfidence));
            }
        }
        
        /// <summary>
        /// Collect confidence values from all sources
        /// </summary>
        private void CollectConfidenceSources(double llmConfidence, double uksConfidence, 
                                            Dictionary<string, double> contextualFactors, 
                                            ConfidenceFusionResult result)
        {
            result.SourceConfidences[ConfidenceSource.LLM_Statistical] = llmConfidence;
            result.SourceConfidences[ConfidenceSource.UKS_Logical] = uksConfidence;
            
            // Extract entity linker confidence if available
            if (contextualFactors?.ContainsKey("entity_confidence") == true)
            {
                result.SourceConfidences[ConfidenceSource.EntityLinker] = contextualFactors["entity_confidence"];
            }
            
            // Calculate contextual factors confidence
            double contextualConfidence = CalculateContextualConfidence(contextualFactors);
            result.SourceConfidences[ConfidenceSource.ContextualFactors] = contextualConfidence;
            
            // Add historical performance factor
            double historicalConfidence = GetHistoricalPerformanceConfidence();
            result.SourceConfidences[ConfidenceSource.HistoricalPerformance] = historicalConfidence;
        }
        
        /// <summary>
        /// Detect potential hallucinations based on confidence patterns
        /// </summary>
        private void DetectHallucinations(ConfidenceFusionResult result)
        {
            var llmConf = result.SourceConfidences[ConfidenceSource.LLM_Statistical];
            var uksConf = result.SourceConfidences[ConfidenceSource.UKS_Logical];
            
            // Pattern 1: High LLM confidence, very low UKS confidence
            if (llmConf > 0.8 && uksConf < 0.2)
            {
                result.HallucinationFlag = true;
                result.HallucinationReason = "High LLM confidence contradicted by very low UKS confidence";
            }
            
            // Pattern 2: Large confidence gap
            double confidenceGap = Math.Abs(llmConf - uksConf);
            if (confidenceGap > CONFIDENCE_GAP_THRESHOLD)
            {
                result.HallucinationFlag = true;
                result.HallucinationReason = $"Large confidence gap detected: {confidenceGap:F3}";
            }
            
            // Pattern 3: UKS indicates contradiction (negative confidence)
            if (uksConf < hallucinationThreshold)
            {
                result.HallucinationFlag = true;
                result.HallucinationReason = $"UKS confidence below hallucination threshold: {uksConf:F3}";
            }
            
            // Pattern 4: Entity linking confidence very low
            if (result.SourceConfidences.ContainsKey(ConfidenceSource.EntityLinker))
            {
                var entityConf = result.SourceConfidences[ConfidenceSource.EntityLinker];
                if (entityConf < 0.3 && llmConf > 0.7)
                {
                    result.WarningMessages.Add("Poor entity linking may indicate hallucination");
                }
            }
        }
        
        /// <summary>
        /// Select appropriate fusion strategy based on confidence patterns
        /// </summary>
        private FusionStrategy SelectFusionStrategy(ConfidenceFusionResult result)
        {
            var llmConf = result.SourceConfidences[ConfidenceSource.LLM_Statistical];
            var uksConf = result.SourceConfidences[ConfidenceSource.UKS_Logical];
            
            // If hallucination detected, use conservative strategy
            if (result.HallucinationFlag)
            {
                return FusionStrategy.MinimumBound;
            }
            
            // If confidences are similar, use multiplication for reinforcement
            double confidenceGap = Math.Abs(llmConf - uksConf);
            if (confidenceGap < 0.2)
            {
                return FusionStrategy.Multiplication;
            }
            
            // If one source is much more confident, use weighted average
            if (confidenceGap > 0.4)
            {
                return FusionStrategy.WeightedAverage;
            }
            
            // For complex cases, use Bayesian fusion
            if (result.SourceConfidences.Count > 3)
            {
                return FusionStrategy.BayesianFusion;
            }
            
            // Default to weighted average
            return FusionStrategy.WeightedAverage;
        }
        
        /// <summary>
        /// Apply the selected fusion strategy
        /// </summary>
        private double ApplyFusionStrategy(FusionStrategy strategy, Dictionary<ConfidenceSource, double> confidences,
                                         Dictionary<string, double> contextualFactors)
        {
            var llmConf = confidences[ConfidenceSource.LLM_Statistical];
            var uksConf = confidences[ConfidenceSource.UKS_Logical];
            
            switch (strategy)
            {
                case FusionStrategy.Multiplication:
                    return ApplyMultiplicationFusion(llmConf, uksConf);
                
                case FusionStrategy.WeightedAverage:
                    return ApplyWeightedAverageFusion(confidences);
                
                case FusionStrategy.MinimumBound:
                    return ApplyMinimumBoundFusion(confidences);
                
                case FusionStrategy.BayesianFusion:
                    return ApplyBayesianFusion(confidences, contextualFactors);
                
                case FusionStrategy.AdaptiveFusion:
                    return ApplyAdaptiveFusion(confidences, contextualFactors);
                
                default:
                    return ApplyWeightedAverageFusion(confidences);
            }
        }
        
        /// <summary>
        /// Multiplication fusion: LLM × UKS (conservative reinforcement)
        /// </summary>
        private double ApplyMultiplicationFusion(double llmConf, double uksConf)
        {
            // Pure multiplication can be too harsh, so we use a modified version
            double product = llmConf * uksConf;
            double average = (llmConf + uksConf) / 2.0;
            
            // Blend multiplication and average based on confidence levels
            double blendFactor = Math.Min(llmConf, uksConf); // Higher when both are confident
            return product * blendFactor + average * (1.0 - blendFactor);
        }
        
        /// <summary>
        /// Weighted average fusion using configured weights
        /// </summary>
        private double ApplyWeightedAverageFusion(Dictionary<ConfidenceSource, double> confidences)
        {
            double weightedSum = 0.0;
            double totalWeight = 0.0;
            
            foreach (var source in confidences.Keys)
            {
                if (fusionWeights.ContainsKey(source))
                {
                    double weight = fusionWeights[source];
                    weightedSum += confidences[source] * weight;
                    totalWeight += weight;
                }
            }
            
            return totalWeight > 0 ? weightedSum / totalWeight : 0.0;
        }
        
        /// <summary>
        /// Minimum bound fusion: most conservative approach
        /// </summary>
        private double ApplyMinimumBoundFusion(Dictionary<ConfidenceSource, double> confidences)
        {
            // Take minimum of primary sources, then apply small penalty
            var primarySources = new[] { ConfidenceSource.LLM_Statistical, ConfidenceSource.UKS_Logical };
            double minPrimary = primarySources.Where(s => confidences.ContainsKey(s))
                                            .Select(s => confidences[s])
                                            .DefaultIfEmpty(0.0)
                                            .Min();
            
            // Apply additional penalty for uncertainty
            return minPrimary * 0.9; // 10% penalty for using conservative strategy
        }
        
        /// <summary>
        /// Bayesian fusion using prior beliefs and likelihood
        /// </summary>
        private double ApplyBayesianFusion(Dictionary<ConfidenceSource, double> confidences,
                                         Dictionary<string, double> contextualFactors)
        {
            // Simplified Bayesian approach
            // Prior: historical performance or default 0.5
            double prior = GetHistoricalPerformanceConfidence();
            
            // Likelihood: product of evidence sources
            double likelihood = 1.0;
            int evidenceCount = 0;
            
            foreach (var source in new[] { ConfidenceSource.LLM_Statistical, ConfidenceSource.UKS_Logical })
            {
                if (confidences.ContainsKey(source))
                {
                    likelihood *= confidences[source];
                    evidenceCount++;
                }
            }
            
            // Normalize likelihood
            if (evidenceCount > 0)
            {
                likelihood = Math.Pow(likelihood, 1.0 / evidenceCount);
            }
            
            // Bayesian update: P(H|E) ∝ P(E|H) * P(H)
            double posterior = (likelihood * prior) / ((likelihood * prior) + ((1 - likelihood) * (1 - prior)));
            
            return Math.Max(0.0, Math.Min(1.0, posterior));
        }
        
        /// <summary>
        /// Adaptive fusion that selects best sub-strategy based on context
        /// </summary>
        private double ApplyAdaptiveFusion(Dictionary<ConfidenceSource, double> confidences,
                                         Dictionary<string, double> contextualFactors)
        {
            // Try multiple strategies and select best result
            var strategies = new[] { 
                FusionStrategy.Multiplication, 
                FusionStrategy.WeightedAverage, 
                FusionStrategy.BayesianFusion 
            };
            
            var results = strategies.Select(s => new {
                Strategy = s,
                Confidence = ApplyFusionStrategy(s, confidences, contextualFactors)
            }).ToList();
            
            // Select strategy based on context
            var llmConf = confidences[ConfidenceSource.LLM_Statistical];
            var uksConf = confidences[ConfidenceSource.UKS_Logical];
            
            // If both sources agree (small gap), prefer multiplication
            if (Math.Abs(llmConf - uksConf) < 0.2)
            {
                return results.First(r => r.Strategy == FusionStrategy.Multiplication).Confidence;
            }
            
            // If sources disagree significantly, prefer weighted average
            if (Math.Abs(llmConf - uksConf) > 0.4)
            {
                return results.First(r => r.Strategy == FusionStrategy.WeightedAverage).Confidence;
            }
            
            // Otherwise, use Bayesian fusion
            return results.First(r => r.Strategy == FusionStrategy.BayesianFusion).Confidence;
        }
        
        /// <summary>
        /// Calculate confidence from contextual factors
        /// </summary>
        private double CalculateContextualConfidence(Dictionary<string, double> contextualFactors)
        {
            if (contextualFactors == null || contextualFactors.Count == 0)
                return 0.5; // Neutral
            
            double totalConfidence = 0.0;
            int factorCount = 0;
            
            // Known contextual factors
            var knownFactors = new[] { 
                "iteration_penalty", "query_complexity", "knowledge_coverage", 
                "entity_confidence", "relationship_strength" 
            };
            
            foreach (var factor in knownFactors)
            {
                if (contextualFactors.ContainsKey(factor))
                {
                    totalConfidence += contextualFactors[factor];
                    factorCount++;
                }
            }
            
            return factorCount > 0 ? totalConfidence / factorCount : 0.5;
        }
        
        /// <summary>
        /// Get historical performance confidence
        /// </summary>
        private double GetHistoricalPerformanceConfidence()
        {
            if (performanceMetrics.ContainsKey("average_accuracy"))
            {
                return performanceMetrics["average_accuracy"];
            }
            
            return 0.5; // Neutral default
        }
        
        /// <summary>
        /// Post-process fusion result
        /// </summary>
        private void PostProcessResult(ConfidenceFusionResult result)
        {
            // Ensure confidence is in valid range
            result.FusedConfidence = Math.Max(0.0, Math.Min(1.0, result.FusedConfidence));
            
            // Add debug information
            result.DebugInfo["fusion_weights"] = fusionWeights;
            result.DebugInfo["hallucination_threshold"] = hallucinationThreshold;
            result.DebugInfo["promotion_threshold"] = promotionThreshold;
            
            // Check if result qualifies for belief promotion
            if (result.FusedConfidence >= promotionThreshold && !result.HallucinationFlag)
            {
                result.DebugInfo["promotion_eligible"] = true;
            }
        }
        
        /// <summary>
        /// Update performance metrics for learning
        /// </summary>
        private void UpdatePerformanceMetrics(ConfidenceFusionResult result)
        {
            // Update running averages
            if (!performanceMetrics.ContainsKey("fusion_count"))
            {
                performanceMetrics["fusion_count"] = 0;
                performanceMetrics["average_confidence"] = 0;
                performanceMetrics["hallucination_rate"] = 0;
            }
            
            double count = performanceMetrics["fusion_count"];
            double avgConf = performanceMetrics["average_confidence"];
            double hallucRate = performanceMetrics["hallucination_rate"];
            
            // Update averages
            performanceMetrics["fusion_count"] = count + 1;
            performanceMetrics["average_confidence"] = (avgConf * count + result.FusedConfidence) / (count + 1);
            
            if (result.HallucinationFlag)
            {
                performanceMetrics["hallucination_rate"] = (hallucRate * count + 1) / (count + 1);
            }
            else
            {
                performanceMetrics["hallucination_rate"] = (hallucRate * count) / (count + 1);
            }
        }
        
        /// <summary>
        /// Process candidate belief for promotion to UKS
        /// </summary>
        public BeliefPromotion ProcessCandidateBelief(string subject, string predicate, string obj, 
                                                     double confidence, List<string> evidence = null)
        {
            var promotion = new BeliefPromotion
            {
                Subject = subject,
                Predicate = predicate,
                Object = obj,
                CurrentConfidence = confidence,
                PromotionThreshold = promotionThreshold,
                ReadyForPromotion = confidence >= promotionThreshold,
                ValidationEvidence = evidence ?? new List<string>()
            };
            
            candidatePromotions.Add(promotion);
            
            LogMessage($"Candidate belief processed: {subject} {predicate} {obj} " +
                      $"(confidence: {confidence:F3}, ready: {promotion.ReadyForPromotion})");
            
            return promotion;
        }
        
        /// <summary>
        /// Get candidates ready for promotion
        /// </summary>
        public List<BeliefPromotion> GetPromotionCandidates()
        {
            return candidatePromotions.Where(p => p.ReadyForPromotion).ToList();
        }
        
        /// <summary>
        /// Load configuration from settings
        /// </summary>
        private void LoadConfiguration()
        {
            // In real implementation, this would load from config file
            // For now, use defaults
            LogMessage("Configuration loaded (using defaults)");
        }
        
        /// <summary>
        /// Initialize performance metrics
        /// </summary>
        private void InitializePerformanceMetrics()
        {
            performanceMetrics["fusion_count"] = 0;
            performanceMetrics["average_confidence"] = 0.5;
            performanceMetrics["average_accuracy"] = 0.7; // Optimistic default
            performanceMetrics["hallucination_rate"] = 0.1; // 10% default
        }
        
        /// <summary>
        /// Export fusion results for analysis
        /// </summary>
        public void ExportFusionLog(string filePath)
        {
            try
            {
                var exportData = new
                {
                    fusion_history = fusionHistory,
                    performance_metrics = performanceMetrics,
                    candidate_promotions = candidatePromotions,
                    configuration = new
                    {
                        hallucination_threshold = hallucinationThreshold,
                        promotion_threshold = promotionThreshold,
                        fusion_weights = fusionWeights
                    }
                };
                
                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.IO.File.WriteAllText(filePath, json);
                
                LogMessage($"Fusion log exported to {filePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to export fusion log: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get current performance statistics
        /// </summary>
        public Dictionary<string, double> GetPerformanceStatistics()
        {
            return new Dictionary<string, double>(performanceMetrics);
        }
        
        /// <summary>
        /// Adjust fusion weights based on performance feedback
        /// </summary>
        public void AdjustFusionWeights(ConfidenceSource source, double adjustment)
        {
            if (fusionWeights.ContainsKey(source))
            {
                fusionWeights[source] = Math.Max(0.0, Math.Min(1.0, fusionWeights[source] + adjustment));
                
                // Renormalize weights
                double totalWeight = fusionWeights.Values.Sum();
                if (totalWeight > 0)
                {
                    var keys = fusionWeights.Keys.ToList();
                    foreach (var key in keys)
                    {
                        fusionWeights[key] /= totalWeight;
                    }
                }
                
                LogMessage($"Adjusted weight for {source}: {fusionWeights[source]:F3}");
            }
        }
        
        private void LogMessage(string message)
        {
            Console.WriteLine($"[ConfidenceFusion] {DateTime.Now:HH:mm:ss} - {message}");
        }
    }
}