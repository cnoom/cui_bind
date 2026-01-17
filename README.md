# CUiAutoBind [![zread](https://img.shields.io/badge/Ask_Zread-_.svg?style=flat&color=00b0aa&labelColor=000000&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTQuOTYxNTYgMS42MDAxSDIuMjQxNTZDMS44ODgxIDEuNjAwMSAxLjYwMTU2IDEuODg2NjQgMS42MDE1NiAyLjI0MDFWNC45NjAxQzEuNjAxNTYgNS4zMTM1NiAxLjg4ODEgNS42MDAxIDIuMjQxNTYgNS42MDAxSDQuOTYxNTZDNS4zMTUwMiA1LjYwMDEgNS42MDE1NiA1LjMxMzU2IDUuNjAxNTYgNC45NjAxVjIuMjQwMUM1LjYwMTU2IDEuODg2NjQgNS4zMTUwMiAxLjYwMDEgNC45NjE1NiAxLjYwMDFaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik00Ljk2MTU2IDEwLjM5OTlIMi4yNDE1NkMxLjg4ODEgMTAuMzk5OSAxLjYwMTU2IDEwLjY4NjQgMS42MDE1NiAxMS4wMzk5VjEzLjc1OTlDMS42MDE1NiAxNC4xMTM0IDEuODg4MSAxNC4zOTk5IDIuMjQxNTYgMTQuMzk5OUg0Ljk2MTU2QzUuMzE1MDIgMTQuMzk5OSA1LjYwMTU2IDE0LjExMzQgNS42MDE1NiAxMy43NTk5VjExLjAzOTlDNS42MDE1NiAxMC42ODY0IDUuMzE1MDIgMTAuMzk5OSA0Ljk2MTU2IDEwLjM5OTlaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik0xMy43NTg0IDEuNjAwMUgxMS4wMzg0QzEwLjY4NSAxLjYwMDEgMTAuMzk4NCAxLjg4NjY0IDEwLjM5ODQgMi4yNDAxVjQuOTYwMUMxMC4zOTg0IDUuMzEzNTYgMTAuNjg1IDUuNjAwMSAxMS4wMzg0IDUuNjAwMUgxMy43NTg0QzE0LjExMTkgNS42MDAxIDE0LjM5ODQgNS4zMTM1NiAxNC4zOTg0IDQuOTYwMVYyLjI0MDFDMTQuMzk4NCAxLjg4NjY0IDE0LjExMTkgMS42MDAxIDEzLjc1ODQgMS42MDAxWiIgZmlsbD0iI2ZmZiIvPgo8cGF0aCBkPSJNNCAxMkwxMiA0TDQgMTJaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik00IDEyTDEyIDQiIHN0cm9rZT0iI2ZmZiIgc3Ryb2tlLXdpZHRoPSIxLjUiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K&logoColor=ffffff)](https://zread.ai/cnoom/cui_bind)

Unity UI 自动绑定系统 - 一款高效的 UI 开发工具，通过自动生成代码来简化 UI 组件的绑定与管理。

## 功能特性

- ✅ **三种绑定模式** - 支持手动拖拽、后缀自动、混合绑定三种模式
- ✅ **自动生成 UI 绑定代码** - 避免手动编写重复的绑定代码
- ✅ **命名约定自动绑定** - 通过后缀命名规则一键自动绑定所有子对象
- ✅ **双文件生成模式** - 使用 partial class 分离自动生成和手动编写的代码
- ✅ **智能增量更新** - 重新生成自动文件时完全不影响手动文件
- ✅ **嵌套生成支持** - 支持递归生成子对象 UI 的代码，自动避免重复绑定
- ✅ **编辑器时自动赋值** - 生成代码时自动将组件赋值到字段，无需运行时查找
- ✅ **灵活的配置系统** - 通过 ScriptableObject 配置生成参数和命名规则
- ✅ **支持父类和接口** - 生成的类可以继承指定基类并实现接口
- ✅ **组件类型支持** - 支持所有 Unity 内置组件和用户自定义组件
- ✅ **自动命名空间导入** - 根据绑定组件类型自动添加 using 语句
- ✅ **友好的编辑器工具** - 提供可视化的绑定界面和批量生成功能

## 安装

### Unity 包安装（推荐）

1. **通过 Packages 目录安装**：
   - 将整个 `CUiAutoBind` 文件夹复制到 Unity 项目的 `Packages` 目录下
   - Unity 会自动识别并加载该包

2. **通过 manifest.json 安装**：
   - 打开 Unity 项目的 `Packages/manifest.json` 文件
   - 在 `dependencies` 中添加：
     ```json
     "com.cframework.cuibind": "file:../Packages/CUiAutoBind"
     ```
   - 根据实际路径调整文件路径

3. **通过 Package Manager 安装**：
   - 打开 Unity 的 Package Manager 窗口
   - 点击左上角的 "+" 按钮
   - 选择 "Add package from disk"
   - 浏览并选择 `CUiAutoBind` 文件夹

### 传统安装方式

将 `CUiAutoBind` 文件夹复制到 Unity 项目的 `Assets` 目录下即可（不推荐，建议使用 Unity 包方式）。

## 快速开始

### 1. 创建配置

首次使用时，系统会自动创建默认配置文件，或者通过以下菜单手动创建：

- 打开菜单 `Tools/CUIBind/打开窗口`
- 在窗口中点击"创建默认配置"按钮

默认配置文件位置：`Assets/CUIBind/Resources/AutoBindConfig.asset`

### 2. 标记 UI 组件

在需要绑定的 GameObject 上添加 `AutoBind` 组件：

1. 选择 UI GameObject
2. 在 Inspector 中点击 `Add Component`
3. 搜索并添加 `AutoBind`

### 3. 配置绑定

在 AutoBind 组件中，首先选择**绑定模式**，然后根据模式进行配置：

#### 三种绑定模式

**1. 手动拖拽绑定**
- 完全手动添加绑定，适合精确控制的场景
- 可以逐个添加组件或从当前 GameObject 添加所有组件
- 适合组件数量较少或需要精确控制的 UI

**2. 后缀自动绑定**
- 根据命名后缀自动扫描并绑定，适合大量组件的场景
- 一键递归扫描所有子对象，根据后缀规则自动绑定
- 适合组件数量较多且有统一命名规范的 UI

**3. 混合绑定**
- 同时支持手动和后缀自动绑定，适合复杂场景
- 可以先用后缀自动绑定快速绑定大部分组件
- 再手动添加特殊组件或调整部分绑定

#### 手动拖拽绑定操作

- **添加新绑定**：点击"添加新绑定"按钮，然后手动选择组件和填写字段名
- **自动添加组件**：点击"从当前 GameObject 添加组件"按钮，系统会自动扫描并添加所有可用组件

#### 后缀自动绑定操作

- **按命名约定自动绑定**：点击"按命名约定自动绑定"按钮，系统会根据配置的命名规则自动扫描并绑定所有子对象

#### 混合绑定操作

- 支持上述所有操作方式
- 可以灵活组合使用

### 🆕 命名约定自动绑定（推荐）

命名约定自动绑定是最快捷的绑定方式，通过后缀命名规则自动识别并绑定组件。

#### 工作原理

1. 在 `AutoBindConfig` 中配置后缀规则（如 `_btn` → Button, `_txt` → Text）
2. 给 GameObject 命名时使用对应后缀（如 `Start_btn`, `Title_txt`, `Bg_img`）
3. 点击"按命名约定自动绑定"按钮
4. 系统自动扫描所有子对象，匹配命名规则并添加绑定
5. 自动将后缀转换为驼峰命名字段名（如 `Start_btn` → `start`）

#### 默认后缀规则

系统已预置以下常用后缀规则（注意后缀以 `_` 开头）：

| 后缀 | 组件类型 | 命名空间 |
|------|---------|---------|
| _btn | Button | UnityEngine.UI |
| _txt | Text | UnityEngine.UI |
| _img | Image | UnityEngine.UI |
| _tgl | Toggle | UnityEngine.UI |
| _slr | Slider | UnityEngine.UI |
| _inp | InputField | UnityEngine.UI |
| _scr | ScrollRect | UnityEngine.UI |
| _grid | GridLayoutGroup | UnityEngine.UI |

#### 使用示例

```
场景结构：
MainMenu (AutoBind组件)
├── Start_btn (Button组件)     → 绑定为: start
├── Settings_btn (Button组件)  → 绑定为: settings
├── Title_txt (Text组件)       → 绑定为: title
├── Volume_txt (Text组件)      → 绑定为: volume
├── Bg_img (Image组件)         → 绑定为: bg
└── SettingsPanel (AutoBind组件)  ← 有自己的AutoBind，不会被父对象绑定
    ├── Close_btn (Button组件)  → SettingsPanel自己绑定: close
    └── Volume_slr (Slider组件) → SettingsPanel自己绑定: volume
```

**步骤：**
1. 在根对象 `MainMenu` 上添加 AutoBind 组件
2. 点击"按命名约定自动绑定"按钮
3. 系统自动扫描并绑定所有子对象（除了有自己AutoBind组件的SettingsPanel）
4. 点击"生成绑定代码"

#### 配置自定义后缀规则

在 `AutoBindConfig` 中可以添加或修改后缀规则：

1. 打开 `Tools/CUIBind/打开窗口`
2. 在配置中找到 `Suffix Configs` 数组
3. 添加新规则或修改现有规则：
   - **Suffix**: 后缀名（如 `_btn`）
   - **Component Type**: 组件类型（如 `Button`）
   - **Namespace**: 命名空间（如 `UnityEngine.UI`）

#### 嵌套绑定规则

- ✅ 父对象会自动跳过有自己AutoBind组件的子对象（避免重复绑定）
- ✅ 支持配置排除前缀（如 "_Background", "TMP_"）
- ✅ 递归扫描所有子对象，深度不限
- ✅ 绑定时会检查组件是否存在，未找到会给出警告

#### 优势

- ⚡ **超快速**：一键绑定所有子对象，无需逐个手动添加
- 🎯 **零配置**：使用默认规则即可，开箱即用
- 🛡️ **安全**：自动跳过已绑定的组件，避免重复
- 🔧 **灵活**：支持自定义后缀规则和排除前缀
- 📦 **智能**：自动将后缀转换为驼峰命名，符合代码规范

### 4. 生成代码

在 AutoBind 组件 Inspector 中点击"生成绑定代码"按钮，或者在主窗口中批量生成。

生成的代码将保存在配置的基础路径下：

**双文件模式（Use Partial Class = true）**：
- 自动文件：`basePath/GameObjectName/GameObjectName.Auto.cs`
- 手动文件：`basePath/GameObjectName/GameObjectName.cs`

**单文件模式（Use Partial Class = false）**：
- 单一文件：`basePath/GameObjectName/GameObjectName.cs`

## 配置说明

在 `AutoBindConfig` 中可以配置以下参数：

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Namespace | 生成的代码命名空间 | UI |
| Base Path | 代码生成的基础路径 | Scripts/UI/Auto/ |
| Base Class | 生成类的基类 | MonoBehaviour |
| Interfaces | 生成类实现的接口数组 | 空 |
| Use Partial Class | 是否使用 partial class 生成双文件 | true |
| Add Field Comments | 是否在字段上添加注释 | true |
| Additional Namespaces | 额外的命名空间引用 | 空 |
| Suffix Configs | 🆕 命名约定规则配置（后缀 → 类型映射） | 8个默认规则 |
| Check Component Exists | 🆕 自动绑定时是否检查组件存在性 | true |

### 🆕 命名约定配置说明

Suffix Configs 是一个数组，每个元素定义一个命名规则：

```csharp
{
    "suffix": "_btn",              // 后缀名（不区分大小写）
    "componentType": "Button",    // 组件类型名
    "namespaceName": "UnityEngine.UI"  // 命名空间
}
```

**示例配置：**
- `_btn` → `Button` (UnityEngine.UI)
- `_txt` → `Text` (UnityEngine.UI)
- `_img` → `Image` (UnityEngine.UI)
- `_custom` → `MyCustomComponent` (YourNamespace) - 支持自定义组件

### 父类和接口配置

配置基类和接口后，生成的类将自动包含继承声明：

```
public partial class MainMenuUI : MonoBehaviour, IUIPanel
{
    // 自动生成字段...
}
```

## 使用教程


### 基础用法

1. 在 UI Panel 上添加 AutoBind 组件
2. 点击"从当前 GameObject 添加组件"自动扫描组件
3. 根据需要调整字段名或移除不需要的绑定
4. 点击"生成绑定代码"
5. **组件会自动赋值到生成的字段**（在编辑器中完成，无需运行时查找）
6. 在生成的代码类中添加业务逻辑

### 重要说明

**编辑器时自动赋值**
- 生成代码后，所有绑定的组件会自动赋值到对应的字段
- 字段使用 `[SerializeField] private` 修饰，既可以在 Inspector 中查看，又不会暴露给其他脚本
- 子对象的引用也会在编辑器中自动赋值
- 无需在 `Awake()` 或 `Start()` 中手动调用 `GetComponent()`，提升运行时性能

### 嵌套生成（高级功能）

CUiAutoBind 支持递归生成子对象的 UI 代码，适合复杂的 UI 层级结构。

#### 使用嵌套生成

1. **根对象配置**：
   - 在主 UI Panel 上添加 AutoBind 组件

2. **子对象配置**：
   - 在子 Panel 上也添加 AutoBind 组件
   - 为每个子对象单独配置绑定
   - 可选：设置自定义类名

3. **生成代码**：
   - 在任意 AutoBind 组件上点击"生成绑定代码"
   - 系统会自动递归生成所有子对象的代码
   - 子对象代码会生成在对应的子目录中

4. **引用子对象**：
   - 如需在父对象中引用子对象，在父对象的 AutoBind 绑定列表中添加子对象的 AutoBind 组件
   - 系统会在父对象代码中生成对应的子对象类型字段
   - 绑定时会自动将子对象的脚本组件赋值到该字段

#### 示例场景

```
场景结构：
MainMenu (AutoBind, generateChildren=true)
├── StartButton (Button)
├── SettingsPanel (AutoBind)
│   ├── CloseButton (Button)
│   └── VolumeSlider (Slider)
└── HelpPanel (AutoBind)
    └── HelpText (Text)

生成的文件结构：
MainMenu/
├── MainMenu.Auto.cs           (根对象)
├── MainMenu.cs
├── SettingsPanel/
│   ├── SettingsPanel.Auto.cs  (子对象)
│   └── SettingsPanel.cs
└── HelpPanel/
    ├── HelpPanel.Auto.cs        (子对象)
    └── HelpPanel.cs
```

#### 子对象绑定

在根对象中，可以通过组件选择器选择子对象的 AutoBind 组件：

```
MainMenu 的 AutoBind 配置：
- component: StartButton → fieldName: startButton
- component: SettingsPanel 的 AutoBind → fieldName: settingsPanel
- component: HelpPanel 的 AutoBind → fieldName: helpPanel
```

生成的代码会自动包含子对象的引用类型，并在编辑器中自动赋值，无需运行时查找。

### 手动编辑代码

#### 双文件模式（推荐）

当 `Use Partial Class` 设置为 `true` 时，会生成两个文件：

**1. 自动文件（GameObjectName.Auto.cs）** - 不要手动编辑
```csharp
// 自动生成的文件，请勿手动修改
// 重新生成时会完全覆盖此文件

using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Text titleText;
    }
}
```

**2. 手动文件（GameObjectName.cs）** - 可以自由编辑
```csharp
// 手动编写的文件，可以自由修改
// 重新生成自动文件时不会影响此文件

using UnityEngine;

namespace UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
        }

        private void OnStartClick()
        {
            Debug.Log("Start button clicked!");
        }
    }
}
```

**优势**：
- ✅ 重新生成时完全不会覆盖手动代码
- ✅ 文件分离，结构清晰
- ✅ 每个文件职责明确
- ✅ 无需使用 region 标记

#### 单文件模式（旧版）

当 `Use Partial Class` 设置为 `false` 时，会生成单个文件，包含两个区域：

- `#region AutoBind Generated` - 自动生成的字段区域，重新生成时会被覆盖
- `#region Manual Code` - 手动代码区域，重新生成时会被保留

```csharp
using UnityEngine;
using UnityEngine.UI;

// Auto-generated by CUiAutoBind
// DO NOT EDIT MANUALLY IN THE AutoBind Generated REGION

namespace UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        #region AutoBind Generated
        public Button startButton;
        public Text titleText;
        #endregion AutoBind Generated

        #region Manual Code
        private void Start()
        {
            startButton.onClick.AddListener(OnStartClick);
        }
        #endregion Manual Code
    }
}
```

### 批量生成

打开主窗口（`Tools/CUIBind/打开窗口`），可以：

- 查看场景中所有的 AutoBind 组件
- 单独为某个 GameObject 生成代码
- 点击"全部生成"按钮批量生成所有代码

## 高级特性

### 嵌套生成

CUiAutoBind 支持递归生成子对象的 UI 代码，适合复杂的 UI 层级结构。

#### 特性

- ✅ **递归扫描** - 自动扫描所有子对象的 AutoBind 组件
- ✅ **深度控制** - 可配置最大递归深度
- ✅ **对象排除** - 支持排除特定前缀的对象
- ✅ **自定义类名** - 每个对象可指定自定义类名
- ✅ **同名处理** - 多种策略处理同名对象

#### AutoBind 组件配置

每个 AutoBind 组件可以配置：

- **自定义类名** - 覆盖自动生成的类名（留空则使用 GameObject 名称）
- **生成子对象** - 是否递归生成子对象
- **最大深度** - 限制递归深度（0 = 无限制）
- **排除前缀** - 排除特定前缀的对象（如 "_Background", "TMP_"）

### 增量更新

#### 双文件模式（推荐）

当 UI 结构发生变化时：

1. 修改 AutoBind 组件中的绑定配置
2. 点击"生成绑定代码"
3. 系统会完全覆盖 `GameObjectName.Auto.cs` 文件
4. `GameObjectName.cs` 文件不受任何影响

#### 单文件模式

当 UI 结构发生变化时：

1. 修改 AutoBind 组件中的绑定配置
2. 点击"生成绑定代码"
3. 系统会自动更新 `AutoBind Generated` 区域
4. `Manual Code` 区域的代码会被完整保留

### 自定义组件支持

系统支持任何继承自 Component 的类型，包括：

- Unity 内置组件（Button, Text, Image, Toggle, Slider 等）
- TextMeshPro 组件（TMP_Text, TMP_Dropdown 等）
- 用户自定义组件

### 类型过滤

在 AutoBindData 属性面板中：

- 使用组件选择器精确选择组件
- 系统会自动为选中组件生成合适的字段名
- 可以通过"生成"复选框控制是否生成该字段

## 项目结构

```
CUiAutoBind/
├── Runtime/                    # 运行时代码
│   ├── AutoBind.cs            # 核心标记组件
│   ├── AutoBindData.cs        # 绑定数据结构
│   ├── BindConfig.cs          # 配置类
│   └── StringUtil.cs          # 字符串工具类
├── Editor/                     # 编辑器代码
│   ├── AutoBindEditor.cs      # 组件检视面板
│   ├── AutoBindWindow.cs      # 主编辑器窗口
│   ├── AutoBindDataDrawer.cs  # 数据绘制器
│   ├── CodeBinder.cs          # 代码绑定器
│   ├── CodeGenerator.cs       # 代码生成器
│   ├── ConfigManager.cs       # 配置管理器
│   ├── AutoBindUtility.cs     # 编辑器工具类
│   └── SuffixConfigDrawer.cs  # 后缀配置绘制器
└── README.md                   # 本文档
```

## 注意事项

### 双文件模式（推荐）

1. **不要编辑自动文件** - `GameObjectName.Auto.cs` 会在重新生成时被完全覆盖
2. **手动文件可自由编辑** - `GameObjectName.cs` 不会被重新生成，可以随意修改
3. **保持 partial class** - 手动文件中的类声明必须使用 `partial` 关键字
4. **无需 region** - 双文件模式下不需要使用 region 标记，整个文件都是手动代码区域
5. **字段命名** - 确保字段名符合 C# 命名规范

### 单文件模式

1. **不要编辑自动生成区域** - `AutoBind Generated` 区域的内容会在重新生成时被完全覆盖
2. **保留手动代码** - 所有自定义逻辑应写在 `Manual Code` 区域
3. **字段命名** - 确保字段名符合 C# 命名规范
4. **配置文件** - 建议在项目根目录创建配置文件并备份

## 常见问题

### Q: 生成的代码找不到引用？

A: 确保在自定义组件的脚本中使用了生成的类名，并且类名与 GameObject 名称一致。

### Q: 双文件模式下手动文件会丢失吗？

A: 不会。在双文件模式下，手动文件（`GameObjectName.cs`）只在第一次生成时创建，之后重新生成不会影响它。

### Q: 单文件模式下手动添加的代码消失了？

A: 确保手动添加的代码放在 `#region Manual Code` 和 `#endregion Manual Code` 之间。

### Q: 如何修改生成的字段类型？

A: 在 AutoBind 组件中重新选择组件，或者修改组件本身的类型，然后重新生成代码。

### Q: 支持嵌套的 UI 结构吗？

A: 支持。使用嵌套生成功能，系统会递归扫描并生成所有子对象的代码。每个子对象需要添加 AutoBind 组件。

### Q: 如何在根对象中引用子对象的 UI？

A: 在根对象的 AutoBind 组件中，通过组件选择器选择子对象的 AutoBind 组件。系统会自动生成引用字段。

### Q: 嵌套生成的文件在哪里？

A: 子对象的代码会生成在根对象的子目录下。例如：
- 根对象：`Scripts/UI/Auto/MainMenu/MainMenu.cs`
- 子对象：`Scripts/UI/Auto/MainMenu/SettingsPanel/SettingsPanel.cs`

### Q: 如何排除某些对象不被生成？

A: 在 AutoBind 组件中配置"排除前缀"，用逗号分隔多个前缀。例如：`_Background, TMP_, Temp`。

### Q: 如何在生成的代码中使用其他命名空间？

A: 在配置文件的"额外命名空间"中添加需要引用的命名空间，或者手动在手动文件中添加 using 语句。

### Q: 生成的代码报错，提示找不到组件类型？

A: 系统会自动根据绑定的组件类型添加对应的 using 语句。如果仍有问题，可以在配置中添加额外的命名空间。

### Q: 子对象初始化失败怎么办？

A: 系统会自动生成 `InitializeChildPanels()` 方法在 `Awake()` 中初始化。确保子对象已正确生成代码并且类名匹配。

## 版本历史

### v1.0.0
- 初始版本发布
- 支持基础 UI 绑定功能
- 实现代码自动生成和增量更新
- 添加父类和接口支持
- 完整的编辑器工具
- 实现双文件生成模式（partial class）
- 支持自动命名空间导入
- 组件类型下拉选择功能

### v1.1.0
- 实现嵌套生成功能
- 支持递归扫描子对象
- 添加子对象自动初始化代码
- 支持同名对象处理（多种命名策略）
- 添加对象排除功能
- 支持自定义 UI 类名
- 完善编辑器界面配置
- 添加后缀命名约定自动绑定功能
- 实现组件编辑器时自动赋值
- 添加绑定验证功能

## 许可证

MIT License

## 联系方式

如有问题或建议，欢迎提交 Issue。
