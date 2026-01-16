# 快速开始指南

## 🚀 5分钟上手命名约定自动绑定

### 第1步：创建UI并命名

在Unity中创建UI对象，使用以下命名后缀：

| 后缀 | 组件类型 |
|------|---------|
| `_btn` | Button |
| `_txt` | Text |
| `_img` | Image |
| `_tgl` | Toggle |
| `_slr` | Slider |
| `_inp` | InputField |

**示例：**
```
MainMenu
├── Start_btn
├── Settings_btn
├── Title_txt
└── Bg_img
```

### 第2步：添加AutoBind组件

1. 选择根对象 `MainMenu`
2. 点击 `Add Component`
3. 搜索并添加 `AutoBind`

### 第3步：一键自动绑定

在AutoBind组件的Inspector中：
1. 找到"命名约定自动绑定"部分
2. 点击 **"按命名约定自动绑定"** 按钮
3. 查看绑定结果

### 第4步：生成代码

点击 **"生成绑定代码"** 按钮，系统会自动：
1. 生成绑定代码
2. 添加脚本组件到GameObject
3. 绑定所有UI组件到字段

### 第5步：使用生成的字段

在生成的代码中，直接使用字段访问UI组件：

```csharp
public partial class MainMenu : MonoBehaviour
{
    private void Start()
    {
        start.onClick.AddListener(OnStartClick);
        title.text = "Welcome!";
    }

    private void OnStartClick()
    {
        Debug.Log("Start button clicked!");
    }
}
```

---

## 📝 完整示例

### 场景结构
```
MainMenu (AutoBind组件)
├── Start_btn (Button)
├── Settings_btn (Button)
├── Title_txt (Text)
└── Bg_img (Image)
```

### 操作步骤

1. **在MainMenu上添加AutoBind组件**

2. **点击"按命名约定自动绑定"**
   ```
   结果：
   ✓ 新增绑定: 4
     - Start_btn → start
     - Settings_btn → settings
     - Title_txt → title
     - Bg_img → bg
   ```

3. **点击"生成绑定代码"**
   ```
   生成的代码：
   Scripts/UI/Auto/MainMenu/
   ├── MainMenu.Auto.cs  (自动生成的字段)
   └── MainMenu.cs       (手动编写业务逻辑)
   ```

4. **使用字段**
   ```csharp
   // MainMenu.cs
   public partial class MainMenu : MonoBehaviour
   {
       private void Start()
       {
           start.onClick.AddListener(OnStartClick);
           settings.onClick.AddListener(OnSettingsClick);
           title.text = "Main Menu";
       }

       private void OnStartClick()
       {
           Debug.Log("Start clicked!");
       }

       private void OnSettingsClick()
       {
           Debug.Log("Settings clicked!");
       }
   }
   ```

---

## 🔧 批量操作

### 批量绑定多个UI

1. 打开菜单：`Tools/CUIBind/打开窗口`

2. 点击 **"批量按命名约定自动绑定"**
   ```
   批量自动绑定完成！

   总计:
     ✓ 新增绑定: 15
     ○ 已存在（跳过）: 0
     处理对象数: 3
   ```

3. 点击 **"全部生成"** 按钮生成所有代码

---

## ⚙️ 自定义配置

### 添加自定义后缀规则

1. 打开 `Tools/CUIBind/打开窗口`
2. 找到 `Suffix Configs` 配置
3. 添加新规则：
   - **Suffix**: 后缀名（如 `custom`）
   - **Component Type**: 组件类型（如 `MyComponent`）
   - **Namespace**: 命名空间（如 `Game.UI`）

### 配置排除前缀

在AutoBind组件中设置 `Excluded Prefixes`：
```
排除前缀: _, TMP, Temp
```

这样名称匹配这些前缀的对象会被自动跳过。

---

## 💡 提示

### 命名规范
- ✅ 使用统一的后缀命名
- ✅ 后缀使用小写字母
- ✅ 名称使用下划线分隔单词

示例：
```
✅ Start_btn
✅ PlayerName_txt
✅ InventoryIcon_img
❌ Button_Start
❌ startButton
```

### 嵌套结构
对于嵌套UI，在每个Panel上单独添加AutoBind组件：
```
MainMenu (AutoBind)
├── Start_btn
└── SettingsPanel (AutoBind)  ← 有自己的AutoBind
    ├── Close_btn
    └── Volume_slr
```

这样SettingsPanel的组件不会被MainMenu绑定，避免重复。

---

## 🎓 下一步

- 查看 [USAGE_EXAMPLES.md](USAGE_EXAMPLES.md) 了解更多详细示例
- 查看 [README.md](README.md) 了解完整功能说明
- 根据项目需求自定义命名规则

---

## ❓ 常见问题

**Q: 点击自动绑定没有反应？**

A: 检查以下几点：
1. 子对象名称是否匹配后缀规则
2. 子对象上是否有对应的组件
3. 配置文件中是否有命名规则

**Q: 如何支持自定义组件？**

A: 在配置中添加新的后缀规则：
```csharp
{
    "suffix": "custom",
    "componentType": "MyCustomComponent",
    "namespaceName": "Game.UI"
}
```

**Q: 生成的字段名可以修改吗？**

A: 可以！在AutoBind组件的绑定列表中，可以手动修改字段名。

---

## 📞 支持

如有问题或建议，请提交Issue。
