# Advanced UKS Features

## Overview

The Universal Knowledge Store (UKS) in BrainSimY provides sophisticated features that go far beyond simple knowledge representation. This document explores the advanced capabilities that enable temporal reasoning, probabilistic knowledge, conditional logic, multi-modal integration, and common sense reasoning.

## Temporal Knowledge Management

### Time-Based Relationships

The UKS supports temporal relationships through several mechanisms:

#### TimeToLive (TTL) Relationships
```csharp
// Create temporary knowledge that expires
Relationship temp = uks.AddStatement("weather", "is", "sunny");
temp.TimeToLive = TimeSpan.FromHours(1); // Expires in 1 hour
UKS.transientRelationships.Add(temp);

// Automatic cleanup occurs every second via timer
```

#### Temporal Tracking
Every relationship includes temporal metadata:
- `DateTime timeCreated` - When the relationship was established
- `DateTime lastUsed` - Most recent access time
- `TimeSpan TimeToLive` - Expiration period (TimeSpan.MaxValue = permanent)

#### Usage-Based Relevance
```csharp
public class Relationship
{
    public DateTime lastUsed;
    public int useCount; // Incremented on each access
    
    // Automatic relevance decay over time
    public float GetRelevanceScore()
    {
        TimeSpan age = DateTime.Now - timeCreated;
        float baseWeight = weight;
        float decayFactor = Math.Max(0.1f, 1.0f - (age.TotalDays / 365.0f));
        return baseWeight * decayFactor * Math.Log(useCount + 1);
    }
}
```

### Temporal Query Processing

The UKS can answer time-sensitive queries:

```csharp
// Find relationships created within a time window
var recentRelationships = uks.GetAllRelationships(sources, false)
    .Where(r => r.timeCreated > DateTime.Now.AddDays(-7))
    .ToList();

// Find relationships that are about to expire
var expiringKnowledge = UKS.transientRelationships
    .Where(r => r.TimeToLive != TimeSpan.MaxValue && 
               r.lastUsed + r.TimeToLive < DateTime.Now.AddHours(1))
    .ToList();
```

## Conditional Logic System

### Clause-Based Conditions

The UKS supports complex conditional relationships through the Clause system:

```csharp
public class Clause
{
    public Thing clauseType;     // AND, OR, NOT, IF, UNLESS, etc.
    public Relationship clause;  // The condition relationship
}

public class Relationship
{
    public List<Clause> Clauses = new();
    public bool conditional = false;
}
```

### Conditional Relationship Examples

```csharp
// Basic conditional: "Birds can fly UNLESS they are penguins"
Relationship birdsFly = uks.AddStatement("bird", "can", "fly");
birdsFly.conditional = true;

Relationship penguinException = uks.AddStatement("penguin", "is-a", "bird");
Clause unlessClause = new Clause(
    uks.Labeled("UNLESS"), 
    penguinException
);
birdsFly.Clauses.Add(unlessClause);

// Complex conditional: "Students can access library IF they have valid ID AND are enrolled"
Relationship libraryAccess = uks.AddStatement("student", "can-access", "library");
libraryAccess.conditional = true;

// Add AND condition
Clause idClause = new Clause(
    uks.Labeled("AND"),
    uks.AddStatement("student", "has", "valid-id")
);
Clause enrolledClause = new Clause(
    uks.Labeled("AND"),
    uks.AddStatement("student", "is", "enrolled")
);

libraryAccess.Clauses.Add(idClause);
libraryAccess.Clauses.Add(enrolledClause);
```

### Conditional Query Processing

The UKS automatically evaluates conditions during queries:

```csharp
// In UKS.Query.cs
private void RemoveFalseConditionals(List<Relationship> relationships)
{
    for (int i = relationships.Count - 1; i >= 0; i--)
    {
        Relationship r = relationships[i];
        if (r.conditional)
        {
            bool conditionMet = EvaluateConditions(r.Clauses);
            if (!conditionMet)
            {
                relationships.RemoveAt(i);
                failedConditions.Add(r);
            }
            else
            {
                succeededConditions.Add(r);
            }
        }
    }
}

private bool EvaluateConditions(List<Clause> clauses)
{
    bool result = true;
    
    foreach (Clause clause in clauses)
    {
        bool clauseResult = EvaluateClause(clause);
        
        switch (clause.clauseType.Label.ToUpper())
        {
            case "AND":
                result = result && clauseResult;
                break;
            case "OR":
                result = result || clauseResult;
                break;
            case "NOT":
            case "UNLESS":
                result = result && !clauseResult;
                break;
            case "IF":
                if (!clauseResult) return false;
                break;
        }
    }
    
    return result;
}
```

## Probabilistic Reasoning

### Weight-Based Confidence

Every relationship has a weight representing confidence or probability:

```csharp
public class Relationship
{
    public float weight = 1.0f; // 0.0 to 1.0 confidence score
}

// Examples of probabilistic knowledge
var probablyTrue = uks.AddStatement("sky", "is", "blue");
probablyTrue.weight = 0.95f; // 95% confidence

var sometimes = uks.AddStatement("weather", "is", "rainy");
sometimes.weight = 0.3f; // 30% probability on any given day
```

### Conflict Resolution

When conflicting information exists, the UKS uses weights to resolve conflicts:

```csharp
// In UKS.Query.cs
private void RemoveConflictingResults(List<Relationship> relationships)
{
    // Group by source and relationship type
    var groups = relationships
        .GroupBy(r => new { r.source, r.relType })
        .Where(g => g.Count() > 1);
    
    foreach (var group in groups)
    {
        // Find conflicts (different targets)
        var targets = group.Select(r => r.target).Distinct();
        if (targets.Count() > 1)
        {
            // Keep only the highest weighted relationship
            var best = group.OrderByDescending(r => r.weight * r.GetRelevanceScore()).First();
            
            // Remove conflicting relationships
            foreach (var conflict in group.Where(r => r != best))
            {
                relationships.Remove(conflict);
            }
        }
    }
}
```

### Uncertainty Propagation

When reasoning through inheritance chains, uncertainty compounds:

```csharp
public float CalculateInheritedConfidence(Thing source, Thing target, string relationshipType)
{
    var path = FindInheritancePath(source, target);
    float confidence = 1.0f;
    
    foreach (var step in path)
    {
        // Multiply confidences along the path
        confidence *= step.weight;
    }
    
    // Apply decay for longer paths
    float pathDecay = Math.Pow(0.9f, path.Count - 1);
    return confidence * pathDecay;
}
```

## Multi-Modal Integration

### Vision Integration

The vision system in `ModuleVision.cs` demonstrates multi-modal knowledge integration:

```csharp
public partial class ModuleVision : ModuleBase
{
    public BitmapImage bitmap = null;
    public List<Corner> corners;
    public List<Segment> segments;
    public Color[,] imageArray;
    
    // Convert visual features to UKS knowledge
    private void ProcessVisualFeatures()
    {
        foreach (var segment in segments)
        {
            // Create knowledge about detected lines
            Thing lineThing = theUKS.GetOrAddThing($"line_{segment.GetHashCode()}", "visual-feature");
            theUKS.AddStatement(lineThing.Label, "has-length", segment.Length.ToString());
            theUKS.AddStatement(lineThing.Label, "has-angle", segment.Angle.ToString());
            
            // Link to spatial location
            theUKS.AddStatement(lineThing.Label, "located-at", 
                $"({segment.P1.X},{segment.P1.Y})-({segment.P2.X},{segment.P2.Y})");
        }
        
        foreach (var corner in corners)
        {
            // Create knowledge about detected corners
            Thing cornerThing = theUKS.GetOrAddThing($"corner_{corner.GetHashCode()}", "visual-feature");
            theUKS.AddStatement(cornerThing.Label, "has-angle", corner.angle.ToString());
            theUKS.AddStatement(cornerThing.Label, "is-curve", corner.curve.ToString());
        }
    }
}
```

### Sensor Data Integration

The system can integrate various sensor modalities:

```csharp
// Audio integration example
public void ProcessAudioFeature(AudioFeature audio)
{
    Thing soundThing = theUKS.GetOrAddThing($"sound_{audio.Timestamp}", "audio-feature");
    theUKS.AddStatement(soundThing.Label, "has-frequency", audio.Frequency.ToString());
    theUKS.AddStatement(soundThing.Label, "has-amplitude", audio.Amplitude.ToString());
    theUKS.AddStatement(soundThing.Label, "detected-at", audio.Timestamp.ToString());
    
    // Link to semantic meaning if recognized
    if (audio.RecognizedWord != null)
    {
        theUKS.AddStatement(soundThing.Label, "represents", audio.RecognizedWord);
    }
}

// Spatial integration
public void ProcessSpatialData(SpatialFeature spatial)
{
    Thing objectThing = theUKS.GetOrAddThing($"object_{spatial.Id}", "spatial-object");
    theUKS.AddStatement(objectThing.Label, "has-position", spatial.Position.ToString());
    theUKS.AddStatement(objectThing.Label, "has-orientation", spatial.Orientation.ToString());
    theUKS.AddStatement(objectThing.Label, "has-velocity", spatial.Velocity.ToString());
}
```

## Common Sense Reasoning

### GPT Integration for Verification

The system includes sophisticated GPT integration for common sense validation:

```csharp
// From ModuleGPTInfo.cs
public static async Task GetChatGPTVerifyParentChild(string child, string parent)
{
    string child1 = GetStringFromThingLabel(child);
    string parent1 = GetStringFromThingLabel(parent);
    
    string systemText = "Provide commonsense facts about the following:";
    string userText = $"Is the following true: a(n) {child1} is a(n) {parent1}? (yes or no, no explanation)";
    
    string answerString = await GPT.GetGPTResult(userText, systemText);
    
    if (!answerString.Contains("yes"))
    {
        // Remove relationships that don't pass common sense test
        Thing tParent = MainWindow.theUKS.Labeled(parent);
        Thing tChild = MainWindow.theUKS.Labeled(child);
        if (tParent != null && tChild != null)
        {
            tChild.RemoveParent(tParent);
            if (tChild.Parents.Count == 0)
                tChild.AddParent(MainWindow.theUKS.Labeled("unknownObject"));
        }
    }
    else
    {
        // Mark as verified
        Relationship r = MainWindow.theUKS.GetRelationship(parent, "has-child", child);
        if (r != null)
        {
            r.GPTVerified = true;
        }
    }
}
```

### Automatic Consistency Checking

The UKS can automatically detect and resolve inconsistencies:

```csharp
public void ValidateKnowledgeConsistency()
{
    // Check for contradictions
    var contradictions = FindContradictions();
    
    foreach (var contradiction in contradictions)
    {
        // Use weights and verification status to resolve
        if (contradiction.Item1.weight > contradiction.Item2.weight)
        {
            RemoveRelationship(contradiction.Item2);
        }
        else if (contradiction.Item2.weight > contradiction.Item1.weight)
        {
            RemoveRelationship(contradiction.Item1);
        }
        else
        {
            // Equal weights - check GPT verification
            if (contradiction.Item1.GPTVerified && !contradiction.Item2.GPTVerified)
            {
                RemoveRelationship(contradiction.Item2);
            }
            else if (contradiction.Item2.GPTVerified && !contradiction.Item1.GPTVerified)
            {
                RemoveRelationship(contradiction.Item1);
            }
            else
            {
                // Flag for human review
                MarkForReview(contradiction);
            }
        }
    }
}
```

## Advanced Query Patterns

### Inference Chains

The UKS can perform multi-step reasoning:

```csharp
public List<Thing> FindByInference(Thing source, string relationshipChain)
{
    // Parse relationship chain like "is-a.can.has"
    string[] steps = relationshipChain.Split('.');
    List<Thing> current = new List<Thing> { source };
    
    foreach (string step in steps)
    {
        List<Thing> next = new List<Thing>();
        
        foreach (Thing thing in current)
        {
            var relationships = GetAllRelationships(new List<Thing> { thing }, false);
            var matching = relationships.Where(r => r.relType.Label == step);
            
            foreach (var rel in matching)
            {
                next.Add(rel.target);
            }
        }
        
        current = next.Distinct().ToList();
    }
    
    return current;
}

// Usage example
var animals = uks.Labeled("animal");
var animalSounds = FindByInference(animals, "has-child.can"); // What can animals do?
```

### Probabilistic Queries

```csharp
public List<(Thing, float)> FindWithProbability(Thing source, string relationshipType, float minConfidence = 0.5f)
{
    var relationships = GetAllRelationships(new List<Thing> { source }, false);
    
    return relationships
        .Where(r => r.relType.Label == relationshipType && r.weight >= minConfidence)
        .Select(r => (r.target, r.weight))
        .OrderByDescending(t => t.Item2)
        .ToList();
}
```

### Temporal Queries

```csharp
public List<Relationship> FindTemporalPattern(string pattern, TimeSpan window)
{
    // Find relationships that match temporal patterns
    // e.g., "A followed by B within 5 minutes"
    
    var allRelationships = uks.UKSList
        .SelectMany(t => t.Relationships)
        .OrderBy(r => r.timeCreated)
        .ToList();
    
    // Pattern matching logic here
    return MatchTemporalPattern(allRelationships, pattern, window);
}
```

## Performance Optimization

### Query Optimization

The UKS includes several performance optimizations:

```csharp
// Skip expensive conflict resolution for large result sets
if (result2.Count < 200)
    RemoveConflictingResults(result2);

// Use indexed label lookup
private static Dictionary<string, Thing> labelIndex = new();

public Thing Labeled(string label)
{
    // Fast O(1) lookup instead of linear search
    return labelIndex.TryGetValue(label.ToLower(), out Thing result) ? result : null;
}
```

### Memory Management

```csharp
// Automatic cleanup of expired relationships
private void RemoveExpiredRelationships(Object stateInfo)
{
    for (int i = transientRelationships.Count - 1; i >= 0; i--)
    {
        Relationship r = transientRelationships[i];
        if (r.TimeToLive != TimeSpan.MaxValue && r.lastUsed + r.TimeToLive < DateTime.Now)
        {
            r.source.RemoveRelationship(r);
            transientRelationships.Remove(r);
            
            // Clean up orphaned things
            if (r.reltype.Label == "has-child" && r.target?.Parents.Count == 0)
            {
                r.target.AddParent(ThingLabels.GetThing("unknownObject"));
            }
        }
    }
}
```

### Parallel Processing

```csharp
// Parallel relationship processing for large datasets
public List<Relationship> GetAllRelationshipsParallel(List<Thing> sources, bool reverse)
{
    var results = sources.AsParallel()
        .SelectMany(source => source.Relationships)
        .Where(r => MeetsQueryCriteria(r))
        .ToList();
    
    return ProcessResults(results);
}
```

## Integration Patterns

### Event-Driven Updates

```csharp
public class UKSEventArgs : EventArgs
{
    public Thing Thing { get; set; }
    public Relationship Relationship { get; set; }
    public string Operation { get; set; } // "Added", "Modified", "Removed"
}

public event EventHandler<UKSEventArgs> KnowledgeChanged;

protected virtual void OnKnowledgeChanged(UKSEventArgs e)
{
    KnowledgeChanged?.Invoke(this, e);
}

// Modules can subscribe to knowledge changes
theUKS.KnowledgeChanged += (sender, e) => {
    // React to knowledge updates
    UpdateVisualization(e.Thing, e.Relationship);
};
```

### Streaming Integration

```csharp
public async Task ProcessDataStream(IAsyncEnumerable<SensorData> dataStream)
{
    await foreach (var data in dataStream)
    {
        // Convert streaming data to knowledge
        var thing = GetOrAddThing($"sensor_reading_{data.Timestamp}", "sensor-data");
        AddStatement(thing.Label, "has-value", data.Value.ToString());
        AddStatement(thing.Label, "measured-at", data.Timestamp.ToString());
        
        // Set expiration for streaming data
        var relationship = GetRelationship(thing.Label, "has-value", data.Value.ToString());
        relationship.TimeToLive = TimeSpan.FromMinutes(5);
        transientRelationships.Add(relationship);
    }
}
```

These advanced features make the UKS a powerful foundation for building intelligent systems that can reason about time, handle uncertainty, integrate multiple modalities, and apply common sense reasoning to complex problems.