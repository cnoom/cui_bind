using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CUiAutoBind
{
    /// <summary>
    /// AutoBindData 的自定义属性绘制器，提供组件类型下拉选择
    /// </summary>
    [CustomPropertyDrawer(typeof(AutoBindData))]
    public class AutoBindDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty componentProp = property.FindPropertyRelative("component");
            SerializedProperty fieldNameProp = property.FindPropertyRelative("fieldName");
            SerializedProperty generateFieldProp = property.FindPropertyRelative("generateField");

            // 组件字段
            Rect componentRect = new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight);
            Rect selectButtonRect = new Rect(position.x + position.width - 55, position.y, 55, EditorGUIUtility.singleLineHeight);

            // 显示组件对象字段
            EditorGUI.PropertyField(componentRect, componentProp, GUIContent.none);

            // 选择按钮
            if (GUI.Button(selectButtonRect, "选择"))
            {
                ShowComponentSelectionMenu(componentProp, fieldNameProp);
            }

            // 如果已选择组件，自动填充字段名（如果是空的）
            if (componentProp.objectReferenceValue != null && string.IsNullOrEmpty(fieldNameProp.stringValue))
            {
                Component component = (Component)componentProp.objectReferenceValue;
                string autoName = GenerateAutoFieldName(component);
                fieldNameProp.stringValue = autoName;
            }

            // 字段名
            Rect fieldNameRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width - 80, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(fieldNameRect, fieldNameProp, new GUIContent("字段名"));

            // 生成字段复选框
            Rect generateFieldRect = new Rect(position.x + position.width - 80, position.y + EditorGUIUtility.singleLineHeight + 2, 80, EditorGUIUtility.singleLineHeight);
            generateFieldProp.boolValue = EditorGUI.ToggleLeft(generateFieldRect, "生成", generateFieldProp.boolValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        /// <summary>
        /// 显示组件选择菜单
        /// </summary>
        private void ShowComponentSelectionMenu(SerializedProperty componentProp, SerializedProperty fieldNameProp)
        {
            // 获取当前绑定的组件
            Component currentComponent = componentProp.objectReferenceValue as Component;
            GameObject targetObject = currentComponent != null ? currentComponent.gameObject : Selection.activeGameObject;

            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选择一个 GameObject", "确定");
                return;
            }

            // 获取组件列表：当前对象的组件
            var componentList = new List<Component>();

            // 获取当前对象上的所有组件
            Component[] selfComponents = targetObject.GetComponents<Component>();
            componentList.AddRange(selfComponents);

            if (componentList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "该 GameObject 上没有可绑定的组件", "确定");
                return;
            }

            // 创建菜单
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("无"), currentComponent == null, () =>
            {
                componentProp.objectReferenceValue = null;
                componentProp.serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            // 按组件类型分组
            var groupedComponents = componentList
                .Where(c => c != null)
                .OrderBy(c => c.GetType().Name)
                .ThenBy(c => c.gameObject == targetObject ? 0 : 1)  // 当前对象的组件在前
                .ThenBy(c => c.name)
                .ToList();

            foreach (var component in groupedComponents)
            {
                string componentType = component.GetType().Name;
                string componentName = component.name;

                // 检查是否是子对象的 AutoBind 组件
                bool isChildAutoBind = (component is AutoBind) && (component.gameObject != targetObject);

                bool isSelected = (currentComponent == component);

                // 为子对象的 AutoBind 添加标记
                string prefix = isChildAutoBind ? "📦 " : "";
                string gameObjectPath = isChildAutoBind ? $" [{GetRelativePath(targetObject, component.gameObject)}]" : "";
                string menuPath = $"{prefix}{componentType}{gameObjectPath} ({componentName})";

                menu.AddItem(new GUIContent(menuPath), isSelected, () =>
                {
                    componentProp.objectReferenceValue = component;
                    componentProp.serializedObject.ApplyModifiedProperties();

                    // 自动生成字段名
                    if (string.IsNullOrEmpty(fieldNameProp.stringValue))
                    {
                        fieldNameProp.stringValue = GenerateAutoFieldName(component);
                        fieldNameProp.serializedObject.ApplyModifiedProperties();
                    }
                });
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        private string GetRelativePath(GameObject parent, GameObject child)
        {
            // 简单方法：通过遍历父级获取相对路径
            List<string> pathParts = new List<string>();
            Transform current = child.transform;

            while (current != null && current != parent.transform)
            {
                pathParts.Insert(0, current.name);
                current = current.parent;
            }

            // 检查是否找到了父对象
            if (current == null)
            {
                return child.name; // 没找到父对象，返回名称
            }

            return string.Join("/", pathParts);
        }

        /// <summary>
        /// 生成自动字段名
        /// </summary>
        private string GenerateAutoFieldName(Component component)
        {
            if (component == null)
                return "";

            // 获取组件类型名称
            string typeName = component.GetType().Name;

            // 转换为驼峰命名
            typeName = StringUtil.ToCamelCase(typeName);

            // 如果是 TextMeshPro 或其他长名称，使用缩写
            if (typeName.Length > 15)
            {
                typeName = typeName.Replace("TextMeshPro", "TMP");
                typeName = typeName.Replace("Component", "");
            }

            return typeName;
        }
    }
}
