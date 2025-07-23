/*
 * Temporal Reasoning Engine - Phase 2 Implementation
 * Implements sophisticated temporal logic operators and time-aware reasoning
 * for LLM-UKS Bridge enhanced reasoning capabilities.
 * 
 * Based on research from:
 * - Temporal logic operators (ALWAYS, EVENTUALLY, NEXT, UNTIL)
 * - Time-aware knowledge representation with validity periods
 * - Mathematical models for confidence degradation over time
 * - Event ordering and causal reasoning algorithms
 * - Integration with UKS's existing temporal knowledge support
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
    /// Temporal logic operators for reasoning
    /// </summary>
    public enum TemporalOperator
    {
        ALWAYS,     // □φ - φ is true in all future states
        EVENTUALLY, // ◇φ - φ will be true in some future state
        NEXT,       // Xφ - φ is true in the next state
        UNTIL,      // φUψ - φ until ψ becomes true
        SINCE,      // φSψ - φ since ψ was true
        BEFORE,     // φ happens before ψ
        AFTER,      // φ happens after ψ
        DURING      // φ happens during ψ
    }

    /// <summary>
    /// Temporal direction for reasoning
    /// </summary>
    public enum TemporalDirection
    {
        Future,     // Forward temporal reasoning
        Past,       // Backward temporal reasoning
        Present,    // Current state reasoning
        Bidirectional // Both directions
    }

    /// <summary>
    /// Granularity of temporal reasoning
    /// </summary>
    public enum TemporalGranularity
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year,
        Decade,
        Century,
        Eternal
    }

    /// <summary>
    /// Temporal query representation
    /// </summary>
    public class TemporalQuery
    {
        public Guid Id { get; set; }
        public TemporalOperator Operator { get; set; }
        public Thing Subject { get; set; }
        public Relationship Predicate { get; set; }
        public Thing Object { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? ReferenceTime { get; set; }
        public TemporalDirection Direction { get; set; }
        public TemporalGranularity Granularity { get; set; }
        public float ConfidenceThreshold { get; set; }
        public Dictionary<string, object> Constraints { get; set; }

        public TemporalQuery()
        {
            Id = Guid.NewGuid();
            ReferenceTime = DateTime.UtcNow;
            Direction = TemporalDirection.Future;
            Granularity = TemporalGranularity.Day;
            ConfidenceThreshold = 0.5f;
            Constraints = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Time-aware knowledge representation
    /// </summary>
    public class TemporalKnowledge
    {
        public Guid Id { get; set; }
        public Thing Subject { get; set; }
        public Relationship Predicate { get; set; }
        public Thing Object { get; set; }
        
        // Temporal validity
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsEternal { get; set; }
        
        // Confidence degradation over time
        public float BaseConfidence { get; set; }
        public TemporalDecayFunction DecayFunction { get; set; }
        
        // Temporal context
        public TemporalGranularity Granularity { get; set; }
        public List<TemporalEvent> RelatedEvents { get; set; }
        public Dictionary<string, object> TemporalMetadata { get; set; }

        public TemporalKnowledge()
        {
            Id = Guid.NewGuid();
            ValidFrom = DateTime.UtcNow;
            IsEternal = false;
            BaseConfidence = 1.0f;
            Granularity = TemporalGranularity.Day;
            RelatedEvents = new List<TemporalEvent>();
            TemporalMetadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Get confidence at specific timestamp
        /// </summary>
        public float GetConfidenceAt(DateTime timestamp)
        {
            if (IsEternal) return BaseConfidence;
            
            if (timestamp < ValidFrom || (ValidUntil.HasValue && timestamp > ValidUntil))
                return 0.0f;
            
            return DecayFunction?.CalculateConfidence(BaseConfidence, ValidFrom, timestamp) ?? BaseConfidence;
        }

        /// <summary>
        /// Check if knowledge is valid at timestamp
        /// </summary>
        public bool IsValidAt(DateTime timestamp)
        {
            return timestamp >= ValidFrom && (!ValidUntil.HasValue || timestamp <= ValidUntil);
        }
    }

    /// <summary>
    /// Temporal decay function for knowledge confidence
    /// </summary>
    public class TemporalDecayFunction
    {
        public string FunctionType { get; set; }
        public Dictionary<string, float> Parameters { get; set; }

        public TemporalDecayFunction(string functionType)
        {
            FunctionType = functionType;
            Parameters = new Dictionary<string, float>();
        }

        public float CalculateConfidence(float baseConfidence, DateTime validFrom, DateTime currentTime)
        {
            TimeSpan age = currentTime - validFrom;
            
            return FunctionType switch
            {
                "exponential" => baseConfidence * (float)Math.Exp(-Parameters["rate"] * age.TotalDays),
                "linear" => Math.Max(0, baseConfidence - (Parameters["rate"] * (float)age.TotalDays)),
                "logarithmic" => baseConfidence * (1 - (float)Math.Log(1 + Parameters["rate"] * age.TotalDays)),
                "constant" => baseConfidence,
                _ => baseConfidence
            };
        }
    }

    /// <summary>
    /// Temporal event representation
    /// </summary>
    public class TemporalEvent
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Thing Actor { get; set; }
        public Thing Action { get; set; }
        public List<Thing> Objects { get; set; }
        public float Confidence { get; set; }
        public TimeSpan? Duration { get; set; }
        public TemporalGranularity Granularity { get; set; }
        public List<TemporalEvent> CausalPredecessors { get; set; }
        public List<TemporalEvent> CausalSuccessors { get; set; }
        public Dictionary<string, object> EventMetadata { get; set; }

        public TemporalEvent()
        {
            Id = Guid.NewGuid();
            Objects = new List<Thing>();
            Confidence = 1.0f;
            Granularity = TemporalGranularity.Day;
            CausalPredecessors = new List<TemporalEvent>();
            CausalSuccessors = new List<TemporalEvent>();
            EventMetadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Causal chain representation
    /// </summary>
    public class CausalChain
    {
        public List<TemporalEvent> Events { get; set; }
        public float OverallConfidence { get; set; }
        public float CausalStrength { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string ChainType { get; set; }
        public List<string> ReasoningSteps { get; set; }

        public CausalChain()
        {
            Events = new List<TemporalEvent>();
            ReasoningSteps = new List<string>();
        }
    }

    /// <summary>
    /// Result of temporal query evaluation
    /// </summary>
    public class TemporalQueryResult
    {
        public TemporalQuery Query { get; set; }
        public bool Result { get; set; }
        public float Confidence { get; set; }
        public List<TemporalKnowledge> SupportingEvidence { get; set; }
        public List<TemporalEvent> RelevantEvents { get; set; }
        public List<string> ReasoningTrace { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public DateTime EvaluatedAt { get; set; }
        public List<TemporalProjection> TemporalProjections { get; set; }

        public TemporalQueryResult()
        {
            SupportingEvidence = new List<TemporalKnowledge>();
            RelevantEvents = new List<TemporalEvent>();
            ReasoningTrace = new List<string>();
            EvaluatedAt = DateTime.UtcNow;
            TemporalProjections = new List<TemporalProjection>();
        }
    }

    /// <summary>
    /// Temporal projection for future/past states
    /// </summary>
    public class TemporalProjection
    {
        public DateTime ProjectedTime { get; set; }
        public float Confidence { get; set; }
        public string Description { get; set; }
        public List<Thing> ProjectedState { get; set; }
        public TemporalOperator OperatorUsed { get; set; }

        public TemporalProjection()
        {
            ProjectedState = new List<Thing>();
        }
    }

    /// <summary>
    /// Main temporal reasoning engine
    /// </summary>
    public class TemporalReasoningEngine
    {
        private readonly UKS.UKS _uks;
        private readonly ConcurrentDictionary<Guid, TemporalKnowledge> _temporalKnowledge;
        private readonly ConcurrentDictionary<Guid, TemporalEvent> _temporalEvents;
        private readonly DirectedGraph<TemporalEvent> _causalGraph;
        private readonly LRUCache<string, TemporalQueryResult> _queryCache;

        // Configuration
        private readonly int _maxCausalChainLength = 10;
        private readonly TimeSpan _maxReasoningTime = TimeSpan.FromSeconds(30);
        private readonly float _minimumConfidence = 0.1f;

        public TemporalReasoningEngine(UKS.UKS uks)
        {
            _uks = uks;
            _temporalKnowledge = new ConcurrentDictionary<Guid, TemporalKnowledge>();
            _temporalEvents = new ConcurrentDictionary<Guid, TemporalEvent>();
            _causalGraph = new DirectedGraph<TemporalEvent>();
            _queryCache = new LRUCache<string, TemporalQueryResult>(1000);
        }

        /// <summary>
        /// Evaluate temporal query using specified operator
        /// </summary>
        public async Task<TemporalQueryResult> EvaluateTemporalQueryAsync(TemporalQuery query)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Check cache first
            string cacheKey = GenerateQueryCacheKey(query);
            if (_queryCache.TryGetValue(cacheKey, out TemporalQueryResult cached))
            {
                return cached;
            }

            var result = new TemporalQueryResult
            {
                Query = query
            };

            try
            {
                result = query.Operator switch
                {
                    TemporalOperator.ALWAYS => await EvaluateAlwaysAsync(query),
                    TemporalOperator.EVENTUALLY => await EvaluateEventuallyAsync(query),
                    TemporalOperator.NEXT => await EvaluateNextAsync(query),
                    TemporalOperator.UNTIL => await EvaluateUntilAsync(query),
                    TemporalOperator.SINCE => await EvaluateSinceAsync(query),
                    TemporalOperator.BEFORE => await EvaluateBeforeAsync(query),
                    TemporalOperator.AFTER => await EvaluateAfterAsync(query),
                    TemporalOperator.DURING => await EvaluateDuringAsync(query),
                    _ => throw new ArgumentException($"Unknown temporal operator: {query.Operator}")
                };

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                // Cache the result
                _queryCache.Set(cacheKey, result);
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Confidence = 0;
                result.ReasoningTrace.Add($"Error during evaluation: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Evaluate ALWAYS operator (□φ - φ is true in all future states)
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateAlwaysAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating ALWAYS operator - checking if condition holds in all future states");

            // Get relevant temporal knowledge
            var relevantKnowledge = GetRelevantTemporalKnowledge(query.Subject, query.Predicate, query.Object);
            
            // Check for eternal knowledge that supports the condition
            var eternalSupport = relevantKnowledge.Where(k => k.IsEternal && k.GetConfidenceAt(DateTime.UtcNow) > query.ConfidenceThreshold);
            
            if (eternalSupport.Any())
            {
                result.Result = true;
                result.Confidence = eternalSupport.Average(k => k.GetConfidenceAt(DateTime.UtcNow));
                result.SupportingEvidence.AddRange(eternalSupport);
                result.ReasoningTrace.Add($"Found {eternalSupport.Count()} eternal knowledge items supporting ALWAYS condition");
                return result;
            }

            // Check temporal projections for future states
            var futureProjections = await GenerateFutureProjections(query, TimeSpan.FromDays(365)); // Check next year
            
            bool allStatesSupport = futureProjections.All(p => p.Confidence > query.ConfidenceThreshold);
            
            result.Result = allStatesSupport;
            result.Confidence = allStatesSupport ? futureProjections.Average(p => p.Confidence) : 0;
            result.TemporalProjections.AddRange(futureProjections);
            result.ReasoningTrace.Add($"Evaluated {futureProjections.Count} future projections, all support: {allStatesSupport}");

            return result;
        }

        /// <summary>
        /// Evaluate EVENTUALLY operator (◇φ - φ will be true in some future state)
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateEventuallyAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating EVENTUALLY operator - checking if condition will be true in some future state");

            // Check existing future events
            var futureEvents = GetFutureEvents(query.Subject, query.Action, query.ReferenceTime ?? DateTime.UtcNow);
            
            if (futureEvents.Any(e => e.Confidence > query.ConfidenceThreshold))
            {
                var supportingEvent = futureEvents.OrderByDescending(e => e.Confidence).First();
                result.Result = true;
                result.Confidence = supportingEvent.Confidence;
                result.RelevantEvents.Add(supportingEvent);
                result.ReasoningTrace.Add($"Found future event with confidence {supportingEvent.Confidence:F2}");
                return result;
            }

            // Generate causal projections
            var causalProjections = await GenerateCausalProjections(query);
            
            bool eventuallyPossible = causalProjections.Any(p => p.Confidence > query.ConfidenceThreshold);
            
            result.Result = eventuallyPossible;
            result.Confidence = eventuallyPossible ? causalProjections.Max(p => p.Confidence) : 0;
            result.TemporalProjections.AddRange(causalProjections);
            result.ReasoningTrace.Add($"Generated {causalProjections.Count} causal projections, eventually possible: {eventuallyPossible}");

            return result;
        }

        /// <summary>
        /// Evaluate NEXT operator (Xφ - φ is true in the next state)
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateNextAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating NEXT operator - checking if condition is true in the immediate next state");

            DateTime nextTime = CalculateNextTime(query.ReferenceTime ?? DateTime.UtcNow, query.Granularity);
            
            // Check scheduled events for next time period
            var nextEvents = GetEventsAtTime(nextTime, query.Granularity);
            var relevantNextEvents = nextEvents.Where(e => 
                IsEventRelevant(e, query.Subject, query.Action, query.Object)).ToList();

            if (relevantNextEvents.Any())
            {
                var bestEvent = relevantNextEvents.OrderByDescending(e => e.Confidence).First();
                result.Result = bestEvent.Confidence > query.ConfidenceThreshold;
                result.Confidence = bestEvent.Confidence;
                result.RelevantEvents.Add(bestEvent);
                result.ReasoningTrace.Add($"Found relevant event at next time with confidence {bestEvent.Confidence:F2}");
            }
            else
            {
                // Predict next state based on current trends
                result.Confidence = await PredictNextStateConfidence(query);
                result.Result = result.Confidence > query.ConfidenceThreshold;
                result.ReasoningTrace.Add($"Predicted next state confidence: {result.Confidence:F2}");
            }

            return result;
        }

        /// <summary>
        /// Evaluate UNTIL operator (φUψ - φ until ψ becomes true)
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateUntilAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating UNTIL operator - checking if φ holds until ψ becomes true");

            // Extract φ and ψ from query constraints
            var phi = query.Constraints.GetValueOrDefault("phi") as TemporalQuery;
            var psi = query.Constraints.GetValueOrDefault("psi") as TemporalQuery;

            if (phi == null || psi == null)
            {
                result.ReasoningTrace.Add("Error: UNTIL operator requires phi and psi conditions in constraints");
                return result;
            }

            DateTime currentTime = query.ReferenceTime ?? DateTime.UtcNow;
            DateTime endTime = currentTime.AddDays(365); // Search up to one year
            
            // Find when ψ becomes true
            var psiResult = await EvaluateEventuallyAsync(psi);
            if (!psiResult.Result)
            {
                result.ReasoningTrace.Add("ψ condition never becomes true within search period");
                return result;
            }

            DateTime psiTrueTime = psiResult.TemporalProjections.FirstOrDefault()?.ProjectedTime ?? endTime;
            
            // Check if φ holds from current time until ψ becomes true
            bool phiHoldsUntilPsi = await CheckConditionHoldsPeriod(phi, currentTime, psiTrueTime);
            
            result.Result = phiHoldsUntilPsi;
            result.Confidence = phiHoldsUntilPsi ? Math.Min(psiResult.Confidence, 0.8f) : 0;
            result.ReasoningTrace.Add($"φ holds until ψ: {phiHoldsUntilPsi}, ψ becomes true at: {psiTrueTime:yyyy-MM-dd}");

            return result;
        }

        /// <summary>
        /// Evaluate SINCE operator (φSψ - φ since ψ was true)
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateSinceAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating SINCE operator - checking if φ has been true since ψ was true");

            var phi = query.Constraints.GetValueOrDefault("phi") as TemporalQuery;
            var psi = query.Constraints.GetValueOrDefault("psi") as TemporalQuery;

            if (phi == null || psi == null)
            {
                result.ReasoningTrace.Add("Error: SINCE operator requires phi and psi conditions in constraints");
                return result;
            }

            DateTime currentTime = query.ReferenceTime ?? DateTime.UtcNow;
            
            // Find when ψ was last true in the past
            var pastEvents = GetPastEvents(psi.Subject, psi.Action, currentTime);
            var psiEvent = pastEvents.OrderByDescending(e => e.Timestamp).FirstOrDefault();

            if (psiEvent == null)
            {
                result.ReasoningTrace.Add("ψ condition was never true in the past");
                return result;
            }

            // Check if φ has held from psiEvent.Timestamp to current time
            bool phiHoldsSincePsi = await CheckConditionHoldsPeriod(phi, psiEvent.Timestamp, currentTime);
            
            result.Result = phiHoldsSincePsi;
            result.Confidence = phiHoldsSincePsi ? psiEvent.Confidence * 0.9f : 0;
            result.ReasoningTrace.Add($"φ holds since ψ (at {psiEvent.Timestamp:yyyy-MM-dd}): {phiHoldsSincePsi}");

            return result;
        }

        /// <summary>
        /// Evaluate BEFORE operator
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateBeforeAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating BEFORE operator - checking temporal ordering");

            // Implementation for BEFORE operator
            // This would check if one event happens before another
            result.Result = false;
            result.Confidence = 0;
            result.ReasoningTrace.Add("BEFORE operator evaluation not fully implemented");

            return result;
        }

        /// <summary>
        /// Evaluate AFTER operator
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateAfterAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating AFTER operator - checking temporal ordering");

            // Implementation for AFTER operator
            result.Result = false;
            result.Confidence = 0;
            result.ReasoningTrace.Add("AFTER operator evaluation not fully implemented");

            return result;
        }

        /// <summary>
        /// Evaluate DURING operator
        /// </summary>
        private async Task<TemporalQueryResult> EvaluateDuringAsync(TemporalQuery query)
        {
            var result = new TemporalQueryResult { Query = query };
            result.ReasoningTrace.Add("Evaluating DURING operator - checking temporal overlap");

            // Implementation for DURING operator
            result.Result = false;
            result.Confidence = 0;
            result.ReasoningTrace.Add("DURING operator evaluation not fully implemented");

            return result;
        }

        /// <summary>
        /// Generate future temporal projections
        /// </summary>
        private async Task<List<TemporalProjection>> GenerateFutureProjections(TemporalQuery query, TimeSpan horizon)
        {
            var projections = new List<TemporalProjection>();
            DateTime currentTime = query.ReferenceTime ?? DateTime.UtcNow;
            
            // Generate projections at regular intervals
            for (int i = 1; i <= 12; i++) // Monthly projections for a year
            {
                DateTime projectionTime = currentTime.AddMonths(i);
                
                var projection = new TemporalProjection
                {
                    ProjectedTime = projectionTime,
                    OperatorUsed = query.Operator,
                    Description = $"Projection for {projectionTime:yyyy-MM-dd}"
                };

                // Calculate confidence based on temporal knowledge
                projection.Confidence = await CalculateProjectionConfidence(query, projectionTime);
                
                projections.Add(projection);
            }

            return projections;
        }

        /// <summary>
        /// Generate causal projections based on causal chains
        /// </summary>
        private async Task<List<TemporalProjection>> GenerateCausalProjections(TemporalQuery query)
        {
            var projections = new List<TemporalProjection>();
            
            // Find causal chains that could lead to the desired outcome
            var causalChains = await FindCausalChainsAsync(query.Subject, query.Object);
            
            foreach (var chain in causalChains)
            {
                var projection = new TemporalProjection
                {
                    ProjectedTime = DateTime.UtcNow.Add(chain.TotalDuration),
                    Confidence = chain.OverallConfidence,
                    OperatorUsed = query.Operator,
                    Description = $"Causal projection via {chain.Events.Count} events"
                };
                
                projections.Add(projection);
            }

            return projections;
        }

        /// <summary>
        /// Find causal chains between subjects and objects
        /// </summary>
        public async Task<List<CausalChain>> FindCausalChainsAsync(Thing startSubject, Thing endObject)
        {
            var chains = new List<CausalChain>();
            
            // Find events involving the start subject
            var startEvents = _temporalEvents.Values.Where(e => e.Actor == startSubject || e.Objects.Contains(startSubject));
            
            // Find events involving the end object
            var endEvents = _temporalEvents.Values.Where(e => e.Actor == endObject || e.Objects.Contains(endObject));

            // Use graph traversal to find causal paths
            foreach (var startEvent in startEvents)
            {
                foreach (var endEvent in endEvents)
                {
                    if (endEvent.Timestamp > startEvent.Timestamp)
                    {
                        var path = FindCausalPath(startEvent, endEvent);
                        if (path.Any())
                        {
                            var chain = new CausalChain
                            {
                                Events = path,
                                OverallConfidence = CalculateChainConfidence(path),
                                CausalStrength = CalculateCausalStrength(path),
                                TotalDuration = endEvent.Timestamp - startEvent.Timestamp,
                                ChainType = "DirectCausal"
                            };
                            chains.Add(chain);
                        }
                    }
                }
            }

            return chains;
        }

        /// <summary>
        /// Calculate confidence for temporal projection
        /// </summary>
        private async Task<float> CalculateProjectionConfidence(TemporalQuery query, DateTime projectionTime)
        {
            var relevantKnowledge = GetRelevantTemporalKnowledge(query.Subject, query.Predicate, query.Object);
            
            if (!relevantKnowledge.Any()) return 0.1f;

            // Calculate average confidence at projection time
            float totalConfidence = 0;
            int validItems = 0;

            foreach (var knowledge in relevantKnowledge)
            {
                if (knowledge.IsValidAt(projectionTime))
                {
                    totalConfidence += knowledge.GetConfidenceAt(projectionTime);
                    validItems++;
                }
            }

            return validItems > 0 ? totalConfidence / validItems : 0.1f;
        }

        /// <summary>
        /// Check if condition holds during a time period
        /// </summary>
        private async Task<bool> CheckConditionHoldsPeriod(TemporalQuery condition, DateTime startTime, DateTime endTime)
        {
            // Sample the condition at regular intervals during the period
            TimeSpan interval = TimeSpan.FromDays(1); // Daily sampling
            DateTime currentTime = startTime;
            
            while (currentTime <= endTime)
            {
                condition.ReferenceTime = currentTime;
                var result = await EvaluateTemporalQueryAsync(condition);
                
                if (!result.Result || result.Confidence <= condition.ConfidenceThreshold)
                {
                    return false;
                }
                
                currentTime = currentTime.Add(interval);
            }

            return true;
        }

        /// <summary>
        /// Helper methods for temporal reasoning
        /// </summary>
        private List<TemporalKnowledge> GetRelevantTemporalKnowledge(Thing subject, Relationship predicate, Thing obj)
        {
            return _temporalKnowledge.Values
                .Where(k => k.Subject == subject && k.Predicate == predicate && k.Object == obj)
                .ToList();
        }

        private List<TemporalEvent> GetFutureEvents(Thing subject, Thing action, DateTime referenceTime)
        {
            return _temporalEvents.Values
                .Where(e => e.Timestamp > referenceTime && 
                           (e.Actor == subject || e.Action == action))
                .ToList();
        }

        private List<TemporalEvent> GetPastEvents(Thing subject, Thing action, DateTime referenceTime)
        {
            return _temporalEvents.Values
                .Where(e => e.Timestamp < referenceTime && 
                           (e.Actor == subject || e.Action == action))
                .ToList();
        }

        private List<TemporalEvent> GetEventsAtTime(DateTime time, TemporalGranularity granularity)
        {
            TimeSpan tolerance = granularity switch
            {
                TemporalGranularity.Day => TimeSpan.FromHours(12),
                TemporalGranularity.Hour => TimeSpan.FromMinutes(30),
                TemporalGranularity.Minute => TimeSpan.FromSeconds(30),
                _ => TimeSpan.FromDays(1)
            };

            return _temporalEvents.Values
                .Where(e => Math.Abs((e.Timestamp - time).TotalMilliseconds) <= tolerance.TotalMilliseconds)
                .ToList();
        }

        private DateTime CalculateNextTime(DateTime referenceTime, TemporalGranularity granularity)
        {
            return granularity switch
            {
                TemporalGranularity.Day => referenceTime.AddDays(1),
                TemporalGranularity.Hour => referenceTime.AddHours(1),
                TemporalGranularity.Minute => referenceTime.AddMinutes(1),
                TemporalGranularity.Week => referenceTime.AddDays(7),
                TemporalGranularity.Month => referenceTime.AddMonths(1),
                _ => referenceTime.AddDays(1)
            };
        }

        private bool IsEventRelevant(TemporalEvent evt, Thing subject, Thing action, Thing obj)
        {
            return (evt.Actor == subject || evt.Action == action || evt.Objects.Contains(obj));
        }

        private async Task<float> PredictNextStateConfidence(TemporalQuery query)
        {
            // Simple prediction based on recent trends
            var recentEvents = GetPastEvents(query.Subject, query.Action, DateTime.UtcNow)
                .Where(e => e.Timestamp > DateTime.UtcNow.AddDays(-30))
                .OrderByDescending(e => e.Timestamp);

            if (recentEvents.Any())
            {
                return recentEvents.Average(e => e.Confidence) * 0.8f; // Slight degradation for prediction
            }

            return 0.3f; // Default low confidence for unknown states
        }

        private List<TemporalEvent> FindCausalPath(TemporalEvent startEvent, TemporalEvent endEvent)
        {
            // Simple path finding - could be enhanced with sophisticated graph algorithms
            var path = new List<TemporalEvent> { startEvent };
            
            var current = startEvent;
            while (current != endEvent && path.Count < _maxCausalChainLength)
            {
                var nextEvent = current.CausalSuccessors
                    .Where(e => e.Timestamp <= endEvent.Timestamp)
                    .OrderBy(e => e.Timestamp)
                    .FirstOrDefault();

                if (nextEvent == null) break;
                
                path.Add(nextEvent);
                current = nextEvent;
            }

            return current == endEvent ? path : new List<TemporalEvent>();
        }

        private float CalculateChainConfidence(List<TemporalEvent> events)
        {
            if (!events.Any()) return 0;
            
            float baseConfidence = events.Min(e => e.Confidence);
            float lengthPenalty = (float)Math.Pow(0.95, events.Count - 1);
            float timePenalty = CalculateTimeGapPenalty(events);
            
            return baseConfidence * lengthPenalty * timePenalty;
        }

        private float CalculateCausalStrength(List<TemporalEvent> events)
        {
            // Calculate causal strength based on temporal proximity and logical connections
            if (events.Count < 2) return 0;
            
            float totalStrength = 0;
            for (int i = 0; i < events.Count - 1; i++)
            {
                var timeGap = events[i + 1].Timestamp - events[i].Timestamp;
                var proximityStrength = 1.0f / (1.0f + (float)timeGap.TotalDays);
                totalStrength += proximityStrength;
            }
            
            return totalStrength / (events.Count - 1);
        }

        private float CalculateTimeGapPenalty(List<TemporalEvent> events)
        {
            if (events.Count < 2) return 1.0f;
            
            float totalPenalty = 1.0f;
            for (int i = 0; i < events.Count - 1; i++)
            {
                var gap = events[i + 1].Timestamp - events[i].Timestamp;
                var penalty = 1.0f / (1.0f + (float)gap.TotalDays / 365.0f); // Yearly normalization
                totalPenalty *= penalty;
            }
            
            return totalPenalty;
        }

        private string GenerateQueryCacheKey(TemporalQuery query)
        {
            var keyData = new
            {
                Operator = query.Operator,
                Subject = query.Subject?.Label,
                Predicate = query.Predicate?.reltype?.Label,
                Object = query.Object?.Label,
                ReferenceTime = query.ReferenceTime?.ToString("yyyy-MM-dd-HH"),
                Direction = query.Direction,
                Granularity = query.Granularity
            };
            
            return JsonSerializer.Serialize(keyData);
        }

        /// <summary>
        /// Add temporal knowledge to the system
        /// </summary>
        public void AddTemporalKnowledge(TemporalKnowledge knowledge)
        {
            _temporalKnowledge[knowledge.Id] = knowledge;
        }

        /// <summary>
        /// Add temporal event to the system
        /// </summary>
        public void AddTemporalEvent(TemporalEvent temporalEvent)
        {
            _temporalEvents[temporalEvent.Id] = temporalEvent;
            
            // Add to causal graph for reasoning
            _causalGraph.AddNode(temporalEvent);
        }

        /// <summary>
        /// Get all temporal knowledge items
        /// </summary>
        public IEnumerable<TemporalKnowledge> GetAllTemporalKnowledge()
        {
            return _temporalKnowledge.Values;
        }

        /// <summary>
        /// Get all temporal events
        /// </summary>
        public IEnumerable<TemporalEvent> GetAllTemporalEvents()
        {
            return _temporalEvents.Values;
        }
    }

    /// <summary>
    /// Simple directed graph for causal reasoning
    /// </summary>
    public class DirectedGraph<T>
    {
        private readonly Dictionary<T, List<T>> _adjacencyList;

        public DirectedGraph()
        {
            _adjacencyList = new Dictionary<T, List<T>>();
        }

        public void AddNode(T node)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                _adjacencyList[node] = new List<T>();
            }
        }

        public void AddEdge(T from, T to)
        {
            AddNode(from);
            AddNode(to);
            _adjacencyList[from].Add(to);
        }

        public List<T> GetNeighbors(T node)
        {
            return _adjacencyList.GetValueOrDefault(node, new List<T>());
        }

        public IEnumerable<T> GetAllNodes()
        {
            return _adjacencyList.Keys;
        }
    }

    /// <summary>
    /// Segregated temporal processor for LLM-UKS integration
    /// </summary>
    public class SegregatedTemporalProcessor
    {
        private readonly PerceptionLayer _perceptionLayer;
        private readonly TemporalReasoningEngine _temporalEngine;
        private readonly UKS.UKS _uks;

        public SegregatedTemporalProcessor(UKS.UKS uks)
        {
            _uks = uks;
            _temporalEngine = new TemporalReasoningEngine(uks);
            _perceptionLayer = new PerceptionLayer();
        }

        /// <summary>
        /// Process temporal query with segregated reasoning
        /// </summary>
        public async Task<TemporalResponse> ProcessTemporalQueryAsync(string naturalLanguageQuery)
        {
            // Stage 1: Perception - Extract temporal elements
            var perceptionResult = await _perceptionLayer.ExtractTemporalElementsAsync(naturalLanguageQuery);
            
            // Stage 2: Temporal Reasoning - Process temporal logic
            var temporalQuery = TranslateToTemporalQuery(perceptionResult);
            var temporalResult = await _temporalEngine.EvaluateTemporalQueryAsync(temporalQuery);
            
            // Stage 3: Integration - Combine with UKS knowledge
            var uksContext = await GetUKSContextAsync(temporalQuery.Subject);
            
            return new TemporalResponse
            {
                Result = temporalResult,
                Context = uksContext,
                Confidence = CombineConfidences(temporalResult.Confidence, uksContext?.Confidence ?? 0.5f)
            };
        }

        private TemporalQuery TranslateToTemporalQuery(PerceptionResult perceptionResult)
        {
            // Translate perception result to temporal query
            return new TemporalQuery
            {
                Operator = DetermineOperator(perceptionResult.TemporalIndicators),
                Subject = perceptionResult.Subject,
                Predicate = perceptionResult.Predicate,
                Object = perceptionResult.Object,
                ReferenceTime = perceptionResult.ReferenceTime
            };
        }

        private TemporalOperator DetermineOperator(List<string> temporalIndicators)
        {
            // Simple heuristic for determining temporal operator
            if (temporalIndicators.Any(i => i.Contains("always") || i.Contains("forever")))
                return TemporalOperator.ALWAYS;
            
            if (temporalIndicators.Any(i => i.Contains("eventually") || i.Contains("someday")))
                return TemporalOperator.EVENTUALLY;
            
            if (temporalIndicators.Any(i => i.Contains("next") || i.Contains("tomorrow")))
                return TemporalOperator.NEXT;
            
            if (temporalIndicators.Any(i => i.Contains("until")))
                return TemporalOperator.UNTIL;
            
            return TemporalOperator.EVENTUALLY; // Default
        }

        private async Task<UKSContext> GetUKSContextAsync(Thing subject)
        {
            // Get relevant UKS context for the subject
            return new UKSContext
            {
                Subject = subject,
                Confidence = 0.8f // Placeholder
            };
        }

        private float CombineConfidences(float temporalConfidence, float uksConfidence)
        {
            // Simple confidence combination - could be enhanced
            return (temporalConfidence + uksConfidence) / 2.0f;
        }
    }

    /// <summary>
    /// Perception layer for extracting temporal elements
    /// </summary>
    public class PerceptionLayer
    {
        public async Task<PerceptionResult> ExtractTemporalElementsAsync(string query)
        {
            // Simple temporal element extraction - could be enhanced with NLP
            return new PerceptionResult
            {
                TemporalIndicators = ExtractTemporalIndicators(query),
                ReferenceTime = ExtractReferenceTime(query)
            };
        }

        private List<string> ExtractTemporalIndicators(string query)
        {
            var indicators = new List<string>();
            var temporalWords = new[] { "always", "never", "eventually", "next", "until", "since", "before", "after", "during" };
            
            foreach (var word in temporalWords)
            {
                if (query.ToLower().Contains(word))
                {
                    indicators.Add(word);
                }
            }
            
            return indicators;
        }

        private DateTime? ExtractReferenceTime(string query)
        {
            // Simple time extraction - could be enhanced with date/time parsing
            if (query.ToLower().Contains("tomorrow"))
                return DateTime.Today.AddDays(1);
            
            if (query.ToLower().Contains("yesterday"))
                return DateTime.Today.AddDays(-1);
            
            return null; // Use current time as default
        }
    }

    /// <summary>
    /// Supporting classes for temporal processing
    /// </summary>
    public class PerceptionResult
    {
        public List<string> TemporalIndicators { get; set; } = new List<string>();
        public Thing Subject { get; set; }
        public Relationship Predicate { get; set; }
        public Thing Object { get; set; }
        public DateTime? ReferenceTime { get; set; }
    }

    public class TemporalResponse
    {
        public TemporalQueryResult Result { get; set; }
        public UKSContext Context { get; set; }
        public float Confidence { get; set; }
    }

    public class UKSContext
    {
        public Thing Subject { get; set; }
        public float Confidence { get; set; }
    }
}