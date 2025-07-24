/*
 * Advanced Confidence Fusion System - Phase 2 Implementation
 * Implements sophisticated multi-dimensional confidence fusion with dynamic weighting
 * and conflict resolution for LLM-UKS Bridge enhanced reasoning capabilities.
 * 
 * Based on research from:
 * - arXiv:2502.11269v1 (Neuro-symbolic AI hybrid systems)
 * - Multi-dimensional confidence vector representations
 * - Contextual weighting systems for domain-specific adaptation
 * - Conflict resolution strategies for high-confidence disagreements
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using BrainSimY.Modules;
using UKS;

namespace LLMBridgePrototype
{
    /// <summary>
    /// Represents the domain of a query for contextual alpha adjustment
    /// </summary>
    public enum QueryDomain
    {
        Factual,        // Historical facts, scientific data
        Mathematical,   // Mathematical operations and proofs
        Creative,       // Creative writing, artistic interpretation
        Philosophical,  // Abstract reasoning and philosophy
        Temporal,       // Time-based reasoning and events
        Causal,         // Cause-and-effect relationships
        Common_Sense    // Common sense reasoning
    }

    /// <summary>
    /// Status of the fusion process
    /// </summary>
    public enum FusionStatus
    {
        Success,              // Normal fusion completed
        ConflictResolved,     // Conflict detected and resolved
        EvidenceNeeded,       // Requires additional evidence
        UserReviewRequired,   // High-confidence conflict requires human review
        UksPreferred,         // UKS confidence strongly favored
        LlmPreferred         // LLM confidence strongly favored
    }

    /// <summary>
    /// Multi-dimensional confidence vector for enhanced reasoning
    /// </summary>
    public struct ConfidenceVector
    {
        public float Factual { get; set; }    // Accuracy of factual claims
        public float Logical { get; set; }    // Validity of reasoning steps
        public float Temporal { get; set; }   // Relevance over time
        public float Source { get; set; }     // Trustworthiness of origin

        /// <summary>
        /// Composite confidence score using weighted combination
        /// </summary>
        public float CompositeScore => 
            (Factual * 0.4f + Logical * 0.3f + Temporal * 0.2f + Source * 0.1f);

        /// <summary>
        /// Check if any dimension indicates high confidence
        /// </summary>
        public bool HasHighConfidence(float threshold = 0.9f) =>
            Factual > threshold || Logical > threshold || 
            Temporal > threshold || Source > threshold;

        /// <summary>
        /// Calculate variance across dimensions to detect uncertainty patterns
        /// </summary>
        public float DimensionalVariance
        {
            get
            {
                float[] values = { Factual, Logical, Temporal, Source };
                float mean = values.Average();
                return values.Select(x => (x - mean) * (x - mean)).Average();
            }
        }
    }

    /// <summary>
    /// LLM confidence representation with source attribution
    /// </summary>
    public class LLMConfidence
    {
        public ConfidenceVector Vector { get; set; }
        public string Model { get; set; }
        public DateTime Timestamp { get; set; }
        public string ReasoningTrace { get; set; }
        public Dictionary<string, float> TokenProbabilities { get; set; }

        public LLMConfidence()
        {
            TokenProbabilities = new Dictionary<string, float>();
            Timestamp = DateTime.UtcNow;
        }

        public float CompositeScore => Vector.CompositeScore;
    }

    /// <summary>
    /// UKS confidence representation with logical grounding
    /// </summary>
    public class UKSConfidence
    {
        public ConfidenceVector Vector { get; set; }
        public List<Thing> SupportingEvidence { get; set; }
        public int InferenceDepth { get; set; }
        public DateTime KnowledgeTimestamp { get; set; }
        public float ProbabilisticValue { get; set; }

        public UKSConfidence()
        {
            SupportingEvidence = new List<Thing>();
            KnowledgeTimestamp = DateTime.UtcNow;
        }

        public float CompositeScore => Vector.CompositeScore;
    }

    /// <summary>
    /// Context information for query-specific confidence fusion
    /// </summary>
    public class QueryContext
    {
        public string Query { get; set; }
        public QueryDomain Domain { get; set; }
        public bool RequiresFactualAccuracy { get; set; }
        public bool RequiresRealTimeData { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> ContextualClues { get; set; }
        public float QueryComplexity { get; set; }
        public float QueryUncertainty { get; set; }

        public QueryContext()
        {
            ContextualClues = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Result of the confidence fusion process
    /// </summary>
    public class FusionResult
    {
        public ConfidenceVector FusedVector { get; set; }
        public FusionStatus Status { get; set; }
        public float AlphaUsed { get; set; }
        public string Explanation { get; set; }
        public Dictionary<string, float> ComponentWeights { get; set; }
        public ConflictResolution ConflictInfo { get; set; }

        public FusionResult(ConfidenceVector vector, FusionStatus status)
        {
            FusedVector = vector;
            Status = status;
            ComponentWeights = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// Information about conflicts detected during fusion
    /// </summary>
    public class ConflictResolution
    {
        public bool ConflictDetected { get; set; }
        public string ConflictType { get; set; }
        public float ConflictMagnitude { get; set; }
        public string ResolutionStrategy { get; set; }
        public string Reasoning { get; set; }
    }

    /// <summary>
    /// Advanced Confidence Fusion Engine implementing Phase 2 enhancements
    /// </summary>
    public class AdvancedConfidenceFusionEngine
    {
        private readonly Dictionary<QueryDomain, float> _domainAlphaWeights;
        private readonly LRUCache<string, FusionResult> _fusionCache;
        private readonly ConcurrentDictionary<QueryDomain, AlphaCalculator> _alphaCalculators;
        private readonly UKS.UKS _uks;

        // Configuration parameters
        private readonly float _conflictThreshold = 0.4f;
        private readonly float _highConfidenceThreshold = 0.9f;
        private readonly int _cacheSize = 1000;

        public AdvancedConfidenceFusionEngine(UKS.UKS uks)
        {
            _uks = uks;
            _fusionCache = new LRUCache<string, FusionResult>(_cacheSize);
            _alphaCalculators = new ConcurrentDictionary<QueryDomain, AlphaCalculator>();
            
            // Initialize domain-specific alpha weights
            _domainAlphaWeights = new Dictionary<QueryDomain, float>
            {
                { QueryDomain.Factual, 0.8f },      // Favor UKS for facts
                { QueryDomain.Mathematical, 0.9f },  // Strong UKS preference for math
                { QueryDomain.Creative, 0.3f },      // Favor LLM for creativity
                { QueryDomain.Philosophical, 0.4f }, // Balanced but slight LLM preference
                { QueryDomain.Temporal, 0.6f },      // Moderate UKS preference
                { QueryDomain.Causal, 0.7f },        // UKS preferred for causality
                { QueryDomain.Common_Sense, 0.5f }   // Balanced approach
            };

            InitializeAlphaCalculators();
        }

        /// <summary>
        /// Main confidence fusion method implementing the enhanced algorithm:
        /// Final_Confidence = (LLM_Confidence × (1 - α)) + (UKS_Confidence × α)
        /// </summary>
        public async Task<FusionResult> FuseConfidenceAsync(
            LLMConfidence llmConf, 
            UKSConfidence uksConf, 
            QueryContext context)
        {
            // Check cache first
            string cacheKey = GenerateCacheKey(llmConf, uksConf, context);
            if (_fusionCache.TryGetValue(cacheKey, out FusionResult cached))
            {
                return cached;
            }

            // Calculate dynamic alpha based on context
            float alpha = CalculateDynamicAlpha(context);

            // Perform multi-dimensional fusion
            ConfidenceVector fusedVector = PerformMultiDimensionalFusion(
                llmConf.Vector, uksConf.Vector, alpha);

            // Detect and resolve conflicts
            ConflictResolution conflictInfo = null;
            FusionStatus status = FusionStatus.Success;

            if (DetectHighConfidenceConflict(llmConf, uksConf))
            {
                var resolutionResult = await ResolveConflictAsync(
                    llmConf, uksConf, context, fusedVector);
                fusedVector = resolutionResult.ResolvedVector;
                status = resolutionResult.Status;
                conflictInfo = resolutionResult.ConflictInfo;
            }

            var result = new FusionResult(fusedVector, status)
            {
                AlphaUsed = alpha,
                ConflictInfo = conflictInfo,
                Explanation = GenerateExplanation(llmConf, uksConf, alpha, status)
            };

            // Cache the result
            _fusionCache.Set(cacheKey, result);
            
            return result;
        }

        /// <summary>
        /// Calculate dynamic alpha based on query characteristics and domain
        /// </summary>
        private float CalculateDynamicAlpha(QueryContext context)
        {
            float baseAlpha = _domainAlphaWeights.GetValueOrDefault(context.Domain, 0.5f);

            // Get domain-specific calculator
            var calculator = _alphaCalculators.GetOrAdd(context.Domain, 
                domain => new AlphaCalculator(domain));

            return calculator.CalculateAlpha(baseAlpha, context);
        }

        /// <summary>
        /// Perform multi-dimensional confidence fusion across all vector components
        /// </summary>
        private ConfidenceVector PerformMultiDimensionalFusion(
            ConfidenceVector llmVector, 
            ConfidenceVector uksVector, 
            float alpha)
        {
            return new ConfidenceVector
            {
                Factual = FuseFactualConfidence(llmVector.Factual, uksVector.Factual, alpha),
                Logical = FuseLogicalConfidence(llmVector.Logical, uksVector.Logical, alpha),
                Temporal = FuseTemporalConfidence(llmVector.Temporal, uksVector.Temporal, alpha),
                Source = FuseSourceConfidence(llmVector.Source, uksVector.Source, alpha)
            };
        }

        /// <summary>
        /// Specialized fusion for factual confidence with accuracy emphasis
        /// </summary>
        private float FuseFactualConfidence(float llmFactual, float uksFactual, float alpha)
        {
            // For factual claims, use conservative fusion with UKS preference
            float adjustedAlpha = Math.Min(alpha * 1.2f, 0.95f);
            return (llmFactual * (1 - adjustedAlpha)) + (uksFactual * adjustedAlpha);
        }

        /// <summary>
        /// Specialized fusion for logical confidence with reasoning validation
        /// </summary>
        private float FuseLogicalConfidence(float llmLogical, float uksLogical, float alpha)
        {
            // Logical reasoning benefits from symbolic validation
            float adjustedAlpha = alpha * 1.1f;
            return (llmLogical * (1 - adjustedAlpha)) + (uksLogical * adjustedAlpha);
        }

        /// <summary>
        /// Specialized fusion for temporal confidence with time-awareness
        /// </summary>
        private float FuseTemporalConfidence(float llmTemporal, float uksTemporal, float alpha)
        {
            // Temporal reasoning uses standard fusion
            return (llmTemporal * (1 - alpha)) + (uksTemporal * alpha);
        }

        /// <summary>
        /// Specialized fusion for source confidence with trustworthiness weighting
        /// </summary>
        private float FuseSourceConfidence(float llmSource, float uksSource, float alpha)
        {
            // Source confidence considers UKS knowledge provenance
            float adjustedAlpha = alpha * 0.9f;
            return (llmSource * (1 - adjustedAlpha)) + (uksSource * adjustedAlpha);
        }

        /// <summary>
        /// Detect high-confidence conflicts between LLM and UKS
        /// </summary>
        private bool DetectHighConfidenceConflict(LLMConfidence llmConf, UKSConfidence uksConf)
        {
            // Check for significant disagreement with high confidence on both sides
            float confidenceDifference = Math.Abs(llmConf.CompositeScore - uksConf.CompositeScore);
            bool bothHighConfidence = llmConf.CompositeScore > _highConfidenceThreshold && 
                                    uksConf.CompositeScore > _highConfidenceThreshold;

            return confidenceDifference > _conflictThreshold && bothHighConfidence;
        }

        /// <summary>
        /// Resolve conflicts using multiple strategies
        /// </summary>
        private async Task<ConflictResolutionResult> ResolveConflictAsync(
            LLMConfidence llmConf, 
            UKSConfidence uksConf, 
            QueryContext context,
            ConfidenceVector initialFusion)
        {
            var conflictInfo = new ConflictResolution
            {
                ConflictDetected = true,
                ConflictMagnitude = Math.Abs(llmConf.CompositeScore - uksConf.CompositeScore),
                ConflictType = DetermineConflictType(llmConf, uksConf)
            };

            // Strategy 1: Evidence Seeking Mode
            if (ShouldSeekAdditionalEvidence(llmConf, uksConf, context))
            {
                conflictInfo.ResolutionStrategy = "Evidence Seeking";
                conflictInfo.Reasoning = "Requesting additional evidence validation";
                return new ConflictResolutionResult
                {
                    ResolvedVector = initialFusion,
                    Status = FusionStatus.EvidenceNeeded,
                    ConflictInfo = conflictInfo
                };
            }

            // Strategy 2: Contextual Weighting
            if (context.RequiresFactualAccuracy)
            {
                conflictInfo.ResolutionStrategy = "UKS Preference";
                conflictInfo.Reasoning = "Factual accuracy required - favoring UKS knowledge";
                
                var uksPreferredVector = PerformMultiDimensionalFusion(
                    llmConf.Vector, uksConf.Vector, 0.8f);
                
                return new ConflictResolutionResult
                {
                    ResolvedVector = uksPreferredVector,
                    Status = FusionStatus.UksPreferred,
                    ConflictInfo = conflictInfo
                };
            }

            // Strategy 3: Domain-Specific Resolution
            var domainResolver = new DomainSpecificResolver();
            var domainResult = domainResolver.ResolveConflict(
                llmConf, uksConf, context.Domain);

            if (domainResult.HasValue)
            {
                conflictInfo.ResolutionStrategy = "Domain Specific";
                conflictInfo.Reasoning = $"Applied {context.Domain} domain resolution";
                
                return new ConflictResolutionResult
                {
                    ResolvedVector = domainResult.Value.Vector,
                    Status = domainResult.Value.Status,
                    ConflictInfo = conflictInfo
                };
            }

            // Strategy 4: User Escalation for unresolvable conflicts
            conflictInfo.ResolutionStrategy = "User Escalation";
            conflictInfo.Reasoning = "High-confidence conflict requires human review";
            
            return new ConflictResolutionResult
            {
                ResolvedVector = initialFusion,
                Status = FusionStatus.UserReviewRequired,
                ConflictInfo = conflictInfo
            };
        }

        /// <summary>
        /// Determine if additional evidence should be sought
        /// </summary>
        private bool ShouldSeekAdditionalEvidence(
            LLMConfidence llmConf, 
            UKSConfidence uksConf, 
            QueryContext context)
        {
            // Seek evidence for medium-confidence conflicts in factual domains
            bool mediumConfidenceConflict = 
                Math.Abs(llmConf.CompositeScore - uksConf.CompositeScore) > 0.3f &&
                (llmConf.CompositeScore < 0.9f || uksConf.CompositeScore < 0.9f);

            bool factualDomain = context.Domain == QueryDomain.Factual ||
                               context.Domain == QueryDomain.Mathematical;

            return mediumConfidenceConflict && factualDomain;
        }

        /// <summary>
        /// Determine the type of conflict for appropriate handling
        /// </summary>
        private string DetermineConflictType(LLMConfidence llmConf, UKSConfidence uksConf)
        {
            var llmVector = llmConf.Vector;
            var uksVector = uksConf.Vector;

            if (Math.Abs(llmVector.Factual - uksVector.Factual) > 0.5f)
                return "Factual Disagreement";
            
            if (Math.Abs(llmVector.Logical - uksVector.Logical) > 0.5f)
                return "Logical Disagreement";
            
            if (Math.Abs(llmVector.Temporal - uksVector.Temporal) > 0.5f)
                return "Temporal Disagreement";
            
            if (Math.Abs(llmVector.Source - uksVector.Source) > 0.5f)
                return "Source Disagreement";

            return "General Disagreement";
        }

        /// <summary>
        /// Generate cache key for fusion results
        /// </summary>
        private string GenerateCacheKey(LLMConfidence llmConf, UKSConfidence uksConf, QueryContext context)
        {
            var keyData = new
            {
                LLMScore = Math.Round(llmConf.CompositeScore, 2),
                UKSScore = Math.Round(uksConf.CompositeScore, 2),
                Domain = context.Domain,
                QueryHash = context.Query?.GetHashCode() ?? 0
            };
            
            return JsonSerializer.Serialize(keyData);
        }

        /// <summary>
        /// Generate explanation for fusion result
        /// </summary>
        private string GenerateExplanation(
            LLMConfidence llmConf, 
            UKSConfidence uksConf, 
            float alpha, 
            FusionStatus status)
        {
            return status switch
            {
                FusionStatus.Success => 
                    $"Confidence fusion completed with α={alpha:F2}. " +
                    $"LLM confidence: {llmConf.CompositeScore:F2}, UKS confidence: {uksConf.CompositeScore:F2}",
                
                FusionStatus.ConflictResolved => 
                    $"Conflict detected and resolved. Applied conflict resolution strategy.",
                
                FusionStatus.EvidenceNeeded => 
                    $"Additional evidence required for high-confidence disagreement.",
                
                FusionStatus.UserReviewRequired => 
                    $"Unresolvable conflict requires human review.",
                
                _ => "Confidence fusion completed."
            };
        }

        /// <summary>
        /// Initialize alpha calculators for different domains
        /// </summary>
        private void InitializeAlphaCalculators()
        {
            foreach (var domain in Enum.GetValues<QueryDomain>())
            {
                _alphaCalculators[domain] = new AlphaCalculator(domain);
            }
        }
    }

    /// <summary>
    /// Domain-specific alpha calculation with contextual adjustments
    /// </summary>
    public class AlphaCalculator
    {
        private readonly QueryDomain _domain;

        public AlphaCalculator(QueryDomain domain)
        {
            _domain = domain;
        }

        public float CalculateAlpha(float baseAlpha, QueryContext context)
        {
            float adjustedAlpha = baseAlpha;

            // Adjust based on query complexity
            float complexityAdjustment = CalculateComplexityAdjustment(context.QueryComplexity);
            adjustedAlpha *= complexityAdjustment;

            // Adjust based on uncertainty
            float uncertaintyAdjustment = CalculateUncertaintyAdjustment(context.QueryUncertainty);
            adjustedAlpha *= uncertaintyAdjustment;

            // Domain-specific adjustments
            adjustedAlpha = ApplyDomainSpecificAdjustments(adjustedAlpha, context);

            return Math.Clamp(adjustedAlpha, 0.1f, 0.9f);
        }

        private float CalculateComplexityAdjustment(float complexity)
        {
            // Higher complexity favors UKS structured reasoning
            return 1.0f + (complexity * 0.2f);
        }

        private float CalculateUncertaintyAdjustment(float uncertainty)
        {
            // Higher uncertainty reduces confidence in both systems
            return 1.0f - (uncertainty * 0.1f);
        }

        private float ApplyDomainSpecificAdjustments(float alpha, QueryContext context)
        {
            return _domain switch
            {
                QueryDomain.Creative => alpha * 0.8f,  // Reduce UKS influence for creativity
                QueryDomain.Mathematical => alpha * 1.2f,  // Increase UKS influence for math
                QueryDomain.Temporal => context.RequiresRealTimeData ? alpha * 0.9f : alpha * 1.1f,
                _ => alpha
            };
        }
    }

    /// <summary>
    /// Result of conflict resolution process
    /// </summary>
    public class ConflictResolutionResult
    {
        public ConfidenceVector ResolvedVector { get; set; }
        public FusionStatus Status { get; set; }
        public ConflictResolution ConflictInfo { get; set; }
    }

    /// <summary>
    /// Domain-specific conflict resolution strategies
    /// </summary>
    public class DomainSpecificResolver
    {
        public (ConfidenceVector Vector, FusionStatus Status)? ResolveConflict(
            LLMConfidence llmConf, 
            UKSConfidence uksConf, 
            QueryDomain domain)
        {
            return domain switch
            {
                QueryDomain.Mathematical => ResolveMathematicalConflict(llmConf, uksConf),
                QueryDomain.Factual => ResolveFactualConflict(llmConf, uksConf),
                QueryDomain.Creative => ResolveCreativeConflict(llmConf, uksConf),
                _ => null
            };
        }

        private (ConfidenceVector, FusionStatus) ResolveMathematicalConflict(
            LLMConfidence llmConf, UKSConfidence uksConf)
        {
            // For mathematical conflicts, strongly prefer UKS logical reasoning
            var vector = new ConfidenceVector
            {
                Factual = uksConf.Vector.Factual * 0.9f + llmConf.Vector.Factual * 0.1f,
                Logical = uksConf.Vector.Logical * 0.95f + llmConf.Vector.Logical * 0.05f,
                Temporal = uksConf.Vector.Temporal * 0.8f + llmConf.Vector.Temporal * 0.2f,
                Source = uksConf.Vector.Source * 0.9f + llmConf.Vector.Source * 0.1f
            };
            
            return (vector, FusionStatus.UksPreferred);
        }

        private (ConfidenceVector, FusionStatus) ResolveFactualConflict(
            LLMConfidence llmConf, UKSConfidence uksConf)
        {
            // For factual conflicts, prefer UKS but consider LLM for recent information
            var vector = new ConfidenceVector
            {
                Factual = uksConf.Vector.Factual * 0.8f + llmConf.Vector.Factual * 0.2f,
                Logical = uksConf.Vector.Logical * 0.75f + llmConf.Vector.Logical * 0.25f,
                Temporal = uksConf.Vector.Temporal * 0.6f + llmConf.Vector.Temporal * 0.4f,
                Source = uksConf.Vector.Source * 0.8f + llmConf.Vector.Source * 0.2f
            };
            
            return (vector, FusionStatus.UksPreferred);
        }

        private (ConfidenceVector, FusionStatus) ResolveCreativeConflict(
            LLMConfidence llmConf, UKSConfidence uksConf)
        {
            // For creative conflicts, prefer LLM intuition
            var vector = new ConfidenceVector
            {
                Factual = llmConf.Vector.Factual * 0.7f + uksConf.Vector.Factual * 0.3f,
                Logical = llmConf.Vector.Logical * 0.6f + uksConf.Vector.Logical * 0.4f,
                Temporal = llmConf.Vector.Temporal * 0.8f + uksConf.Vector.Temporal * 0.2f,
                Source = llmConf.Vector.Source * 0.7f + uksConf.Vector.Source * 0.3f
            };
            
            return (vector, FusionStatus.LlmPreferred);
        }
    }

    /// <summary>
    /// Simple LRU cache implementation for fusion results
    /// </summary>
    public class LRUCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _cache;
        private readonly LinkedList<(TKey Key, TValue Value)> _lruList;

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>();
            _lruList = new LinkedList<(TKey, TValue)>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                return true;
            }

            value = default(TValue);
            return false;
        }

        public void Set(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var existingNode))
            {
                existingNode.Value = (key, value);
                _lruList.Remove(existingNode);
                _lruList.AddFirst(existingNode);
                return;
            }

            if (_cache.Count >= _capacity)
            {
                var lastNode = _lruList.Last;
                _lruList.RemoveLast();
                _cache.Remove(lastNode.Value.Key);
            }

            var newNode = new LinkedListNode<(TKey, TValue)>((key, value));
            _lruList.AddFirst(newNode);
            _cache[key] = newNode;
        }
    }
}