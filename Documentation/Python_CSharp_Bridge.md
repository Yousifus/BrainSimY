# Python-C# Bridge Documentation

## Overview

BrainSimY implements a sophisticated Python-C# bridge using Python.NET that enables seamless integration between Python modules and the C# Universal Knowledge Store (UKS). This bridge allows Python developers to access the full power of the UKS while writing modules in Python's more flexible and rapid development environment.

## Architecture

### Bridge Components

1. **Python.NET Runtime**: Provides .NET CLR hosting in Python processes
2. **ViewBase Class**: Python base class that standardizes module interfaces
3. **Module Handler**: C# class that manages Python module lifecycle
4. **Interop Layer**: Handles data type conversion and method calls between environments

### Communication Flow

```
Python Module → Python.NET → CLR Bridge → C# UKS → Knowledge Store
     ↑                                           ↓
UI Events ←─────────── C# MainWindow ←─── Module Manager
```

## Setting Up the Bridge

### C# Side Configuration

The bridge is initialized in `MainWindowPythonModules.cs`:

```csharp
public static List<(string, dynamic)> activePythonModules = new();

static void RunScript(string moduleLabel)
{
    // Get module type from UKS
    Thing tModule = theUKS.Labeled(moduleLabel);
    Thing tModuleType = tModule.Parents.FindFirst(x => x.HasAncestorLabeled("AvailableModule"));
    string moduleType = tModuleType.Label.Replace(".py", "");
    
    // Initialize or run existing module
    if (!activePythonModules.Any(x => x.Item1 == moduleLabel))
    {
        // First time - initialize module
        dynamic module = Python.Runtime.Py.Import(moduleType);
        module.Init();
        activePythonModules.Add((moduleLabel, module));
    }
    else
    {
        // Subsequent calls - just fire the module
        var activeModule = activePythonModules.First(x => x.Item1 == moduleLabel);
        activeModule.Item2.Fire();
    }
}
```

### Python Side Configuration

Every Python module must use the standard bridge setup in `utils.py`:

```python
## Import UKS.dll from C# modules
import pythonnet
pythonnet.load("coreclr")
import clr
clr.AddReference("UKS")
from UKS import *

# Create global UKS instance
uks = None
try:
    uks = UKS()
except Exception as e:
    print(e)
```

## Module Development Patterns

### Standard Module Template

All Python modules should inherit from `ViewBase` and follow this pattern:

```python
import sys, os
from typing import Union
import tkinter as tk
from utils import ViewBase

class MyModule(ViewBase):
    def __init__(self, level: Union[tk.Tk, tk.Toplevel]) -> None:
        title: str = "My Module Title"
        super(MyModule, self).__init__(
            title=title, 
            level=level, 
            module_type=os.path.basename(__file__)
        )
        self.build()
    
    def build(self):
        """Build the UI components"""
        self.level.geometry("400x300+100+100")
        # Add your UI elements here
        
        # Required for standalone debugging
        if sys.argv[0] != "":
            self.level.mainloop()
    
    def fire(self) -> bool:
        """Main execution loop - called repeatedly"""
        # Your module logic here
        
        # Always update the UI
        self.level.update()
        return self.level.winfo_exists()

# Required exposed functions for C# integration
def Init():
    global view
    view = MyModule(level=tk.Tk())

def Fire() -> bool:
    return view.fire()
    
def GetHWND() -> int:
    return view.level.frame()

def SetLabel(label: str):
    view.setLabel(label)
    
def Close():
    view.close()

# Standalone execution for debugging
if sys.argv[0] != "":
    Init()
```

### Required Exposed Functions

Every Python module must expose these functions for C# integration:

1. **Init()**: Creates the module instance
2. **Fire()**: Executes the module logic (called repeatedly)
3. **GetHWND()**: Returns window handle for embedding
4. **SetLabel(label)**: Sets the module instance label
5. **Close()**: Cleanup and close the module

## UKS Integration Examples

### Basic Knowledge Operations

```python
def add_knowledge_example(self):
    """Example of adding knowledge to UKS from Python"""
    # Add simple statements
    self.uks.AddStatement("dog", "is-a", "animal")
    self.uks.AddStatement("animal", "is-a", "living-thing")
    
    # Add properties
    self.uks.AddStatement("dog", "has", "fur")
    self.uks.AddStatement("dog", "can", "bark")
    
    # Query knowledge
    dog_thing = self.uks.Labeled("dog")
    if dog_thing is not None:
        # Get all relationships
        relationships = dog_thing.Relationships
        for rel in relationships:
            print(f"{rel.source.Label} {rel.relType.Label} {rel.target.Label}")
```

### Interactive Knowledge Entry

Example from `module_add_statement.py`:

```python
def submit_input(self):
    """Process input from UI and add to UKS"""
    src: str = self.input_src.get()
    rel: str = self.input_rel.get()
    tgt: str = self.input_tgt.get()
    
    # Add statement to UKS
    self.uks.AddStatement(src, rel, tgt)
    print(f"Added: [{src}, {rel}, {tgt}]")

def text_changed(self, sv: tk.StringVar, e: tk.Entry) -> None:
    """Visual feedback for known/unknown Things"""
    s: str = sv.get()
    if self.uks.Labeled(s) is not None:
        e.configure(bg="lightyellow")  # Thing exists
    else:
        e.configure(bg="pink")  # New Thing
```

### Tree Visualization

Example from `module_uks_tree.py`:

```python
def add_children(self, parent_id: str, item_label: str) -> None:
    """Recursively add UKS children to tree view"""
    if self.curr_depth >= MAX_DEPTH:
        return
        
    parent_thing = self.uks.Labeled(item_label)
    if parent_thing is None:
        return
        
    children = parent_thing.Children
    for child in children:
        try:
            iid: str = self.tree_view.insert(
                parent=parent_id, 
                index="end", 
                iid=child.ToString(),
                text=child.ToString()
            )
            
            # Recursively add grandchildren
            self.curr_depth += 1
            self.add_children(iid, child.Label)
            self.curr_depth -= 1
            
        except Exception as e:
            print(f"Error adding child: {e}")
```

## Data Type Conversion

### Automatic Conversions

The bridge automatically handles most data type conversions:

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| str | string | Direct conversion |
| int | int | Direct conversion |
| float | float | Direct conversion |
| bool | bool | Direct conversion |
| list | List<T> | Elements converted recursively |
| None | null | Direct mapping |

### UKS-Specific Types

UKS types are exposed directly to Python:

```python
# These work identically in Python and C#
thing = uks.Labeled("concept")  # Returns Thing object
relationship = uks.AddStatement("a", "rel", "b")  # Returns Relationship
children = thing.Children  # Returns IList<Thing>
```

### Collection Handling

C# collections are accessible in Python with some considerations:

```python
# Read-only collections (safe)
things = uks.UKSList  # IList<Thing>
relationships = thing.Relationships  # IList<Relationship>

# Iterate safely
for thing in things:
    print(thing.Label)

# Convert to Python list if needed
python_list = list(things)
```

## Error Handling

### Bridge-Specific Exceptions

```python
try:
    thing = uks.Labeled("nonexistent")
    if thing is None:
        print("Thing not found")
except Exception as e:
    print(f"Bridge error: {e}")
```

### Common Issues and Solutions

1. **Assembly Loading Errors**
   ```python
   # Solution: Ensure UKS.dll is in the correct path
   import os
   os.chdir(correct_path)
   clr.AddReference("UKS")
   ```

2. **Type Conversion Errors**
   ```python
   # Problem: Passing Python object where C# object expected
   # Solution: Use proper type conversion
   thing_list = [uks.Labeled("item1"), uks.Labeled("item2")]
   # Filter out None values
   valid_things = [t for t in thing_list if t is not None]
   ```

3. **Threading Issues**
   ```python
   # UKS is thread-safe, but UI updates must be on main thread
   def safe_uks_operation(self):
       result = self.uks.Labeled("something")  # Safe from any thread
       # UI updates must be on main thread
       self.level.after(0, lambda: self.update_ui(result))
   ```

## Performance Optimization

### Batch Operations

```python
def batch_knowledge_creation(self):
    """Efficient batch knowledge creation"""
    # Collect all statements first
    statements = [
        ("dog", "is-a", "animal"),
        ("cat", "is-a", "animal"),
        ("bird", "is-a", "animal"),
        # ... more statements
    ]
    
    # Add in batch (reduces bridge overhead)
    for src, rel, tgt in statements:
        self.uks.AddStatement(src, rel, tgt)
```

### Caching Strategies

```python
class OptimizedModule(ViewBase):
    def __init__(self, level):
        super().__init__("Optimized", level, __file__)
        self._thing_cache = {}
    
    def get_cached_thing(self, label: str):
        """Cache frequently accessed Things"""
        if label not in self._thing_cache:
            self._thing_cache[label] = self.uks.Labeled(label)
        return self._thing_cache[label]
```

## Module Communication

### Inter-Module Communication

Modules can communicate through the shared UKS:

```python
def send_message_to_other_module(self, target_module: str, message: str):
    """Send message via UKS to another module"""
    # Create temporary communication node
    msg_thing = self.uks.GetOrAddThing(f"msg_{int(time.time())}", "message")
    msg_thing.V = message  # Store message content
    
    # Link to target module
    self.uks.AddStatement(target_module, "has-message", msg_thing.Label)

def check_for_messages(self, my_module_label: str):
    """Check for messages in UKS"""
    my_thing = self.uks.Labeled(my_module_label)
    if my_thing:
        for rel in my_thing.Relationships:
            if rel.relType.Label == "has-message":
                message_thing = rel.target
                message_content = message_thing.V
                # Process message
                self.process_message(message_content)
                # Clean up
                my_thing.RemoveRelationship(rel)
```

### Event Broadcasting

```python
def broadcast_event(self, event_type: str, event_data: dict):
    """Broadcast event to all listening modules"""
    event_thing = self.uks.GetOrAddThing(f"event_{event_type}_{int(time.time())}")
    event_thing.V = event_data
    
    # Add to event queue
    self.uks.AddStatement("event-queue", "has", event_thing.Label)
```

## Debugging and Development

### Standalone Development

```python
# Enable standalone running for development
if __name__ == "__main__":
    import tkinter as tk
    root = tk.Tk()
    module = MyModule(root)
    root.mainloop()
```

### Debug Utilities

```python
def debug_uks_state(self):
    """Print current UKS state for debugging"""
    print(f"Total Things: {len(list(self.uks.UKSList))}")
    
    # Print all Things with relationships
    for thing in self.uks.UKSList:
        if thing.Relationships:
            print(f"\n{thing.Label}:")
            for rel in thing.Relationships:
                print(f"  {rel}")
```

### Error Logging

```python
import logging

class LoggingModule(ViewBase):
    def __init__(self, level):
        super().__init__("Logger", level, __file__)
        self.setup_logging()
    
    def setup_logging(self):
        logging.basicConfig(
            filename='module.log',
            level=logging.DEBUG,
            format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
        )
        self.logger = logging.getLogger(self.__class__.__name__)
    
    def safe_uks_operation(self, operation_name: str, operation):
        try:
            result = operation()
            self.logger.info(f"{operation_name} succeeded")
            return result
        except Exception as e:
            self.logger.error(f"{operation_name} failed: {e}")
            return None
```

## Best Practices

### Module Design
1. **Single Responsibility**: Each module should have one clear purpose
2. **Stateless Operations**: Prefer stateless operations when possible
3. **Error Handling**: Always handle bridge exceptions gracefully
4. **Resource Cleanup**: Properly close windows and clean up resources

### UKS Integration
1. **Label Conventions**: Use consistent naming conventions for Things
2. **Relationship Types**: Reuse standard relationship types when possible
3. **Conflict Avoidance**: Be careful not to create conflicting knowledge
4. **Performance**: Cache frequently accessed Things

### Threading Considerations
1. **UI Thread**: All UI operations must be on the main thread
2. **UKS Thread Safety**: UKS operations are thread-safe
3. **Background Processing**: Use threading for long-running operations
4. **Synchronization**: Use proper synchronization when needed

This bridge enables powerful hybrid applications that combine Python's flexibility with C#'s performance and the UKS's sophisticated knowledge representation capabilities.