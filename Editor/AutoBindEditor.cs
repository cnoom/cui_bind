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
        private SerializedProperty bindModeProperty;
        private SerializedProperty customClassNameProperty;
        private SerializedProperty showBindingListProperty;

        /// <summary>
        /// 缓存的配置，用于 AutoAssignComponents
        /// </summary>
        private BindConfig config;

        private void OnEnable()
        {
            bindingsProperty = serializedObject.FindProperty("bindings");
            bindModeProperty = serializedObject.FindProperty("bindMode");
            customClassNameProperty = serializedObject.FindProperty("customClassName");
            showBindingListProperty = serializedObject.FindProperty("showBindingList");

            // 预加载配置
            config = ConfigManager.LoadConfig();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AutoBind autoBind = (AutoBind)target;

            // 显示绑定模式选择
            EditorGUILayout.LabelField("绑定模式", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(bindModeProperty, new GUIContent("绑定方式", "选择绑定模式"));
            EditorGUILayout.HelpBox(autoBind.GetBindModeDescription(), MessageType.Info);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(customClassNameProperty, new GUIContent("自定义类名", "留空则使用 GameObject 名称"));

            EditorGUILayout.Space();
            
            // 绑定列表标题（使用 Foldout）
            showBindingListProperty.boolValue = EditorGUILayout.Foldout(showBindingListProperty.boolValue, "UI 绑定列表 (" + bindingsProperty.arraySize + ")", true, EditorStyles.boldLabel);

            // 显示绑定列表
            if (showBindingListProperty.boolValue)
            {
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
            }

            EditorGUILayout.Space();

            // 根据绑定模式显示不同的操作按钮
            switch (autoBind.bindMode)
            {
                case BindMode.Manual:
                    DrawManualBindingButtons(autoBind);
                    break;
                case BindMode.AutoSuffix:
                    DrawAutoSuffixBindingButtons(autoBind);
                    break;
                case BindMode.Hybrid:
                    DrawHybridBindingButtons(autoBind);
                    break;
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

            if (GUILayout.Button("绑定组件", GUILayout.Height(25)))
            {
                RebindComponents();
            }

            if (GUILayout.Button("验证绑定", GUILayout.Height(25)))
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
        /// 按命名约定自动绑定
        /// </summary>
        private void AutoBindByNamingConvention(AutoBind autoBind)
        {
            // 加载配置
            BindConfig config = ConfigManager.LoadConfig();
            if (config == null || config.suffixConfigs == null || config.suffixConfigs.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先在配置文件中添加命名规则", "确定");
                return;
            }

            // 统计信息
            int addedCount = 0;
            int skippedCount = 0;
            int notFoundCount = 0;

            // 递归遍历所有子对象
            AutoBindByNamingConventionRecursive(autoBind.transform, autoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);

            // 显示结果
            StringBuilder message = new StringBuilder();
            message.AppendLine($"自动绑定完成！");
            message.AppendLine($"✓ 新增绑定: {addedCount}");
            message.AppendLine($"○ 已存在（跳过）: {skippedCount}");
            if (notFoundCount > 0)
            {
                message.AppendLine($"✗ 未找到组件: {notFoundCount}");
            }

            EditorUtility.DisplayDialog("完成", message.ToString(), "确定");
        }

        /// <summary>
        /// 递归按命名约定自动绑定
        /// </summary>
        private void AutoBindByNamingConventionRecursive(Transform current, AutoBind parentAutoBind, BindConfig config, ref int addedCount, ref int skippedCount, ref int notFoundCount)
        {
            AutoBindUtility.AutoBindByNamingConventionRecursive(current, parentAutoBind, config, ref addedCount, ref skippedCount, ref notFoundCount);
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
                message.AppendLine($"自动文件: {config.GetAutoGeneratedFilePath(autoBind.gameObject.name)}");
                message.AppendLine($"手动文件: {config.GetManualFilePath(autoBind.gameObject.name)}");
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

        /// <summary>
        /// 绘制手动拖拽绑定模式的按钮
        /// </summary>
        private void DrawManualBindingButtons(AutoBind autoBind)
        {
            EditorGUILayout.LabelField("手动拖拽绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("完全手动添加绑定，适合精确控制的场景。", MessageType.None);

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
        }

        /// <summary>
        /// 绘制后缀自动绑定模式的按钮
        /// </summary>
        private void DrawAutoSuffixBindingButtons(AutoBind autoBind)
        {
            EditorGUILayout.LabelField("后缀自动绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("根据命名后缀自动扫描并绑定，适合大量组件的场景。", MessageType.None);

            // 按命名约定自动绑定按钮
            if (GUILayout.Button("按命名约定自动绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(autoBind, "Auto Bind By Naming Convention");
                AutoBindByNamingConvention(autoBind);
                EditorUtility.SetDirty(autoBind);
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox("提示：命名约定会递归扫描所有子对象，根据后缀规则自动绑定。可在配置文件中自定义后缀规则。", MessageType.Info);
        }

        /// <summary>
        /// 绘制混合绑定模式的按钮
        /// </summary>
        private void DrawHybridBindingButtons(AutoBind autoBind)
        {
            EditorGUILayout.LabelField("混合绑定", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("同时支持手动和后缀自动绑定，适合复杂场景。", MessageType.None);

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

            // 按命名约定自动绑定按钮
            if (GUILayout.Button("按命名约定自动绑定", GUILayout.Height(30)))
            {
                Undo.RecordObject(autoBind, "Auto Bind By Naming Convention");
                AutoBindByNamingConvention(autoBind);
                EditorUtility.SetDirty(autoBind);
                serializedObject.Update();
            }

            EditorGUILayout.HelpBox("提示：可以同时使用手动添加和后缀自动绑定。后缀自动绑定会自动跳过已绑定的组件。", MessageType.Info);
        }
    }
}
