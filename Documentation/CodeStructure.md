# Code Structure Navigation Guide

## Introduction

This guide provides a comprehensive roadmap for navigating the BrainSimY codebase. Understanding the code organization is essential for effective development, debugging, and contribution to the project.

## Solution Architecture

### High-Level Project Structure

```
BrainSimulator.sln
├── BrainSimulator (Main WPF Application)
│   ├── MainWindow.xaml/.cs         # Primary UI
│   ├── Modules/                    # Module implementations
│   ├── Resources/                  # UI resources
│   └── Tools/                      # Utility classes
├── UKS (Universal Knowledge Store)
│   ├── UKS.cs                      # Core UKS class
│   ├── Thing.cs                    # Knowledge nodes
│   ├── Relationship.cs             # Knowledge edges
│   └── Documentation/              # API docs
├── PythonProj (Python Integration)
│   ├── MainWindow.py               # Python UI
│   ├── module_*.py                 # Python modules
│   └── utils.py                    # Python utilities
├── BrainSimMAC (macOS Support)
└── TestPython (Python Testing)
```

## BrainSimulator Project Deep Dive

### Core Application Files

#### MainWindow.xaml/MainWindow.xaml.cs
**Purpose**: Primary application window and entry point
**Key Components**:
```csharp
public partial class MainWindow : Window
{
    public List<ModuleBase> activeModules = new();
    public List<string> pythonModules = new();
    public static string currentFileName = "";
    public static ModuleHandler moduleHandler = new();
    public static UKS.UKS theUKS = moduleHandler.theUKS;
}
```

**Important Methods**:
- `MainWindow_Loaded()`: Application initialization
- `FireModules()`: Main execution loop
- File I/O methods for UKS persistence

#### MainWindowEventHandlers.cs
**Purpose**: UI event handling separation
**Contains**:
- Menu click handlers
- Window state management
- User interaction responses

#### MainWindowFiles.cs
**Purpose**: File operations management
**Key Features**:
- UKS save/load operations
- File format handling
- Error recovery mechanisms

#### MainWindowPythonModules.cs
**Purpose**: Python integration bridge
**Functionality**:
- Python module discovery
- Inter-process communication
- Python environment management

### Module System Implementation

#### ModuleHandler.cs
**Purpose**: Central module management
```csharp
public class ModuleHandler
{
    public UKS.UKS theUKS = new();
    private List<ModuleBase> availableModules;
    
    public void LoadModules()
    public void FireActiveModules()
    public void RegisterModule(ModuleBase module)
}
```

#### ModuleViewMenu.cs
**Purpose**: Dynamic menu generation for modules
**Features**:
- Runtime menu creation
- Module state management
- UI integration handling

### Modules Directory Structure

```
Modules/
├── Core UKS Modules
│   ├── ModuleUKS.cs                # Direct UKS interface
│   ├── ModuleUKSDlg.xaml/.cs       # UKS dialog
│   ├── ModuleUKSQuery.cs           # Query interface
│   ├── ModuleUKSStatement.cs       # Statement creation
│   └── ModuleUKSClause.cs          # Clause management
├── Processing Modules
│   ├── ModuleAttributeBubble.cs    # Attribute propagation
│   ├── ModuleBalanceTree.cs        # Tree operations
│   ├── ModuleRemoveRedundancy.cs   # Deduplication
│   └── ModuleClassCreate.cs        # Classification
├── AI Integration
│   ├── ModuleGPTInfo.cs            # GPT integration
│   └── ModuleOnlineInfo.cs         # Web information
├── Utilities
│   ├── ModuleStressTest.cs         # Performance testing
│   └── ModuleAddCounts.cs          # Statistics
└── Vision/ (Subdirectory)
    └── Vision-related modules
```

#### Module Base Architecture

**ModuleBase.cs** - Abstract base class:
```csharp
abstract public class ModuleBase
{
    // Core properties
    public bool initialized = false;
    public bool isEnabled = true;
    public string Label = "";
    
    // UI management
    protected ModuleBaseDlg dlg = null;
    public Point dlgPos, dlgSize;
    public bool dlgIsOpen = false;
    
    // UKS connection
    public UKS.UKS theUKS = null;
    
    // Abstract interface
    abstract public void Fire();
    abstract public void Initialize();
    
    // Lifecycle hooks
    public virtual void UKSInitializedNotification() { }
    public virtual void UKSReloadedNotification() { }
}
```

**ModuleBaseDlg.cs** - Dialog base class:
```csharp
public class ModuleBaseDlg : UserControl
{
    public virtual void UpdateDialog() { }
    public virtual void Save() { }
    public virtual void Load() { }
}
```

### Utility Classes

#### Utils.cs
**Purpose**: Common utility functions
**Key Features**:
- File path management
- String manipulation helpers
- Type conversion utilities
- Debugging support

#### Network.cs
**Purpose**: Network operations for modules
**Functionality**:
- HTTP request handling
- Data serialization
- Connection management

#### PointPlus.cs
**Purpose**: Extended point mathematics
**Usage**: Geometric calculations for vision modules

### Resource Management

#### Resources Directory
```
Resources/
├── Images/                 # UI icons and graphics
├── Styles/                # WPF styling resources
└── Templates/             # XAML templates
```

#### ModuleDescriptions.xml
**Purpose**: Module metadata and configuration
```xml
<ModuleDescriptions>
  <Module Name="ModuleUKS" 
          Description="Direct UKS Interface"
          Assembly="BrainSimulator" />
  <Module Name="ModuleGPTInfo" 
          Description="GPT Integration"
          RequiresNetwork="true" />
</ModuleDescriptions>
```

## UKS Project Architecture

### Core UKS Implementation

#### UKS.cs
**Purpose**: Main UKS class with core functionality
```csharp
public partial class UKS
{
    static private List<Thing> uKSList = new() { Capacity = 1000000, };
    public IList<Thing> UKSList { get => uKSList; }
    
    public UKS() { /* Constructor logic */ }
    public virtual Thing AddThing(string label, Thing parent)
    public virtual void DeleteThing(Thing t)
}
```

#### UKS.File.cs
**Purpose**: File I/O operations
**Key Methods**:
- `SaveToXMLFile(string fileName)`
- `LoadFromXMLFile(string fileName)`
- Serialization helpers
- Backup and recovery

#### UKS.Query.cs
**Purpose**: Knowledge querying system
**Query Types**:
- Direct relationship queries
- Inheritance-based queries
- Complex multi-hop queries
- Pattern matching

```csharp
public IList<Thing> GetChildren(Thing parent, Thing relType = null)
public IList<Relationship> GetRelationships(Thing source, Thing relType = null)
public IList<Thing> Query(QueryParameters params)
```

#### UKS.Statement.cs
**Purpose**: Knowledge assertion and modification
**Operations**:
- Statement creation
- Relationship management
- Knowledge validation
- Consistency checking

### Knowledge Representation

#### Thing.cs
**Purpose**: Knowledge node implementation
```csharp
public partial class Thing
{
    private List<Relationship> relationships = new();
    private List<Relationship> relationshipsFrom = new();
    private string label = "";
    object value;
    
    // Implicit string conversion
    public static implicit operator Thing(string label)
    
    // Relationship management
    public Relationship AddRelationship(Thing target, Thing relType)
    public void RemoveRelationship(Relationship r)
}
```

**Key Features**:
- Implicit string conversion for ease of use
- Thread-safe relationship lists
- Usage tracking for optimization
- Value attachment for arbitrary data

#### Relationship.cs
**Purpose**: Knowledge edge implementation
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
    
    public List<Clause> Clauses = new();
}
```

**Advanced Features**:
- Weighted relationships
- Usage statistics
- Time-to-live for temporary knowledge
- Clause support for conditional logic

#### ThingLabels.cs
**Purpose**: Efficient label-to-Thing mapping
```csharp
public static class ThingLabels
{
    private static Dictionary<string, Thing> labelDictionary = new();
    
    public static Thing GetThing(string label)
    public static void AddThing(Thing thing)
    public static void RemoveThing(Thing thing)
    public static void ClearLabelList()
}
```

## Python Project Structure

### Python Integration Architecture

#### MainWindow.py
**Purpose**: Primary Python interface
```python
class MainWindow(ViewBase):
    def __init__(self, level: Union[tk.Tk, tk.Toplevel]) -> None:
        self.setupUKS()
        self.build()
    
    def setupcontent(self):
        # Module listing
        # File operations
```

#### utils.py
**Purpose**: Python utility base classes
```python
class ViewBase:
    def __init__(self, title: str, level, module_type: str):
        self.title = title
        self.level = level
        self.module_type = module_type
    
    def setupUKS(self):
        # UKS bridge initialization
    
    def fire(self) -> bool:
        # Default processing
```

### Python Module Templates

#### module_template.py
**Purpose**: Standard template for new Python modules
**Structure**:
- ViewBase inheritance
- Required interface methods
- UI setup patterns
- UKS integration examples

#### Specialized Modules

**module_uks_tree.py**: Tree visualization
**module_add_statement.py**: Knowledge creation interface

### Python-C# Bridge

The integration works through:
1. **Process Communication**: Inter-process messaging
2. **Shared Data**: UKS access through bridge
3. **Event Synchronization**: State management

## Navigation Tips

### Finding Specific Functionality

#### UKS Operations
- **Core UKS**: `UKS/UKS.cs`
- **Queries**: `UKS/UKS.Query.cs`
- **File I/O**: `UKS/UKS.File.cs`
- **Knowledge Creation**: `UKS/UKS.Statement.cs`

#### Module Development
- **Base Classes**: `BrainSimulator/Modules/ModuleBase.cs`
- **Examples**: Any `Module*.cs` in Modules directory
- **UI Patterns**: `Module*Dlg.xaml` files

#### Python Integration
- **Bridge Code**: `BrainSimulator/MainWindowPythonModules.cs`
- **Python Base**: `PythonProj/utils.py`
- **Templates**: `PythonProj/module_template.py`

#### Application Startup
- **Entry Point**: `BrainSimulator/App.xaml.cs`
- **Main Window**: `BrainSimulator/MainWindow.xaml.cs`
- **Initialization**: `MainWindow_Loaded()` method

### Code Search Strategies

#### Using Visual Studio
1. **Solution Explorer**: Navigate project structure
2. **Find All References**: F12 on any symbol
3. **Go to Definition**: Right-click → Go to Definition
4. **Search Solution**: Ctrl+Shift+F for text search

#### Common Search Patterns
- `abstract public void Fire()`: Find all module implementations
- `AddRelationship`: UKS relationship operations
- `theUKS`: UKS usage patterns
- `ModuleBase`: Module inheritance patterns

### Debugging Locations

#### Key Breakpoint Locations
- `MainWindow_Loaded()`: Application startup
- `ModuleBase.Fire()`: Module execution
- `UKS.AddThing()`: Knowledge creation
- `UKS.Query()`: Knowledge retrieval

#### Log Output Locations
- Visual Studio Output window
- Debug console for Python modules
- Application event logs

## Development Patterns

### Common Code Patterns

#### Module Implementation Pattern
```csharp
public class ModuleExample : ModuleBase
{
    private SomeData moduleData;
    
    public override void Initialize()
    {
        // Setup module state
        GetUKS();
        moduleData = new SomeData();
    }
    
    public override void Fire()
    {
        // Check if ready
        if (!initialized) Init();
        
        // Core processing
        ProcessData();
        
        // UI updates
        UpdateDialog();
    }
}
```

#### UKS Usage Pattern
```csharp
// Get UKS reference
theUKS = MainWindow.theUKS;

// Create knowledge
var dog = theUKS.AddThing("dog", "animal");
dog.AddRelationship("4 legs", "has-property");

// Query knowledge
var animals = theUKS.GetChildren("animal");
var properties = theUKS.GetRelationships(dog, "has-property");
```

#### Python Module Pattern
```python
class ViewExample(ViewBase):
    def __init__(self, level):
        super().__init__("Example", level, __file__)
        self.setupUKS()
        self.build()
    
    def fire(self) -> bool:
        if self.update_paused:
            return True
        
        # Processing logic
        self.level.update()
        return self.level.winfo_exists()
```

### Testing and Validation

#### Unit Testing Locations
- **UKS Tests**: `UKS/` project (test methods in classes)
- **Python Tests**: `TestPython/` directory
- **Integration Tests**: Within module implementations

#### Manual Testing Approaches
- Use ModuleStressTest for performance validation
- Use ModuleUKS for knowledge structure verification
- Use Python modules for cross-language integration testing

---

This navigation guide provides the foundation for understanding and working with the BrainSimY codebase. The modular architecture, combined with the UKS knowledge system, creates a powerful platform for AI development and experimentation.