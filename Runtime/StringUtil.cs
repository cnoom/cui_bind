using System.IO;
using System.Linq;

namespace CUiAutoBind
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// 获取安全的类名
        /// </summary>
        public static string GetSafeClassName(string gameObjectName)
        {
            if (string.IsNullOrEmpty(gameObjectName))
                return "AutoBindUI";

            string className = gameObjectName;

            // 移除不合法的字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            className = new string(className.Where(c => !invalidChars.Contains(c)).ToArray());

            // 移除空格
            className = className.Replace(" ", "");

            // 首字母大写
            className = char.ToUpper(className[0]) + className.Substring(1);

            return className;
        }

        /// <summary>
        /// 转换为驼峰命名
        /// </summary>
        public static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length == 0)
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// 获取组件类型名称
        /// </summary>
        public static string GetComponentTypeName(System.Type type)
        {
            if (type == null)
                return "Component";

            // 处理 Unity 内置类型
            if (type == typeof(UnityEngine.UI.Button)) return "Button";
            if (type == typeof(UnityEngine.UI.Text)) return "Text";
            if (type == typeof(UnityEngine.UI.Image)) return "Image";
            if (type == typeof(UnityEngine.UI.Toggle)) return "Toggle";
            if (type == typeof(UnityEngine.UI.Slider)) return "Slider";
            if (type == typeof(UnityEngine.UI.ScrollRect)) return "ScrollRect";
            if (type == typeof(UnityEngine.Transform)) return "Transform";
            if (type == typeof(UnityEngine.GameObject)) return "GameObject";

            // 处理泛型类型
            if (type.IsGenericType)
            {
                return type.Name.Split('`')[0];
            }

            // 返回完整类型名
            return type.Name;
        }
    }
}
