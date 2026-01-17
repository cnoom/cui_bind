using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CUiAutoBind
{
    /// <summary>
    /// 绑定模式枚举
    /// </summary>
    public enum BindMode
    {
        /// <summary>
        /// 手动拖拽绑定 - 完全手动添加绑定
        /// </summary>
        Manual,

        /// <summary>
        /// 后缀自动绑定 - 根据命名后缀自动扫描并绑定
        /// </summary>
        AutoSuffix,

        /// <summary>
        /// 混合绑定 - 同时支持手动和后缀自动绑定
        /// </summary>
        Hybrid
    }

    /// <summary>
    /// AutoBind 组件，用于标记和管理 UI 组件绑定
    /// </summary>
    [ExecuteAlways]
    public class AutoBind : MonoBehaviour
    {
        /// <summary>
        /// 绑定模式
        /// </summary>
        public BindMode bindMode = BindMode.Manual;

        /// <summary>
        /// 绑定数据数组
        /// </summary>
        public List<AutoBindData> bindings = new List<AutoBindData>();

        /// <summary>
        /// 自定义 UI 类名（留空则使用 GameObject 名称）
        /// </summary>
        public string customClassName = "";

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

            // 遍历所有子对象
            foreach (Transform child in transform)
            {
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
        /// 获取 UI 类名（优先使用自定义名称）
        /// </summary>
        public string GetUIClassName()
        {
            return string.IsNullOrEmpty(customClassName) ? StringUtil.GetSafeClassName(gameObject.name) : customClassName;
        }

        /// <summary>
        /// 获取绑定模式描述
        /// </summary>
        public string GetBindModeDescription()
        {
            switch (bindMode)
            {
                case BindMode.Manual:
                    return "手动拖拽绑定 - 完全手动添加绑定";
                case BindMode.AutoSuffix:
                    return "后缀自动绑定 - 根据命名后缀自动扫描并绑定";
                case BindMode.Hybrid:
                    return "混合绑定 - 同时支持手动和后缀自动绑定";
                default:
                    return "未知绑定模式";
            }
        }
    }
}
