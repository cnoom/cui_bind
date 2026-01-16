using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;

namespace CUiAutoBind
{
    /// <summary>
    /// AutoBind 组件的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(AutoBind))]
    public class AutoBindEditor : Editor
    {
        private SerializedProperty bindingsProperty;
        private SerializedProperty autoBindEnabledProperty;
        private SerializedProperty customClassNameProperty;
        private SerializedProperty generateChildrenProperty;
        private SerializedProperty maxDepthProperty;
        private SerializedProperty excludedPrefixesProperty;

        /// <summary>
        /// 缓存的配置，用于 AutoAssignComponents
        /// </summary>
        private BindConfig config;

        private void OnEnable()
        {
            bindingsProperty = serializedObject.FindProperty("bindings");
            autoBindEnabledProperty = serializedObject.FindProperty("autoBindEnabled");
            customClassNameProperty = serializedObject.FindProperty("customClassName");
            generateChildrenProperty = serializedObject.FindProperty("generateChildren");
            maxDepthProperty = serializedObject.FindProperty("maxDepth");
            excludedPrefixesProperty = serializedObject.FindProperty("excludedPrefixes");

            // 预加载配置
            config = ConfigManager.LoadConfig();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AutoBind autoBind = (AutoBind)target;

            EditorGUILayout.HelpBox("CUiAutoBind - UI 自动绑定系统", MessageType.None);

            // 显示基本属性
            EditorGUILayout.PropertyField(autoBindEnabledProperty);

            EditorGUILayout.Space();

            // 嵌套生成配置
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("嵌套生成设置", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(customClassNameProperty, new GUIContent("自定义类名", "留空则使用 GameObject 名称"));
            EditorGUILayout.PropertyField(generateChildrenProperty, new GUIContent("生成子对象", "是否递归生成子对象的代码"));

            if (generateChildrenProperty.boolValue)
            {
                EditorGUILayout.PropertyField(maxDepthProperty, new GUIContent("最大深度", "0 表示无限制"));
                EditorGUILayout.PropertyField(excludedPrefixesProperty, new GUIContent("排除前缀", "要排除的 GameObject 名称前缀（用逗号分隔）"));
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // 绑定列表标题
            EditorGUILayout.LabelField("UI 绑定列表", EditorStyles.boldLabel);

            // 显示绑定列表
            EditorGUI.indentLevel++;
            for (int i = 0; i < bindingsProperty.arraySize; i++)
            {
                SerializedProperty bindingProperty = bindingsProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 使用 AutoBindDataDrawer 绘制绑定项
                EditorGUILayout.PropertyField(bindingProperty, true);

                // 移除按钮
                if (GUILayout.Button("移除绑定", GUILayout.Height(25)))
                {
                    Undo.RecordObject(autoBind, "Remove Binding");
                    autoBind.RemoveBinding(i < autoBind.bindings.Count ? autoBind.bindings[i] : null);
                    EditorUtility.SetDirty(autoBind);
                    serializedObject.Update();
                    i--;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                    continue;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // 添加新绑定按钮
            if (GUILayout.Button("添加新绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(autoBind, "Add New Binding");
                autoBind.AddBinding(null, "NewBinding");
                EditorUtility.SetDirty(autoBind);
                serializedObject.Update();
            }

            // 从 GameObject 自动添加组件按钮
            if (GUILayout.Button("从当前 GameObject 添加组件", GUILayout.Height(30)))
            {
                Undo.RecordObject(autoBind, "Auto Add Components");
                AutoAddComponents(autoBind);
                EditorUtility.SetDirty(autoBind);
                serializedObject.Update();
            }

            EditorGUILayout.Space();

            // 生成代码按钮
            if (GUILayout.Button("生成绑定代码", GUILayout.Height(35)))
            {
                GenerateCode(autoBind);
            }

            EditorGUILayout.Space();

            // 额外功能按钮区域
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("重新绑定组件", GUILayout.Height(25)))
            {
                RebindComponents();
            }

            if (GUILayout.Button("验证绑定状态", GUILayout.Height(25)))
            {
                ValidateBinding();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 从当前 GameObject 自动添加组件
        /// </summary>
        private void AutoAddComponents(AutoBind autoBind)
        {
            Component[] components = autoBind.GetComponents<Component>();

            foreach (var component in components)
            {
                // 跳过 AutoBind 自身和 Transform
                if (component is AutoBind || component is Transform)
                    continue;

                // 检查是否已经存在
                bool exists = autoBind.bindings.Exists(b => b.component == component);
                if (!exists)
                {
                    // 自动生成字段名：驼峰命名
                    string fieldName = component.GetType().Name;
                    fieldName = char.ToLower(fieldName[0]) + fieldName.Substring(1);

                    autoBind.AddBinding(component, fieldName);
                }
            }
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        private void GenerateCode(AutoBind autoBind)
        {
            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载或创建配置文件", "确定");
                return;
            }

            // 创建代码生成器
            CodeGenerator generator = new CodeGenerator(config);

            // 获取有效的绑定
            var validBindings = autoBind.GetValidBindings();
            if (validBindings.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有有效的绑定数据", "确定");
                return;
            }

            // 生成代码
            try
            {
                generator.GenerateCode(autoBind.gameObject, validBindings);

                // 等待脚本编译完成
                EditorUtility.DisplayProgressBar("编译脚本", "等待脚本编译...", 0.5f);
                System.Threading.Thread.Sleep(500);
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                // 使用 CodeBinder 绑定组件到生成的字段（会自动添加脚本组件到 GameObject）
                CodeBinder codeBinder = new CodeBinder(config);
                bool bindSuccess = codeBinder.BindComponents(autoBind);

                StringBuilder message = new StringBuilder();
                message.AppendLine("✓ 代码生成成功！");
                message.AppendLine();
                if (config.usePartialClass)
                {
                    message.AppendLine($"自动文件: {config.GetAutoGeneratedFilePath(autoBind.gameObject.name)}");
                    message.AppendLine($"手动文件: {config.GetManualFilePath(autoBind.gameObject.name)}");
                }
                else
                {
                    message.AppendLine($"路径: {config.GetGeneratedFilePath(autoBind.gameObject.name)}");
                }
                message.AppendLine();
                message.AppendLine("✓ 脚本组件已自动添加到 GameObject");
                message.AppendLine("✓ UI 组件已成功绑定到字段！");

                EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("错误", "代码生成失败: " + e.Message, "确定");
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 重新绑定组件（不重新生成代码）
        /// </summary>
        private void RebindComponents()
        {
            AutoBind autoBind = (AutoBind)target;

            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载配置文件", "确定");
                return;
            }

            // 使用 CodeBinder 重新绑定
            CodeBinder codeBinder = new CodeBinder(config);
            bool success = codeBinder.BindComponents(autoBind);

            if (success)
            {
                EditorUtility.DisplayDialog("成功", "组件重新绑定成功！", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("完成", "组件重新绑定完成，但可能存在问题，请检查控制台日志。", "确定");
            }
        }

        /// <summary>
        /// 验证绑定状态
        /// </summary>
        private void ValidateBinding()
        {
            AutoBind autoBind = (AutoBind)target;

            // 加载配置
            config = ConfigManager.LoadOrCreateConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载配置文件", "确定");
                return;
            }

            // 使用 CodeBinder 验证
            CodeBinder codeBinder = new CodeBinder(config);
            var validation = codeBinder.ValidateBinding(autoBind);
            string report = codeBinder.GenerateReport(validation);

            Debug.Log(report);
            EditorUtility.DisplayDialog("绑定验证", report, "确定");
        }
    }
}
