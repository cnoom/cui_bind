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
        private SerializedProperty excludedPrefixesProperty;
        private SerializedProperty showBindingListProperty;

        /// <summary>
        /// 缓存的配置，用于 AutoAssignComponents
        /// </summary>
        private BindConfig config;

        private void OnEnable()
        {
            bindingsProperty = serializedObject.FindProperty("bindings");
            autoBindEnabledProperty = serializedObject.FindProperty("autoBindEnabled");
            customClassNameProperty = serializedObject.FindProperty("customClassName");
            excludedPrefixesProperty = serializedObject.FindProperty("excludedPrefixes");
            showBindingListProperty = serializedObject.FindProperty("showBindingList");

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
            EditorGUILayout.PropertyField(excludedPrefixesProperty, new GUIContent("排除前缀", "要排除的 GameObject 名称前缀（用逗号分隔）"));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // 命名约定自动绑定部分
            EditorGUILayout.LabelField("命名约定自动绑定", EditorStyles.boldLabel);

            // 显示当前配置的后缀规则
            BindConfig config = ConfigManager.LoadConfig();
            if (config != null && config.suffixConfigs != null && config.suffixConfigs.Length > 0)
            {
                EditorGUILayout.HelpBox($"配置了 {config.suffixConfigs.Length} 个命名规则", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("未配置命名规则，请在配置文件中添加", MessageType.Warning);
            }

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

            // 获取排除前缀
            string[] excludedPrefixes = GetExcludedPrefixes(autoBind);

            // 统计信息
            int addedCount = 0;
            int skippedCount = 0;
            int notFoundCount = 0;

            // 递归遍历所有子对象
            AutoBindByNamingConventionRecursive(autoBind.transform, autoBind, config, excludedPrefixes, ref addedCount, ref skippedCount, ref notFoundCount);

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
        private void AutoBindByNamingConventionRecursive(Transform current, AutoBind parentAutoBind, BindConfig config, string[] excludedPrefixes, ref int addedCount, ref int skippedCount, ref int notFoundCount)
        {
            // 跳过父对象自身
            if (current == parentAutoBind.transform)
            {
                // 只遍历直接子对象
                foreach (Transform child in current)
                {
                    AutoBindByNamingConventionRecursive(child, parentAutoBind, config, excludedPrefixes, ref addedCount, ref skippedCount, ref notFoundCount);
                }
                return;
            }

            // 检查排除前缀
            if (IsExcluded(current.name, excludedPrefixes))
                return;

            // 检查是否有AutoBind组件（如果有，说明这个对象由自己管理，跳过）
            if (current.GetComponent<AutoBind>() != null)
                return;

            // 检查是否匹配命名约定
            SuffixConfig matchedSuffix = null;
            foreach (var suffixConfig in config.suffixConfigs)
            {
                if (current.name.EndsWith(suffixConfig.suffix, System.StringComparison.OrdinalIgnoreCase))
                {
                    matchedSuffix = suffixConfig;
                    break;
                }
            }

            // 如果匹配到命名规则，尝试绑定
            if (matchedSuffix != null)
            {
                // 尝试获取组件类型
                System.Type componentType = GetComponentType(matchedSuffix);
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
                AutoBindByNamingConventionRecursive(child, parentAutoBind, config, excludedPrefixes, ref addedCount, ref skippedCount, ref notFoundCount);
            }
        }

        /// <summary>
        /// 根据后缀配置获取组件类型
        /// </summary>
        private System.Type GetComponentType(SuffixConfig suffixConfig)
        {
            if (suffixConfig.componentType == null || string.IsNullOrEmpty(suffixConfig.componentType.FullTypeName))
                return null;

            // 尝试从已加载的程序集中查找类型
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // 使用完整的类型名
                    System.Type type = assembly.GetType(suffixConfig.componentType.FullTypeName, false, true);
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
        private string ConvertToFieldName(string objectName, string suffix)
        {
            // 移除后缀（不区分大小写）
            string fieldName = objectName;
            if (objectName.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// 获取排除前缀数组
        /// </summary>
        private string[] GetExcludedPrefixes(AutoBind autoBind)
        {
            if (string.IsNullOrEmpty(autoBind.excludedPrefixes))
                return new string[0];

            return autoBind.excludedPrefixes.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
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
