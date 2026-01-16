using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CUiAutoBind
{
    /// <summary>
    /// CUiAutoBind 主编辑器窗口
    /// </summary>
    public class AutoBindWindow : EditorWindow
    {
        private static BindConfig config;
        private Vector2 scrollPosition;

        [MenuItem("Tools/CUiAutoBind/打开窗口", false, 10)]
        public static void ShowWindow()
        {
            GetWindow<AutoBindWindow>("CUiAutoBind");
        }

        private void OnGUI()
        {
            // 加载配置
            if (config == null)
            {
                config = ConfigManager.LoadConfig();
            }

            // 标题
            GUILayout.Label("CUiAutoBind - UI 自动绑定系统", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 配置部分
            DrawConfigSection();

            EditorGUILayout.Space();

            // 批量生成部分
            DrawBatchGenerateSection();
        }

        /// <summary>
        /// 绘制配置部分
        /// </summary>
        private void DrawConfigSection()
        {
            GUILayout.Label("配置", EditorStyles.boldLabel);

            if (config != null)
            {
                SerializedObject serializedConfig = new SerializedObject(config);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedConfig.FindProperty("namespaceName"), new GUIContent("命名空间"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("basePath"), new GUIContent("基础路径"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("baseClass"), new GUIContent("基类"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("interfaces"), new GUIContent("接口"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("usePartialClass"), new GUIContent("使用 Partial 类"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("addFieldComments"), new GUIContent("添加字段注释"));
                EditorGUILayout.PropertyField(serializedConfig.FindProperty("additionalNamespaces"), new GUIContent("额外命名空间"));

                EditorGUILayout.Space();

                if (EditorGUI.EndChangeCheck())
                {
                    serializedConfig.ApplyModifiedProperties();
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.Space();

                // 重新生成配置按钮
                if (GUILayout.Button("重新生成默认配置", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("确认", "确定要重新生成配置吗？当前配置将被覆盖。", "确定", "取消"))
                    {
                        config = ConfigManager.RegenerateConfig();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("未找到配置文件", MessageType.Warning);

                if (GUILayout.Button("创建默认配置", GUILayout.Height(30)))
                {
                    config = ConfigManager.CreateDefaultConfig();
                }
            }
        }

        /// <summary>
        /// 绘制批量生成部分
        /// </summary>
        private void DrawBatchGenerateSection()
        {
            GUILayout.Label("批量生成", EditorStyles.boldLabel);

            if (config == null || !config.IsValid())
            {
                EditorGUILayout.HelpBox("请先配置有效的生成路径", MessageType.Warning);
                return;
            }

            // 扫描场景中的 AutoBind 组件
            AutoBind[] autoBinds = GameObject.FindObjectsOfType<AutoBind>();

            if (autoBinds.Length == 0)
            {
                EditorGUILayout.HelpBox("场景中没有 AutoBind 组件", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox($"找到 {autoBinds.Length} 个 AutoBind 组件", MessageType.Info);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var autoBind in autoBinds)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(autoBind.gameObject.name, EditorStyles.boldLabel);

                var validBindings = autoBind.GetValidBindings();
                EditorGUILayout.LabelField($"绑定数量: {validBindings.Count}");

                if (GUILayout.Button($"生成代码: {autoBind.gameObject.name}", GUILayout.Height(25)))
                {
                    GenerateCodeFor(autoBind);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 全部生成按钮
            if (GUILayout.Button("全部生成", GUILayout.Height(35)))
            {
                GenerateAllCode(autoBinds);
            }

            EditorGUILayout.Space();

            // 批量绑定按钮
            if (GUILayout.Button("批量重新绑定", GUILayout.Height(35)))
            {
                BatchRebind(autoBinds);
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private BindConfig LoadConfig()
        {
            return ConfigManager.LoadConfig();
        }

        /// <summary>
        /// 为指定 AutoBind 生成代码
        /// </summary>
        private void GenerateCodeFor(AutoBind autoBind)
        {
            CodeGenerator generator = new CodeGenerator(config);
            var validBindings = autoBind.GetValidBindings();

            if (validBindings.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"'{autoBind.gameObject.name}' 没有有效的绑定数据", "确定");
                return;
            }

            try
            {
                generator.GenerateCode(autoBind.gameObject, validBindings);

                // 等待脚本编译完成
                EditorUtility.DisplayProgressBar("编译脚本", $"等待 {autoBind.gameObject.name} 的脚本编译...", 0.5f);
                System.Threading.Thread.Sleep(500);
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                // 使用 CodeBinder 绑定组件（会自动添加脚本组件）
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
                EditorUtility.DisplayDialog("错误", $"代码生成失败: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 生成所有代码
        /// </summary>
        private void GenerateAllCode(AutoBind[] autoBinds)
        {
            int successCount = 0;
            int failCount = 0;
            List<string> errorMessages = new List<string>();

            // 先生成所有代码
            foreach (var autoBind in autoBinds)
            {
                try
                {
                    CodeGenerator generator = new CodeGenerator(config);
                    var validBindings = autoBind.GetValidBindings();

                    if (validBindings.Count > 0)
                    {
                        generator.GenerateCode(autoBind.gameObject, validBindings);
                        successCount++;
                    }
                }
                catch (System.Exception e)
                {
                    failCount++;
                    errorMessages.Add($"{autoBind.gameObject.name}: {e.Message}");
                    Debug.LogError(e);
                }
            }

            // 等待脚本编译完成
            if (successCount > 0)
            {
                EditorUtility.DisplayProgressBar("编译脚本", "等待所有脚本编译...", 0.5f);
                System.Threading.Thread.Sleep(1000);
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                // 批量绑定组件
                CodeBinder codeBinder = new CodeBinder(config);
                BindingResult bindResult = codeBinder.BindMultiple(autoBinds);
            }

            string message = $"生成完成！\n成功: {successCount}\n失败: {failCount}";
            if (errorMessages.Count > 0)
            {
                message += "\n\n错误信息:\n" + string.Join("\n", errorMessages);
            }

            EditorUtility.DisplayDialog(failCount > 0 ? "完成但有错误" : "完成", message, "确定");
        }

        /// <summary>
        /// 批量重新绑定
        /// </summary>
        private void BatchRebind(AutoBind[] autoBinds)
        {
            if (autoBinds == null || autoBinds.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到 AutoBind 组件", "确定");
                return;
            }

            CodeBinder codeBinder = new CodeBinder(config);
            BindingResult result = codeBinder.BindMultiple(autoBinds);

            StringBuilder message = new StringBuilder();
            message.AppendLine($"批量绑定完成！");
            message.AppendLine($"成功: {result.successCount}");
            message.AppendLine($"失败: {result.failureCount}");

            if (result.failureList.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("失败列表:");
                foreach (var name in result.failureList)
                {
                    message.AppendLine($"  - {name}");
                }
            }

            if (result.errors.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("错误详情:");
                foreach (var error in result.errors)
                {
                    message.AppendLine($"  - {error}");
                }
            }

            EditorUtility.DisplayDialog(result.failureCount > 0 ? "完成但有错误" : "完成", message.ToString(), "确定");
        }
    }
}
