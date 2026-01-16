using UnityEngine;

namespace CUiAutoBind
{
    /// <summary>
    /// AutoBind 数据结构，用于存储绑定配置
    /// </summary>
    [System.Serializable]
    public class AutoBindData
    {
        /// <summary>
        /// 绑定的组件类型
        /// </summary>
        public Component component;

        /// <summary>
        /// 生成的字段名称
        /// </summary>
        public string fieldName;

        /// <summary>
        /// 是否生成字段
        /// </summary>
        public bool generateField = true;

        /// <summary>
        /// 组件类型名称（用于序列化）
        /// </summary>
        [System.NonSerialized]
        private System.Type componentType;

        /// <summary>
        /// 获取组件类型
        /// </summary>
        public System.Type ComponentType
        {
            get
            {
                if (componentType == null && component != null)
                {
                    componentType = component.GetType();
                }
                return componentType;
            }
            set
            {
                componentType = value;
            }
        }

        /// <summary>
        /// 判断是否绑定的是 AutoBind 组件
        /// </summary>
        public bool IsAutoBindReference()
        {
            return component != null && component is AutoBind;
        }
    }
}
