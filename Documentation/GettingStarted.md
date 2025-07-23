# Getting Started with BrainSimY

## Introduction

This guide will walk you through setting up, building, and running BrainSimY on your development machine. The system supports Windows as the primary platform with experimental macOS support.

## System Requirements

### Minimum Requirements
- **Operating System**: Windows 10/11 (64-bit) or macOS 10.15+
- **Memory**: 4 GB RAM (8 GB recommended)
- **Storage**: 2 GB available space
- **.NET**: .NET 8.0 SDK or later
- **Python**: Python 3.8+ (optional, for Python modules)

### Recommended Development Environment
- **Visual Studio 2022** (Community Edition or higher)
- **Python 3.9+** with tkinter support
- **Git** for version control
- **Windows Terminal** or PowerShell for command line operations

## Prerequisites Installation

### 1. Install .NET 8.0 SDK

#### Windows
1. Download .NET 8.0 SDK from [Microsoft's official site](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Run the installer and follow the setup wizard
3. Verify installation:
   ```cmd
   dotnet --version
   ```

#### macOS
```bash
# Using Homebrew
brew install dotnet

# Or download from Microsoft
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. Install Python (Optional)

#### Windows
1. Download Python from [python.org](https://www.python.org/downloads/)
2. **Important**: Check "Add Python to PATH" during installation
3. Verify installation:
   ```cmd
   python --version
   pip --version
   ```

#### macOS
```bash
# Using Homebrew
brew install python-tk

# Or use system Python with tkinter
python3 -m tkinter
```

### 3. Install Visual Studio (Recommended)

#### Windows
1. Download Visual Studio 2022 Community (free)
2. During installation, ensure these workloads are selected:
   - **.NET desktop development**
   - **Python development** (if using Python modules)
   - **Game development with C#** (for WPF components)

## Getting the Source Code

### Clone the Repository
```bash
git clone <repository-url>
cd BrainSimY
```

### Repository Structure Overview
```
BrainSimY/
├── BrainSimulator/          # Main WPF application
├── UKS/                     # Universal Knowledge Store
├── PythonProj/             # Python integration
├── BrainSimMAC/            # macOS compatibility
├── TestPython/             # Python testing framework
├── Documentation/          # Project documentation
├── BrainSimulator.sln      # Visual Studio solution
└── README.md
```

## Building the Project

### Using Visual Studio (Recommended)

1. **Open the Solution**
   ```
   File → Open → Project/Solution
   Navigate to BrainSimulator.sln
   ```

2. **Set Startup Project**
   - Right-click "BrainSimulator" in Solution Explorer
   - Select "Set as Startup Project"

3. **Restore NuGet Packages**
   ```
   Tools → NuGet Package Manager → Package Manager Console
   ```
   Run: `dotnet restore`

4. **Build Solution**
   - Press `Ctrl+Shift+B` or
   - Build → Build Solution

### Using Command Line

```bash
# Navigate to project root
cd BrainSimY

# Restore packages
dotnet restore

# Build entire solution
dotnet build

# Or build specific project
dotnet build BrainSimulator/BrainSimulator.csproj
```

### Build Configurations

#### Debug Configuration (Default)
- Includes debugging symbols
- Optimizations disabled
- Suitable for development

```bash
dotnet build --configuration Debug
```

#### Release Configuration
- Optimized for performance
- Debugging symbols removed
- Suitable for distribution

```bash
dotnet build --configuration Release
```

## Python Environment Setup

### 1. Configure Python Path

#### Windows
1. Find your Python installation path:
   ```cmd
   where python
   ```
2. Set environment variable:
   - Open System Properties → Advanced → Environment Variables
   - Add user variable: `PythonPath` = `C:\Path\To\Python\python.exe`

#### Alternative: Set via BrainSimulator
- On first run, BrainSimulator will prompt for Python path
- Navigate to your Python executable
- The path will be saved automatically

### 2. Install Python Dependencies

```bash
# Navigate to Python project
cd PythonProj

# Install required packages
pip install tkinter typing
```

### 3. Test Python Integration

```bash
# Run Python main window independently
cd PythonProj
python MainWindow.py
```

## Running BrainSimY

### Method 1: Visual Studio
1. Ensure "BrainSimulator" is set as startup project
2. Press `F5` (Debug) or `Ctrl+F5` (Run without debugging)

### Method 2: Command Line
```bash
# From project root
dotnet run --project BrainSimulator

# Or from BrainSimulator directory
cd BrainSimulator
dotnet run
```

### Method 3: Execute Built Binary
```bash
# After building
cd BrainSimulator/bin/Debug/net8.0-windows
./BrainSimulator.exe
```

## First Run Experience

### Initial Setup

1. **Python Configuration**
   - If Python modules desired, configure Python path when prompted
   - Click "Yes" when asked about Python modules
   - Navigate to your Python executable

2. **UKS Initialization**
   - The system will create an initial knowledge structure
   - Basic concepts and relationships are established automatically

3. **UI Overview**
   - Main window displays available modules
   - Menu bar provides file operations and module management
   - Status bar shows system information

### Basic Operations

#### Loading Sample Knowledge
1. **File → Open** (or Ctrl+O)
2. Navigate to sample UKS files (if available)
3. Select a `.xml` file containing knowledge data

#### Exploring Modules
1. **View → Modules** to see available modules
2. Double-click a module to open its interface
3. Try "ModuleUKS" for direct knowledge exploration

#### Creating Knowledge
1. Open "ModuleUKSStatement" 
2. Add simple statements like:
   - Source: "dog"
   - Relationship: "is-a"
   - Target: "animal"

## Troubleshooting

### Common Issues

#### Build Errors

**"SDK not found" error**
```bash
# Verify .NET installation
dotnet --list-sdks

# Reinstall .NET 8.0 SDK if missing
```

**"Project file not found"**
- Ensure you're in the correct directory
- Check that .csproj files exist

**NuGet restore failures**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

#### Python Integration Issues

**"Python not found" error**
- Verify Python installation: `python --version`
- Check PATH environment variable
- Reconfigure Python path in BrainSimulator

**"tkinter not found" error**
```bash
# Windows: Reinstall Python with tkinter
# macOS: Install python-tk
brew install python-tk
```

#### Runtime Issues

**"UKS initialization failed"**
- Check file permissions in application directory
- Ensure sufficient disk space
- Try running as administrator (Windows)

**Module loading errors**
- Verify module files exist in Modules directory
- Check ModuleDescriptions.xml for configuration
- Review application logs for specific errors

### Debugging Tips

#### Enable Detailed Logging
1. Open App.config (if present)
2. Add logging configuration
3. Check output window in Visual Studio

#### Module Development Debugging
```csharp
// Add debug output in module Fire() method
System.Diagnostics.Debug.WriteLine($"Module {Label} executing");
```

#### Python Module Debugging
```python
# Add print statements in fire() method
def fire(self) -> bool:
    print(f"Python module {self.title} firing")
    return True
```

## Development Workflow

### Recommended Workflow

1. **Setup Development Environment**
   - Install all prerequisites
   - Clone repository
   - Build solution

2. **Understand the Architecture**
   - Read ProjectOverview.md
   - Study UKS_Explanation.md
   - Review ModuleSystem.md

3. **Start with Existing Modules**
   - Examine ModuleUKS for basic UKS interaction
   - Look at Python template for Python development

4. **Create Your First Module**
   - Copy existing module as template
   - Implement required abstract methods
   - Test integration with UKS

### Code Modification Workflow

1. **Make Changes**
   - Edit source files
   - Add new modules

2. **Build and Test**
   ```bash
   dotnet build
   dotnet run --project BrainSimulator
   ```

3. **Debug Issues**
   - Use Visual Studio debugger
   - Add logging statements
   - Test individual components

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "Description of changes"
   ```

## Next Steps

After successful setup:

1. **Explore the UKS**: Use ModuleUKS to understand knowledge representation
2. **Try Python Modules**: Experiment with Python integration
3. **Study the Code**: Review existing modules for patterns
4. **Read Advanced Documentation**: 
   - CodeStructure.md for codebase navigation
   - FutureEnhancements.md for improvement opportunities

### Learning Path

1. **Beginner**: Run existing modules, explore UKS through UI
2. **Intermediate**: Modify existing modules, create simple new modules
3. **Advanced**: Develop complex modules, contribute to core UKS functionality

## Getting Help

### Resources
- **Documentation**: Read all markdown files in Documentation/
- **Code Comments**: Extensive inline documentation in source
- **Future AI Society**: Join community meetings for support

### Community
- **Online Meetings**: Regular development discussions
- **Collaborative Development**: Contribute to the open-source community
- **Educational Mission**: Help advance AI research

---

Welcome to BrainSimY! This system represents cutting-edge research in AI knowledge representation. Take time to understand the concepts, experiment with the modules, and consider how you can contribute to advancing artificial intelligence through common-sense reasoning.