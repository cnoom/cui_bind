using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CUiAutoBind
{
    /// <summary>
    /// 组件类型选择器 - 封装组件类型和命名空间信息
    /// </summary>
    [System.Serializable]
    public class ComponentTypeSelector
    {
        /// <summary>
        /// 组件类型全名（含命名空间）
        /// </summary>
        [SerializeField]
        private string fullTypeName = "UnityEngine.UI.Button";

        /// <summary>
        /// 获取或设置组件类型全名
        /// </summary>
        public string FullTypeName
        {
            get => fullTypeName;
            set => fullTypeName = value;
        }

        /// <summary>
        /// 获取组件类型名称（不含命名空间）
        /// </summary>
        public string ComponentType
        {
            get
            {
                if (string.IsNullOrEmpty(fullTypeName))
                    return "";
                int lastDot = fullTypeName.LastIndexOf('.');
                return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
            }
            set
            {
                if (string.IsNullOrEmpty(fullTypeName))
                {
                    fullTypeName = $"UnityEngine.UI.{value}";
                }
                else
                {
                    int lastDot = fullTypeName.LastIndexOf('.');
                    fullTypeName = lastDot >= 0 ? $"{fullTypeName.Substring(0, lastDot)}.{value}" : value;
                }
            }
        }

        /// <summary>
        /// 获取命名空间
        /// </summary>
        public string NamespaceName
        {
            get
            {
                if (string.IsNullOrEmpty(fullTypeName))
                    return "";
                int lastDot = fullTypeName.LastIndexOf('.');
                return lastDot >= 0 ? fullTypeName.Substring(0, lastDot) : "";
            }
            set
            {
                if (string.IsNullOrEmpty(fullTypeName))
                {
                    fullTypeName = $"{value}.Button";
                }
                else
                {
                    int lastDot = fullTypeName.LastIndexOf('.');
                    fullTypeName = $"{value}.{fullTypeName.Substring(lastDot + 1)}";
                }
            }
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(fullTypeName);

        /// <summary>
        /// 构造函数
        /// </summary>
        public ComponentTypeSelector() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ComponentTypeSelector(string componentType, string namespaceName)
        {
            fullTypeName = string.IsNullOrEmpty(namespaceName) ? componentType : $"{namespaceName}.{componentType}";
        }

        /// <summary>
        /// 从字符串构造
        /// </summary>
        public static ComponentTypeSelector FromString(string fullTypeName)
        {
            return new ComponentTypeSelector { fullTypeName = fullTypeName };
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            return fullTypeName ?? "";
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        public static implicit operator string(ComponentTypeSelector selector) => selector?.fullTypeName ?? "";
    }

    /// <summary>
    /// 后缀配置项
    /// </summary>
    [System.Serializable]
    public class SuffixConfig
    {
        /// <summary>
        /// 后缀名称（如 "btn"）
        /// </summary>
        public string suffix = "btn";

        /// <summary>
        /// 对应的组件类型选择器
        /// </summary>
        public ComponentTypeSelector componentType = new ComponentTypeSelector("Button", "UnityEngine.UI");

        /// <summary>
        /// 兼容旧版本：获取组件类型名称
        /// </summary>
        [System.Obsolete("请使用 componentType.ComponentType 代替")]
        public string componentTypeLegacy
        {
            get => componentType.ComponentType;
            set => componentType.ComponentType = value;
        }

        /// <summary>
        /// 兼容旧版本：获取命名空间
        /// </summary>
        [System.Obsolete("请使用 componentType.NamespaceName 代替")]
        public string namespaceNameLegacy
        {
            get => componentType.NamespaceName;
            set => componentType.NamespaceName = value;
        }
    }

    /// <summary>
    /// AutoBind 配置类，用于管理代码生成参数
    /// </summary>
    [CreateAssetMenu(fileName = "AutoBindConfig", menuName = "CUiAutoBind/Config")]
    public class BindConfig : ScriptableObject
    {
        /// <summary>
        /// 命名空间名称
        /// </summary>
        public string namespaceName = "UI";

        /// <summary>
        /// 基础生成路径（相对 Assets/）
        /// </summary>
        public string basePath = "Scripts/UI/Auto/";

        /// <summary>
        /// 生成类的基类名称
        /// </summary>
        public string baseClass = "MonoBehaviour";

        /// <summary>
        /// 生成类实现的接口列表
        /// </summary>
        public string[] interfaces = new string[0];

        /// <summary>
        /// 是否使用 partial class
        /// </summary>
        public bool usePartialClass = true;

        /// <summary>
        /// 额外的命名空间引用（自动添加到生成的代码中）
        /// </summary>
        public string[] additionalNamespaces = new string[0];

        /// <summary>
        /// 后缀命名规则配置（用于自动绑定）
        /// </summary>
        public SuffixConfig[] suffixConfigs = new SuffixConfig[]
        {
            new SuffixConfig { suffix = "_btn", componentType = new ComponentTypeSelector("Button", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_txt", componentType = new ComponentTypeSelector("Text", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_img", componentType = new ComponentTypeSelector("Image", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_tgl", componentType = new ComponentTypeSelector("Toggle", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_slr", componentType = new ComponentTypeSelector("Slider", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_inp", componentType = new ComponentTypeSelector("InputField", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_scr", componentType = new ComponentTypeSelector("ScrollRect", "UnityEngine.UI") },
            new SuffixConfig { suffix = "_grid", componentType = new ComponentTypeSelector("GridLayoutGroup", "UnityEngine.UI") }
        };

        /// <summary>
        /// 是否在自动绑定时检查组件存在性
        /// </summary>
        public bool checkComponentExists = true;

        /// <summary>
        /// 自动生成区域的标记字符串
        /// </summary>
        public const string AutoBindRegionStart = "#region AutoBind Generated";
        public const string AutoBindRegionEnd = "#endregion AutoBind Generated";
        public const string ManualRegionStart = "#region Manual Code";
        public const string ManualRegionEnd = "#endregion Manual Code";

        /// <summary>
        /// 获取完整的命名空间声明
        /// </summary>
        public string GetNamespaceDeclaration()
        {
            return string.IsNullOrEmpty(namespaceName) ? "" : $"namespace {namespaceName}";
        }

        /// <summary>
        /// 获取类的基类和接口声明
        /// </summary>
        public string GetClassInheritance()
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(baseClass))
            {
                parts.Add(baseClass);
            }

            if (interfaces != null && interfaces.Length > 0)
            {
                foreach (var iface in interfaces)
                {
                    if (!string.IsNullOrEmpty(iface))
                    {
                        parts.Add(iface);
                    }
                }
            }

            return parts.Count > 0 ? " : " + string.Join(", ", parts) : "";
        }

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(basePath);
        }

        /// <summary>
        /// 获取文件路径（公共方法）
        /// </summary>
        private string GetFilePath(string gameObjectName, string fileName)
        {
            return System.IO.Path.Combine("Assets", basePath.Trim('/'), gameObjectName, fileName);
        }

        /// <summary>
        /// 获取自动生成文件的完整路径
        /// </summary>
        public string GetAutoGeneratedFilePath(string gameObjectName)
        {
            return GetFilePath(gameObjectName, $"{gameObjectName}.Auto.cs");
        }

        /// <summary>
        /// 获取手动文件的完整路径
        /// </summary>
        public string GetManualFilePath(string gameObjectName)
        {
            return GetFilePath(gameObjectName, $"{gameObjectName}.cs");
        }

        /// <summary>
        /// 获取生成类的完整路径（兼容旧版）
        /// </summary>
        public string GetGeneratedFilePath(string gameObjectName)
        {
            return usePartialClass ? GetAutoGeneratedFilePath(gameObjectName) : GetManualFilePath(gameObjectName);
        }
    }
}
