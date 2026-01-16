---
name: cui-autobind
description: This skill should be used when the user needs to work with the CUiAutoBind Unity Package, including creating UI bindings, configuring the AutoBind system, generating code, or setting up the tool in a Unity project. It provides specialized knowledge for working with Unity's UI binding workflow, managing ScriptableObject configurations, and understanding the code generation system.
---

# CUiAutoBind Skill

CUiAutoBind is a Unity UI automatic binding system designed to accelerate UI development by automating the generation of UI component binding code. This skill provides specialized knowledge for working with the CUiAutoBind package.

## Purpose

Enable efficient UI development in Unity by:
- Automatically generating UI binding code
- Providing flexible configuration through ScriptableObject
- Supporting incremental code updates that preserve manual edits
- Offering comprehensive editor tools for UI management

## When to Use This Skill

Use this skill when:
- Setting up CUiAutoBind in a Unity project
- Creating or modifying UI bindings with the AutoBind component
- Configuring generation parameters (namespace, base class, interfaces)
- Generating or updating binding code
- Troubleshooting issues with code generation or binding
- Extending the system with custom components

## Core Components

### AutoBind Component
The AutoBind component marks GameObjects for automatic code generation. Key features:
- Stores binding data linking GameObject components to field names
- Supports all Unity built-in components and user-defined components
- Provides inspector interface for manual binding configuration

### AutoBindData
Binding data structure containing:
- `component`: The Component to bind
- `fieldName`: The generated field name
- `generateField`: Boolean flag to control field generation

### BindConfig (ScriptableObject)
Configuration asset managing code generation parameters:
- `namespaceName`: Namespace for generated code (default: "UI")
- `basePath`: Base path for file generation (default: "Scripts/UI/Auto/")
- `baseClass`: Base class for generated classes (default: "MonoBehaviour")
- `interfaces`: Array of interfaces to implement
- `usePartialClass`: Use partial class for incremental updates (default: true)
- `addFieldComments`: Add comments to generated fields (default: true)

## Workflow

### Setting Up CUiAutoBind

1. Install the package by copying the CUiAutoBind folder to the project's Assets directory
2. Open the editor window via `Tools/CUiAutoBind/打开窗口`
3. Create or verify the AutoBindConfig in `Assets/CUiAutoBind/Resources/AutoBindConfig.asset`

### Creating UI Bindings

**Option 1: Manual Binding**
1. Select the UI GameObject
2. Add the AutoBind component
3. Click "添加新绑定" to create a new binding entry
4. Select the component and specify the field name

**Option 2: Automatic Binding**
1. Select the UI GameObject
2. Add the AutoBind component
3. Click "从当前 GameObject 添加组件" to auto-detect components
4. Review and adjust the generated field names as needed

### Generating Code

**Single GameObject**
- In the AutoBind component Inspector, click "生成绑定代码"

**Batch Generation**
- Open the AutoBind window (`Tools/CUiAutoBind/打开窗口`)
- Review the list of AutoBind components in the scene
- Click individual "生成代码" buttons or "全部生成" for batch processing

### Customizing Generated Code

Generated files use partial class structure with two regions:

**AutoBind Generated Region**
```
#region AutoBind Generated
public Button startButton;
public Text titleText;
#endregion AutoBind Generated
```
- Do NOT manually edit this region
- Content is overwritten on regeneration

**Manual Code Region**
```
#region Manual Code
private void Start() {
    startButton.onClick.AddListener(OnStartClick);
}
#endregion Manual Code
```
- Add all custom logic here
- Content is preserved on regeneration

### Incremental Updates

When UI structure changes:
1. Modify AutoBind component bindings (add/remove/edit)
2. Regenerate code using the same method
3. System updates AutoBind Generated region
4. Manual Code region remains intact

## Configuration Examples

### Basic Configuration
```csharp
Namespace: UI
Base Path: Scripts/UI/Auto/
Base Class: MonoBehaviour
Interfaces: (empty)
```

Generated class:
```csharp
namespace UI {
    public partial class MainMenuUI : MonoBehaviour {
        // Auto-generated fields...
    }
}
```

### With Base Class and Interface
```csharp
Namespace: GameUI
Base Path: Scripts/Game/UI/
Base Class: BaseUIController
Interfaces: ["IUIPanel", "IUpdateable"]
```

Generated class:
```csharp
namespace GameUI {
    public partial class MainMenuUI : BaseUIController, IUIPanel, IUpdateable {
        // Auto-generated fields...
    }
}
```

## Code Generation Details

### File Structure
Generated files are placed at: `{basePath}/{GameObjectName}/{GameObjectName}.cs`

Example: For GameObject "MainMenu" with default config:
- Path: `Scripts/UI/Auto/MainMenu/MainMenu.cs`

### Field Naming
- Component types automatically convert to camelCase
- Common abbreviations: "TextMeshPro" → "TMP", "Component" → ""
- Examples:
  - Button → button
  - TMP_Text → tmpText
  - RawImage → rawImage

### Component Type Support
All Component-derived types are supported:
- Unity UI: Button, Text, Image, Toggle, Slider, ScrollRect, etc.
- TextMeshPro: TMP_Text, TMP_Dropdown, TMP_InputField, etc.
- User-defined: Any custom Component class

## Troubleshooting

### Generated Code Not Found
- Ensure the generated class name matches GameObject name
- Check that the file exists at the expected path
- Verify namespace settings match usage location

### Manual Code Disappears
- Confirm manual code is in `#region Manual Code` section
- Check that region tags are properly formatted
- Verify `usePartialClass` is enabled in config

### Compilation Errors
- Check for duplicate field names in bindings
- Ensure component types are valid and accessible
- Verify namespace and using statements are correct

### Component Not Detected
- Ensure the component is added to the GameObject
- Check that the component derives from Component
- Verify the component is not abstract or an interface

## Best Practices

1. **Use Partial Classes**: Always keep custom logic in the Manual Code region
2. **Descriptive Field Names**: Adjust auto-generated names to be more descriptive
3. **Consistent Namespace**: Use the same namespace across all generated files
4. **Base Class Pattern**: Create a base UI class for common functionality
5. **Regular Regeneration**: Regenerate after structural UI changes
6. **Version Control**: Include generated files in version control

## Reference Files

For detailed implementation information, consult:
- `Runtime/AutoBind.cs` - Core component implementation
- `Runtime/BindConfig.cs` - Configuration class details
- `Editor/CodeGenerator.cs` - Code generation logic
- `Editor/AutoBindWindow.cs` - Editor window implementation
- `README.md` - Complete user documentation

## Notes

- The system uses `ExecuteAlways` on AutoBind for editor-time operation
- Generated code uses `public` fields for direct access (consider changing to properties if needed)
- The system preserves user code through region-based parsing
- Config files are stored as ScriptableObject assets for easy version control
