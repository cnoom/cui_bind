using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CUiAutoBind
{
    /// <summary>
    /// 代码绑定类，负责将组件自动绑定到生成的脚本字段上
    /// </summary>
    public class CodeBinder
    {
        private BindConfig config;

        public CodeBinder(BindConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// 绑定单个 AutoBind 的组件到生成的脚本
        /// </summary>
        public bool BindComponents(AutoBind autoBind)
        {
            if (autoBind == null)
            {
                Debug.LogError("CodeBinder: AutoBind is null");
                return false;
            }

            string className = autoBind.GetUIClassName();
            GameObject targetGameObject = autoBind.gameObject;

            // 尝试获取或添加生成的脚本组件
            Component generatedComponent = GetOrAddGeneratedScriptComponent(targetGameObject, className);
            if (generatedComponent == null)
            {
                Debug.LogWarning($"CodeBinder: 无法找到或添加生成的脚本组件 '{className}' 到对象 '{targetGameObject.name}' 上");
                return false;
            }

            // 使用 SerializedObject 来安全地修改字段
            SerializedObject serializedObject = new SerializedObject(generatedComponent);

            // 绑定当前 AutoBind 的字段
            BindFields(autoBind, serializedObject);

            // 应用修改
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(generatedComponent);

            Debug.Log($"CodeBinder: 成功绑定组件到 '{className}'");

            // 递归绑定子对象（如果需要）
            if (autoBind.generateChildren)
            {
                BindChildComponents(autoBind);
            }

            return true;
        }

        /// <summary>
        /// 获取或添加生成的脚本组件到 GameObject
        /// </summary>
        private Component GetOrAddGeneratedScriptComponent(GameObject target, string className)
        {
            // 首先尝试获取已存在的组件
            Component existingComponent = GetGeneratedComponent(target, className);
            if (existingComponent != null)
            {
                return existingComponent;
            }

            // 如果不存在，尝试添加新组件
            return AddGeneratedScriptToGameObject(target, className);
        }

        /// <summary>
        /// 将生成的脚本添加到 GameObject 上
        /// </summary>
        private Component AddGeneratedScriptToGameObject(GameObject target, string className)
        {
            // 等待 Unity 编译生成的脚本
            AssetDatabase.Refresh();

            // 获取脚本类型
            Type scriptType = GetScriptType(className);
            if (scriptType == null)
            {
                Debug.LogError($"CodeBinder: 无法找到脚本类型 '{className}'，请确保脚本已编译");
                return null;
            }

            // 检查该组件是否已存在（避免重复添加）
            Component existingComponent = target.GetComponent(scriptType);
            if (existingComponent != null)
            {
                return existingComponent;
            }

            // 添加脚本组件
            Component newComponent = target.AddComponent(scriptType);
            if (newComponent != null)
            {
                Debug.Log($"CodeBinder: 成功添加脚本组件 '{className}' 到 '{target.name}'");
                EditorUtility.SetDirty(target);
                return newComponent;
            }
            else
            {
                Debug.LogError($"CodeBinder: 添加脚本组件 '{className}' 到 '{target.name}' 失败");
                return null;
            }
        }

        /// <summary>
        /// 获取脚本类型
        /// </summary>
        private Type GetScriptType(string className)
        {
            // 尝试从所有已加载的程序集中查找类型
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // 尝试完整类型名（包含命名空间）
                    if (!string.IsNullOrEmpty(config.namespaceName))
                    {
                        string fullClassName = $"{config.namespaceName}.{className}";
                        Type type = assembly.GetType(fullClassName, false, true);
                        if (type != null)
                            return type;
                    }

                    // 尝试仅类名
                    Type typeByName = assembly.GetType(className, false, true);
                    if (typeByName != null)
                        return typeByName;
                }
                catch
                {
                    // 某些程序集可能无法加载类型，忽略
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取生成的脚本组件
        /// </summary>
        private Component GetGeneratedComponent(GameObject target, string className)
        {
            // 获取所有组件（包括 MonoBehaviour 脚本）
            Component[] components = target.GetComponents<Component>();

            // 查找类型名称匹配的组件
            foreach (var component in components)
            {
                if (component == null)
                    continue;

                Type componentType = component.GetType();

                // 跳过 Unity 内置组件
                if (IsUnityBuiltInComponent(componentType))
                    continue;

                // 检查类型名称是否匹配
                if (componentType.Name == className)
                    return component;
            }

            // 如果使用命名空间，尝试完整名称匹配
            if (!string.IsNullOrEmpty(config.namespaceName))
            {
                string fullClassName = $"{config.namespaceName}.{className}";
                Type type = System.Type.GetType(fullClassName);
                if (type != null)
                {
                    return target.GetComponent(type);
                }
            }

            return null;
        }

        /// <summary>
        /// 绑定字段
        /// </summary>
        private void BindFields(AutoBind autoBind, SerializedObject serializedObject)
        {
            List<AutoBindData> bindings = autoBind.GetValidBindings();

            foreach (var binding in bindings)
            {
                if (binding == null || !binding.generateField)
                    continue;

                string fieldName = binding.fieldName;

                // 查找对应的 SerializedProperty
                SerializedProperty property = serializedObject.FindProperty(fieldName);
                if (property == null)
                {
                    Debug.LogWarning($"CodeBinder: 字段 '{fieldName}' 在脚本中未找到");
                    continue;
                }

                // 根据绑定类型赋值
                if (!AssignFieldValue(property, binding))
                {
                    Debug.LogWarning($"CodeBinder: 无法赋值字段 '{fieldName}'");
                }
            }
        }

        /// <summary>
        /// 赋值字段
        /// </summary>
        private bool AssignFieldValue(SerializedProperty property, AutoBindData binding)
        {
            if (binding == null || binding.component == null)
                return false;

            // AutoBind 引用类型
            if (binding.IsAutoBindReference())
            {
                return AssignAutoBindReference(property, binding);
            }

            // 普通组件类型
            return AssignComponentReference(property, binding);
        }

        /// <summary>
        /// 赋值 AutoBind 引用字段
        /// </summary>
        private bool AssignAutoBindReference(SerializedProperty property, AutoBindData binding)
        {
            AutoBind childAutoBind = binding.component as AutoBind;
            if (childAutoBind == null)
                return false;

            string childClassName = childAutoBind.GetUIClassName();

            // 获取子对象的脚本组件
            Component childScript = GetGeneratedComponent(childAutoBind.gameObject, childClassName);
            if (childScript == null)
            {
                Debug.LogWarning($"CodeBinder: 无法找到子对象的脚本组件 '{childClassName}'");
                return false;
            }

            property.objectReferenceValue = childScript;
            return true;
        }

        /// <summary>
        /// 赋值普通组件引用字段
        /// </summary>
        private bool AssignComponentReference(SerializedProperty property, AutoBindData binding)
        {
            // 直接赋值组件
            property.objectReferenceValue = binding.component;
            return true;
        }

        /// <summary>
        /// 递归绑定子对象
        /// </summary>
        private void BindChildComponents(AutoBind parentAutoBind)
        {
            List<AutoBind> childAutoBinds = parentAutoBind.GetChildAutoBinds();

            foreach (var childAutoBind in childAutoBinds)
            {
                BindComponents(childAutoBind);
            }
        }

        /// <summary>
        /// 批量绑定多个 AutoBind
        /// </summary>
        public BindingResult BindMultiple(AutoBind[] autoBinds)
        {
            BindingResult result = new BindingResult();

            if (autoBinds == null || autoBinds.Length == 0)
            {
                return result;
            }

            foreach (var autoBind in autoBinds)
            {
                try
                {
                    if (BindComponents(autoBind))
                    {
                        result.successCount++;
                        result.successList.Add(autoBind.gameObject.name);
                    }
                    else
                    {
                        result.failureCount++;
                        result.failureList.Add(autoBind.gameObject.name);
                    }
                }
                catch (System.Exception e)
                {
                    result.failureCount++;
                    result.failureList.Add(autoBind.gameObject.name);
                    result.errors.Add($"{autoBind.gameObject.name}: {e.Message}");
                    Debug.LogError(e);
                }
            }

            return result;
        }

        /// <summary>
        /// 检查是否是 Unity 内置组件
        /// </summary>
        private bool IsUnityBuiltInComponent(Type type)
        {
            if (type == null)
                return false;

            string typeName = type.FullName ?? "";
            string @namespace = type.Namespace ?? "";

            // Unity 内置的命名空间
            string[] unityNamespaces = new string[]
            {
                "UnityEngine",
                "UnityEngine.UI",
                "UnityEngine.EventSystems"
            };

            // 检查是否在内置命名空间中
            foreach (var ns in unityNamespaces)
            {
                if (@namespace.StartsWith(ns))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 验证绑定状态
        /// </summary>
        public BindingValidation ValidateBinding(AutoBind autoBind)
        {
            BindingValidation validation = new BindingValidation();
            validation.autoBindName = autoBind.gameObject.name;

            string className = autoBind.GetUIClassName();
            GameObject targetGameObject = autoBind.gameObject;

            // 检查生成的脚本是否存在
            Component generatedComponent = GetGeneratedComponent(targetGameObject, className);
            if (generatedComponent == null)
            {
                validation.isValid = false;
                validation.errors.Add($"生成的脚本 '{className}' 不存在");
                return validation;
            }

            validation.scriptType = generatedComponent.GetType().Name;

            // 获取所有字段
            SerializedObject serializedObject = new SerializedObject(generatedComponent);
            SerializedProperty property = serializedObject.GetIterator();

            List<string> scriptFields = new List<string>();

            // 收集脚本中的字段
            while (property.Next(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    scriptFields.Add(property.name);
                }
            }

            // 验证每个绑定
            List<AutoBindData> bindings = autoBind.GetValidBindings();

            foreach (var binding in bindings)
            {
                if (binding == null || !binding.generateField)
                    continue;

                BindingFieldStatus fieldStatus = new BindingFieldStatus();
                fieldStatus.fieldName = binding.fieldName;
                fieldStatus.targetComponent = binding.component != null ? binding.component.GetType().Name : "Null";

                // 检查字段是否存在
                if (!scriptFields.Contains(binding.fieldName))
                {
                    fieldStatus.isBound = false;
                    fieldStatus.message = "字段不存在";
                    validation.invalidFields.Add(fieldStatus);
                    continue;
                }

                // 检查字段是否已赋值
                SerializedProperty fieldProperty = serializedObject.FindProperty(binding.fieldName);
                if (fieldProperty != null && fieldProperty.objectReferenceValue == null)
                {
                    fieldStatus.isBound = false;
                    fieldStatus.message = "字段未赋值";
                    validation.invalidFields.Add(fieldStatus);
                    continue;
                }

                fieldStatus.isBound = true;
                fieldStatus.message = "已绑定";
                validation.validFields.Add(fieldStatus);
            }

            validation.isValid = validation.invalidFields.Count == 0;

            return validation;
        }

        /// <summary>
        /// 生成绑定报告
        /// </summary>
        public string GenerateReport(BindingValidation validation)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"=== 绑定验证报告 ===");
            sb.AppendLine($"对象: {validation.autoBindName}");
            sb.AppendLine($"脚本类型: {validation.scriptType}");
            sb.AppendLine($"状态: {(validation.isValid ? "✓ 有效" : "✗ 无效")}");
            sb.AppendLine();

            sb.AppendLine($"有效绑定 ({validation.validFields.Count}):");
            foreach (var field in validation.validFields)
            {
                sb.AppendLine($"  ✓ {field.fieldName} ({field.targetComponent}) - {field.message}");
            }

            if (validation.invalidFields.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"无效绑定 ({validation.invalidFields.Count}):");
                foreach (var field in validation.invalidFields)
                {
                    sb.AppendLine($"  ✗ {field.fieldName} ({field.targetComponent}) - {field.message}");
                }
            }

            if (validation.errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("错误信息:");
                foreach (var error in validation.errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 批量绑定结果
    /// </summary>
    public class BindingResult
    {
        public int successCount;
        public int failureCount;
        public List<string> successList = new List<string>();
        public List<string> failureList = new List<string>();
        public List<string> errors = new List<string>();

        public override string ToString()
        {
            return $"绑定完成 - 成功: {successCount}, 失败: {failureCount}";
        }
    }

    /// <summary>
    /// 绑定验证结果
    /// </summary>
    public class BindingValidation
    {
        public string autoBindName;
        public string scriptType;
        public bool isValid;
        public List<BindingFieldStatus> validFields = new List<BindingFieldStatus>();
        public List<BindingFieldStatus> invalidFields = new List<BindingFieldStatus>();
        public List<string> errors = new List<string>();
    }

    /// <summary>
    /// 字段绑定状态
    /// </summary>
    public class BindingFieldStatus
    {
        public string fieldName;
        public string targetComponent;
        public bool isBound;
        public string message;
    }
}
