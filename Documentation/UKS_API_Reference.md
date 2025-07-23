# UKS API Reference

## Overview

The Universal Knowledge Store (UKS) provides a comprehensive API for creating, querying, and managing knowledge in BrainSimY. This document serves as a complete reference for developers who need to interact with the knowledge system programmatically.

## Core Classes

### UKS Class

The main entry point for all knowledge operations.

#### Constructor
```csharp
UKS()
```
Creates a new reference to the UKS and initializes it if it's the first reference. The UKS is a singleton - there's only one shared knowledge base per application.

#### Key Properties
- `IList<Thing> UKSList` - Read-only list of all Things in the knowledge store
- `string FileName` - Currently loaded knowledge base file

#### Core Knowledge Creation Methods

**AddThing(string label, Thing parent)**
```csharp
public virtual Thing AddThing(string label, Thing parent)
```
Primitive method for creating root Things with optional parent. Use sparingly - prefer AddStatement for structured knowledge.

**GetOrAddThing(string label, string parentLabel = null)**
```csharp
public Thing GetOrAddThing(string label, string parentLabel = null)
```
Gets existing Thing by label or creates new one with specified parent. Thread-safe and handles label conflicts.

**AddStatement(object source, object relationshipType, object target, ...)**
```csharp
public Relationship AddStatement(
    object oSource, 
    object oRelationshipType, 
    object oTarget,
    object oSourceProperties = null,
    object oTypeProperties = null,
    object oTargetProperties = null
)
```
Primary method for adding structured knowledge. Creates Things from strings automatically and handles relationship inverses.

#### Query Methods

**Labeled(string label)**
```csharp
public Thing Labeled(string label)
```
Finds Thing by exact label match (case-insensitive). Returns null if not found.

**GetAllRelationships(List<Thing> sources, bool reverse)**
```csharp
public List<Relationship> GetAllRelationships(List<Thing> sources, bool reverse)
```
Gets all relationships from/to a group of Things, including inherited relationships. Handles conflicts and conditionals.

**GetAllAttributes(Thing t)**
```csharp
public List<Thing> GetAllAttributes(Thing t)
```
Gets all "is" relationships for a Thing, following inheritance chains.

#### File Operations

**LoadFromXML(string fileName)**
```csharp
public bool LoadFromXML(string fileName)
```
Loads knowledge base from XML file. Returns success status.

**SaveToXML(string fileName)**
```csharp
public bool SaveToXML(string fileName)
```
Saves current knowledge base to XML file with timestamp and metadata.

**CreateInitialStructure()**
```csharp
public void CreateInitialStructure()
```
Creates fundamental knowledge structure: Thing, Object, Action, RelationshipType, and basic relationships.

### Thing Class

Represents any concept, object, or entity in the knowledge base.

#### Key Properties
- `string Label` - Human-readable identifier (case-insensitive but preserves original case)
- `object V` - Any serializable value can be attached
- `IList<Relationship> Relationships` - Read-only list of outgoing relationships
- `IList<Relationship> RelationshipsFrom` - Read-only list of incoming relationships
- `int useCount` - Usage tracking for relevance scoring
- `DateTime lastFiredTime` - Timestamp of last access

#### Navigation Properties
- `IList<Thing> Children` - Things that have this as parent (via "has-child" relationship)
- `IList<Thing> Parents` - Parent Things (via "is-a" relationship)

#### Key Methods

**AddParent(Thing parent)**
```csharp
public void AddParent(Thing parent)
```
Creates "is-a" relationship to parent. Handles inheritance and creates bidirectional links.

**RemoveParent(Thing parent)**
```csharp
public void RemoveParent(Thing parent)
```
Removes parent relationship and cleans up bidirectional links.

**AddRelationship(Thing relType, Thing target, float weight = 1.0f)**
```csharp
public Relationship AddRelationship(Thing relType, Thing target, float weight = 1.0f)
```
Creates new relationship to target Thing with specified type and weight.

**HasAncestorLabeled(string label)**
```csharp
public bool HasAncestorLabeled(string label)
```
Checks if Thing has ancestor with given label (traverses inheritance chain).

**ToString(bool showProperties = false)**
```csharp
public string ToString(bool showProperties = false)
```
Returns displayable string. With showProperties=true, includes relationship list.

### Relationship Class

Represents weighted, typed connections between Things.

#### Key Properties
- `Thing source` - Starting Thing
- `Thing target` - Ending Thing  
- `Thing relType` - Relationship type (also a Thing)
- `float weight` - Relevance/confidence score (0.0-1.0)
- `DateTime lastUsed` - Timestamp tracking
- `TimeSpan TimeToLive` - Expiration time (TimeSpan.MaxValue = permanent)
- `bool GPTVerified` - Flag for AI verification status
- `List<Clause> Clauses` - Conditional logic and dependencies

#### Temporal Properties
- `DateTime timeCreated` - Creation timestamp
- `TimeSpan timeToLive` - Automatic expiration
- `bool conditional` - Whether relationship depends on conditions

#### Methods

**ToString()**
```csharp
public override string ToString()
```
Returns formatted relationship string: "source relType target"

### Clause Class

Represents conditional logic and dependencies between relationships.

#### Properties
- `Thing clauseType` - Type of dependency (AND, OR, NOT, etc.)
- `Relationship clause` - Target relationship for the condition

## Python Integration API

### ViewBase Class (Python)

Base class for all Python modules in BrainSimY.

```python
class ViewBase(object):
    def __init__(self, title: str, level: Union[tk.Tk, tk.Toplevel], 
                 module_type: str, uks=uks) -> None
```

#### Key Properties
- `self.uks` - Reference to the C# UKS instance
- `self.level` - Tkinter window reference
- `self.module_type` - Module identifier
- `self.label` - Module instance label

#### Required Methods
```python
@abstractmethod
def build(self):
    """Build the UI components"""
    pass
    
@abstractmethod
def fire(self):
    """Main execution loop - called repeatedly"""
    pass
```

### Python-C# Bridge

The bridge uses Python.NET to provide seamless access to UKS from Python:

```python
import pythonnet
pythonnet.load("coreclr")
import clr
clr.AddReference("UKS")
from UKS import *

# Create UKS instance
uks = UKS()

# Use exactly like C# API
thing = uks.Labeled("concept")
uks.AddStatement("dog", "is-a", "animal")
```

## Common Usage Patterns

### Creating Knowledge Hierarchies
```csharp
// Create basic taxonomy
uks.AddStatement("animal", "is-a", "living-thing");
uks.AddStatement("dog", "is-a", "animal");
uks.AddStatement("golden-retriever", "is-a", "dog");

// Add properties
uks.AddStatement("dog", "has", "fur");
uks.AddStatement("dog", "can", "bark");
```

### Querying Knowledge
```csharp
// Find all animals
Thing animalThing = uks.Labeled("animal");
List<Thing> allAnimals = animalThing.Children; // All things that "is-a" animal

// Find all properties of dogs
Thing dogThing = uks.Labeled("dog");
List<Relationship> dogProperties = uks.GetAllRelationships(
    new List<Thing> { dogThing }, false);
```

### Conditional Knowledge
```csharp
// Add conditional relationship
Relationship r = uks.AddStatement("bird", "can", "fly");
Relationship condition = uks.AddStatement("penguin", "is-a", "bird");
r.conditional = true;
// Add clause that negates flying for penguins
```

### Temporal Knowledge
```csharp
// Create temporary knowledge that expires
Relationship temp = uks.AddStatement("weather", "is", "sunny");
temp.TimeToLive = TimeSpan.FromHours(1); // Expires in 1 hour
UKS.transientRelationships.Add(temp);
```

## Error Handling

### Common Exceptions
- `ArgumentNullException` - When required Thing is not found
- `InvalidOperationException` - When attempting invalid operations (e.g., deleting Thing with children)
- `ThreadSafetyException` - When concurrent access violations occur

### Best Practices
1. Always check for null when calling `Labeled()`
2. Use `GetOrAddThing()` when you want automatic creation
3. Handle relationship cleanup when deleting Things
4. Be careful with circular references in knowledge structures

## Performance Considerations

### Optimization Guidelines
1. **Batch Operations**: Group multiple AddStatement calls when possible
2. **Label Indexing**: Labels are indexed for fast lookup - prefer label-based queries
3. **Relationship Limits**: Queries with >200 relationships skip conflict resolution for performance
4. **Memory Management**: Clean up temporary relationships to prevent memory leaks
5. **Threading**: UKS operations are thread-safe but avoid unnecessary concurrent access

### Query Performance
- Use specific labels rather than traversing all relationships
- Limit inheritance depth for better performance
- Consider relationship weights for relevance-based queries
- Cache frequently accessed Things rather than repeated label lookups

## Integration Examples

### Module Development
```csharp
public class MyModule : ModuleBase
{
    public override void Fire()
    {
        // Access UKS through inherited theUKS property
        Thing concept = theUKS.Labeled("my-concept");
        if (concept == null)
            concept = theUKS.AddStatement("my-concept", "is-a", "object");
    }
}
```

### Python Module Development
```python
class MyPythonModule(ViewBase):
    def fire(self) -> bool:
        # Access UKS through self.uks
        concept = self.uks.Labeled("my-concept")
        if concept is None:
            self.uks.AddStatement("my-concept", "is-a", "object")
        return True
```

## Advanced Features

### Inheritance Resolution
The UKS automatically resolves inheritance chains, allowing queries like "what can animals do?" to return capabilities from all animal types.

### Conflict Resolution
When multiple conflicting relationships exist, the system uses weights, timestamps, and verification status to determine the most reliable information.

### Relationship Inverses
Many relationship types have automatic inverses (e.g., "has-child" ↔ "is-a"), maintaining bidirectional consistency.

### Common Sense Integration
The system includes hooks for GPT verification of relationships, allowing AI-assisted validation of knowledge claims.

This API reference provides the foundation for building sophisticated knowledge-based applications and integrating LLMs with the BrainSimY knowledge system.