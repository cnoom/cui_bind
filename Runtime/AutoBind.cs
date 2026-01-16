using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CUiAutoBind
{
    /// <summary>
    /// AutoBind 组件，用于标记和管理 UI 组件绑定
    /// </summary>
    [ExecuteAlways]
    public class AutoBind : MonoBehaviour
    {
        /// <summary>
        /// 绑定数据数组
        /// </summary>
        public List<AutoBindData> bindings = new List<AutoBindData>();

        /// <summary>
        /// 是否启用自动绑定
        /// </summary>
        public bool autoBindEnabled = true;

        /// <summary>
        /// 自定义 UI 类名（留空则使用 GameObject 名称）
        /// </summary>
        public string customClassName = "";
        
        /// <summary>
        /// 要排除的 GameObject 名称前缀（多个用逗号分隔）
        /// </summary>
        public string excludedPrefixes = "";

        /// <summary>
        /// 是否显示UI绑定列表
        /// </summary>
        public bool showBindingList = true;

        /// <summary>
        /// 添加绑定数据
        /// </summary>
        public void AddBinding(Component component, string fieldName)
        {
            var data = new AutoBindData
            {
                component = component,
                fieldName = fieldName,
                generateField = true
            };

            // 只有当组件不为 null 时才检查重复
            if (component != null)
            {
                if (bindings.Exists(b => b.component == component))
                    return;
            }

            bindings.Add(data);
        }

        /// <summary>
        /// 移除绑定数据
        /// </summary>
        public void RemoveBinding(AutoBindData data)
        {
            if (data != null && bindings.Contains(data))
            {
                bindings.Remove(data);
            }
        }

        /// <summary>
        /// 清空所有绑定
        /// </summary>
        public void ClearBindings()
        {
            bindings.Clear();
        }

        /// <summary>
        /// 获取所有有效的绑定数据
        /// </summary>
        public List<AutoBindData> GetValidBindings()
        {
            return bindings.FindAll(b => b != null && b.component != null && !string.IsNullOrEmpty(b.fieldName));
        }

        /// <summary>
        /// 获取所有子对象的 AutoBind 组件
        /// </summary>
        public List<AutoBind> GetChildAutoBinds()
        {
            return GetChildAutoBindsRecursive();
        }

        /// <summary>
        /// 递归获取子对象的 AutoBind 组件
        /// </summary>
        private List<AutoBind> GetChildAutoBindsRecursive()
        {
            List<AutoBind> result = new List<AutoBind>();

            // 获取排除前缀
            string[] prefixes =  GetExcludedPrefixes();

            // 遍历所有子对象
            foreach (Transform child in transform)
            {
                // 检查排除前缀
                if (IsExcluded(child.name, prefixes))
                    continue;

                // 获取子对象的 AutoBind 组件
                AutoBind childAutoBind = child.GetComponent<AutoBind>();
                if (childAutoBind != null)
                {
                    result.Add(childAutoBind);

                    // 递归获取更深的子对象
                    result.AddRange(childAutoBind.GetChildAutoBindsRecursive());
                }
            }

            return result;
        }

        /// <summary>
        /// 获取排除前缀数组
        /// </summary>
        private string[] GetExcludedPrefixes()
        {
            if (string.IsNullOrEmpty(excludedPrefixes))
                return new string[0];

            return excludedPrefixes.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();
        }

        /// <summary>
        /// 检查是否被排除
        /// </summary>
        private bool IsExcluded(string name, string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                return false;

            return prefixes.Any(prefix => name.StartsWith(prefix));
        }

        /// <summary>
        /// 获取 UI 类名（优先使用自定义名称）
        /// </summary>
        public string GetUIClassName()
        {
            return string.IsNullOrEmpty(customClassName) ? StringUtil.GetSafeClassName(gameObject.name) : customClassName;
        }
    }
}
