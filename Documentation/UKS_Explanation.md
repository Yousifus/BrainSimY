# Universal Knowledge Store (UKS) Deep Dive

## Introduction

The Universal Knowledge Store (UKS) is the foundational component of BrainSimY, implementing a graph-based knowledge representation system that captures the relationships between concepts, objects, and ideas. Unlike traditional databases or knowledge graphs, the UKS implements sophisticated inheritance mechanisms with exception handling, making it uniquely suited for representing common-sense knowledge.

## Core Concepts

### Things: The Knowledge Nodes

**Things** are the fundamental building blocks of knowledge in the UKS. A Thing can represent anything:
- Physical objects (car, dog, house)
- Abstract concepts (love, intelligence, color)
- Properties (red, fast, large)
- Actions (run, think, eat)
- Categories (animal, vehicle, emotion)

#### Thing Structure
```csharp
public partial class Thing
{
    private List<Relationship> relationships = new List<Relationship>();
    private List<Relationship> relationshipsFrom = new List<Relationship>();
    private string label = "";
    object value;
    public int useCount = 0;
    public DateTime lastFiredTime = new();
}
```

#### Key Features
- **Labels**: Human-readable identifiers (case-insensitive)
- **Values**: Any serializable object can be attached
- **Usage Tracking**: Monitors access patterns for optimization
- **Implicit Conversion**: Strings automatically convert to Things via label lookup

### Relationships: The Knowledge Connections

**Relationships** connect Things together, forming the knowledge graph. Each Relationship has:
- **Source**: The Thing making the relationship
- **Target**: The Thing being related to
- **Relationship Type**: What kind of connection exists (also a Thing)

#### Common Relationship Types
- `is-a`: Taxonomic relationships (Fido is-a dog)
- `has-child`: Hierarchical structure (dog has-child Fido)
- `has-property`: Attribute relationships (car has-property red)
- `can-do`: Action capabilities (dog can-do bark)
- `part-of`: Compositional relationships (wheel part-of car)

#### Relationship Structure
```csharp
public class Relationship
{
    public Thing source { get; set; }
    public Thing target { get; set; }
    public Thing reltype { get; set; }
    public float weight = 1.0f;
    public int useCount = 0;
    public DateTime lastUsed = DateTime.Now;
    public TimeSpan TimeToLive = TimeSpan.MaxValue;
    public List<Clause> Clauses = new List<Clause>();
}
```

### Clauses: Conditional Knowledge

**Clauses** relate multiple Relationships, enabling conditional and context-dependent knowledge:
```
"Fido can play fetch IF the weather is sunny"
```

This is represented as:
- Relationship 1: `Fido can-do fetch`
- Relationship 2: `weather is-property sunny`
- Clause: `IF` connecting these relationships

## Inheritance System

### Basic Inheritance

The UKS implements automatic inheritance where properties of parent Things are automatically inherited by children:

```
dog has-property "4 legs"
Fido is-a dog
→ Fido automatically has "4 legs" (inherited)
```

### Exception Handling

The inheritance system supports exceptions, allowing specific instances to override inherited properties:

```
dog has-property "4 legs"
Tripper is-a dog
Tripper has-property "3 legs" (exception)
→ Query for Tripper's legs returns "3 legs", not "4 legs"
```

### Implementation Details

The inheritance mechanism works through the query system:

```csharp
// From UKS.Query.cs
public IList<Thing> GetChildren(Thing parent, Thing relType = null, 
    int maxCount = int.MaxValue, bool includeInherited = true)
{
    // Direct children
    var directChildren = GetDirectChildren(parent, relType, maxCount);
    
    if (includeInherited)
    {
        // Add inherited children from parent's parents
        foreach (var parentParent in parent.Parents)
        {
            var inheritedChildren = GetChildren(parentParent, relType, 
                maxCount - directChildren.Count, true);
            // Merge without duplicates, prioritizing direct relationships
        }
    }
    
    return directChildren;
}
```

## Knowledge Organization

### Hierarchical Structure

The UKS organizes knowledge in hierarchies:

```
Thing (root)
├── PhysicalObject
│   ├── Animal
│   │   ├── Dog
│   │   │   ├── Fido
│   │   │   └── Tripper
│   │   └── Cat
│   └── Vehicle
└── AbstractConcept
    ├── Color
    │   ├── Red
    │   └── Blue
    └── Emotion
```

### Label Management

The `ThingLabels` class provides efficient label-to-Thing mapping:

```csharp
public static class ThingLabels
{
    private static Dictionary<string, Thing> labelDictionary = new();
    
    public static Thing GetThing(string label)
    {
        if (labelDictionary.TryGetValue(label.ToLower(), out Thing thing))
            return thing;
        return null;
    }
}
```

## Query System

### Basic Queries

The UKS provides powerful querying capabilities:

```csharp
// Find all dogs
var dogs = uks.GetChildren("Animal", "has-child").Where(t => t.Label.Contains("dog"));

// Find what Fido can do
var actions = uks.GetRelationships("Fido", "can-do");

// Find all red things
var redThings = uks.GetRelationshipsFrom("red", "has-property");
```

### Advanced Queries

Complex queries can traverse multiple relationship types:

```csharp
// Find all animals that can run fast
var fastAnimals = uks.Query()
    .FromType("Animal")
    .WithProperty("can-do", "run")
    .WithProperty("speed", "fast")
    .Execute();
```

### Query Optimization

The system optimizes queries through:
- **Usage Tracking**: Frequently accessed relationships are prioritized
- **Caching**: Recent query results are cached
- **Index Structures**: Label dictionary for fast Thing lookup
- **Lazy Evaluation**: Inheritance is computed on-demand

## Persistence and File Management

### XML Serialization

The UKS saves/loads knowledge using XML format:

```xml
<UKS>
  <Things>
    <Thing Label="dog" Value="" UseCount="5">
      <Relationships>
        <Relationship Source="dog" Target="animal" Type="is-a" Weight="1.0"/>
        <Relationship Source="dog" Target="4 legs" Type="has-property" Weight="1.0"/>
      </Relationships>
    </Thing>
  </Things>
</UKS>
```

### Circular Reference Handling

During serialization, the system:
1. Converts Thing references to indices (SThing structures)
2. Saves the temporary indexed structure
3. Restores Thing references during loading

## Performance Considerations

### Time-to-Live (TTL) Relationships

Transient relationships automatically expire:

```csharp
// Create temporary relationship
var tempRel = fido.AddRelationship("at-location", "park");
tempRel.TimeToLive = TimeSpan.FromHours(1); // Expires in 1 hour
```

### Memory Management

- **Orphan Cleanup**: Things without parents are automatically managed
- **Garbage Collection**: Unused relationships are periodically removed
- **Reference Counting**: Tracks relationship usage for optimization

## Integration with Modules

### Module Access

Modules access the UKS through the base class:

```csharp
public abstract class ModuleBase
{
    public UKS.UKS theUKS = null;
    
    public void GetUKS()
    {
        theUKS = MainWindow.theUKS;
    }
}
```

### Real-time Updates

Modules can:
- Add new Things and Relationships
- Modify existing knowledge
- Query for decision making
- Subscribe to knowledge changes

## Advanced Features

### Multi-threading Support

The UKS handles concurrent access:

```csharp
lock (UKSList)
{
    UKSList.Add(newThing);
}
```

### Extensibility

The partial class design allows for extensions:
- `UKS.File.cs`: File operations
- `UKS.Query.cs`: Query operations  
- `UKS.Statement.cs`: Knowledge assertions

### Event System

The UKS can notify modules of changes:
- Thing creation/deletion
- Relationship modifications
- Query executions

## Best Practices

### Knowledge Design
1. **Use Consistent Hierarchies**: Maintain clear is-a relationships
2. **Meaningful Labels**: Use descriptive, consistent naming
3. **Balanced Granularity**: Avoid overly specific or generic concepts
4. **Exception Documentation**: Document when and why exceptions are used

### Performance Optimization
1. **Batch Operations**: Group related knowledge updates
2. **TTL Management**: Use temporary relationships for transient data
3. **Query Efficiency**: Leverage inheritance for efficient queries
4. **Indexing**: Ensure important concepts have clear labels

### Integration Guidelines
1. **Module Isolation**: Modules should not directly manipulate other modules' knowledge
2. **Consistent Interfaces**: Use standard relationship types across modules
3. **Error Handling**: Handle missing Things gracefully
4. **Documentation**: Document module-specific knowledge structures

---

The UKS represents a significant advancement in knowledge representation, combining the flexibility of graph databases with the efficiency of inheritance systems. Its ability to handle exceptions while maintaining hierarchical relationships makes it uniquely suited for representing the nuanced, contextual knowledge required for common-sense reasoning.