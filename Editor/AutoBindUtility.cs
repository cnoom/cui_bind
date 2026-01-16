using UnityEngine;
using System;

namespace CUiAutoBind
{
    /// <summary>
    /// AutoBind 编辑器工具类，提供共享的辅助方法
    /// </summary>
    public static class AutoBindUtility
    {
        /// <summary>
        /// 递归按命名约定自动绑定
        /// </summary>
        public static void AutoBindByNamingConventionRecursive(Transform current, AutoBind parentAutoBind, BindConfig config, ref int addedCount, ref int skippedCount, ref int notFoundCount)
        {
            // 跳过父对象自身
            if (current == parentAutoBind.transform)
            {
                // 只遍历直接子对象
                foreach (Transform child in current)
                {
                    AutoBindByNamingConventionRecursive(child, parentAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);
                }
                return;
            }

            // 检查是否有AutoBind组件（如果有，说明这个对象由自己管理，跳过）
            if (current.GetComponent<AutoBind>() != null)
                return;

            // 检查是否匹配命名约定
            SuffixConfig matchedSuffix = null;
            foreach (var suffixConfig in config.suffixConfigs)
            {
                if (current.name.EndsWith(suffixConfig.suffix, StringComparison.OrdinalIgnoreCase))
                {
                    matchedSuffix = suffixConfig;
                    break;
                }
            }

            // 如果匹配到命名规则，尝试绑定
            if (matchedSuffix != null)
            {
                // 尝试获取组件类型
                Type componentType = GetComponentType(matchedSuffix);
                if (componentType != null)
                {
                    Component component = current.GetComponent(componentType);

                    if (component != null)
                    {
                        // 生成字段名（将后缀转换为驼峰命名）
                        string fieldName = ConvertToFieldName(current.name, matchedSuffix.suffix);

                        // 检查是否已经绑定
                        bool exists = parentAutoBind.bindings.Exists(b => b.component == component);
                        if (!exists)
                        {
                            parentAutoBind.AddBinding(component, fieldName);
                            addedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                    else
                    {
                        notFoundCount++;
                        Debug.LogWarning($"对象 '{current.name}' 匹配后缀 '{matchedSuffix.suffix}'，但未找到对应的 '{matchedSuffix.componentType.ComponentType}' 组件");
                    }
                }
            }

            // 递归处理子对象
            foreach (Transform child in current)
            {
                AutoBindByNamingConventionRecursive(child, parentAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);
            }
        }

        /// <summary>
        /// 根据后缀配置获取组件类型
        /// </summary>
        public static Type GetComponentType(SuffixConfig suffixConfig)
        {
            if (suffixConfig.componentType == null || string.IsNullOrEmpty(suffixConfig.componentType.FullTypeName))
                return null;

            // 尝试从已加载的程序集中查找类型
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // 使用完整的类型名
                    Type type = assembly.GetType(suffixConfig.componentType.FullTypeName, false, true);
                    if (type != null)
                        return type;
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// 将对象名称转换为字段名（移除后缀，首字母小写）
        /// </summary>
        public static string ConvertToFieldName(string objectName, string suffix)
        {
            // 移除后缀（不区分大小写）
            string fieldName = objectName;
            if (objectName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                fieldName = objectName.Substring(0, objectName.Length - suffix.Length);
            }

            // 首字母小写（驼峰命名）
            if (fieldName.Length > 0)
            {
                fieldName = char.ToLower(fieldName[0]) + fieldName.Substring(1);
            }

            return fieldName;
        }
    }
}
