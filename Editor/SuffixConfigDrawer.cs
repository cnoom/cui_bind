using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CUiAutoBind
{
    /// <summary>
    /// 组件类型信息
    /// </summary>
    public class ComponentTypeInfo
    {
        public string displayName;
        public string componentType;
        public string namespaceName;
        public Type type;

        public ComponentTypeInfo(string displayName, string componentType, string namespaceName, Type type)
        {
            this.displayName = displayName;
            this.componentType = componentType;
            this.namespaceName = namespaceName;
            this.type = type;
        }

        public override string ToString()
        {
            return displayName;
        }
    }

    /// <summary>
    /// 组件类型扫描器
    /// </summary>
    public static class ComponentTypeScanner
    {
        private static List<ComponentTypeInfo> cachedTypes = null;

        /// <summary>
        /// 扫描所有可用的组件类型
        /// </summary>
        public static List<ComponentTypeInfo> ScanAllComponentTypes()
        {
            if (cachedTypes != null)
            {
                return cachedTypes;
            }

            cachedTypes = new List<ComponentTypeInfo>();

            try
            {
                // 获取所有已加载的程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // 跳过系统程序集
                        if (assembly.FullName.StartsWith("System") ||
                            assembly.FullName.StartsWith("mscorlib") ||
                            assembly.FullName.StartsWith("Microsoft"))
                            continue;

                        // 获取所有类型
                        var types = assembly.GetTypes();

                        foreach (var type in types)
                        {
                            // 只选择继承自 Component 的类型
                            if (typeof(Component).IsAssignableFrom(type) &&
                                !type.IsAbstract &&
                                !type.IsInterface &&
                                type.IsClass)
                            {
                                // 过滤掉 UnityEngine 的基础组件（除非是 UI 组件）
                                if (type.Namespace == "UnityEngine" && type.Namespace != "UnityEngine.UI")
                                {
                                    // 只包含一些常用的基础组件
                                    if (type.Name != "Transform" && type.Name != "RectTransform" && type.Name != "Behaviour" && type.Name != "Component")
                                        continue;
                                }

                                string namespaceName = type.Namespace ?? "";
                                string displayName = string.IsNullOrEmpty(namespaceName) ? type.Name : $"{namespaceName}.{type.Name}";

                                cachedTypes.Add(new ComponentTypeInfo(displayName, type.Name, namespaceName, type));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略无法访问的程序集
                        continue;
                    }
                }

                // 排序：先按命名空间，再按类型名
                cachedTypes.Sort((a, b) =>
                {
                    int namespaceCompare = string.Compare(a.namespaceName, b.namespaceName, StringComparison.Ordinal);
                    if (namespaceCompare != 0) return namespaceCompare;
                    return string.Compare(a.componentType, b.componentType, StringComparison.Ordinal);
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"扫描组件类型失败: {e.Message}");
            }

            return cachedTypes;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            cachedTypes = null;
        }
    }

    /// <summary>
    /// SuffixConfig 的自定义属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(SuffixConfig))]
    public class SuffixConfigDrawer : PropertyDrawer
    {
        // 每个属性实例的状态管理
        private class DrawerState
        {
            public List<ComponentTypeInfo> filteredTypes;
            public string searchText = "";
            public List<ComponentTypeInfo> recentlyUsed = new List<ComponentTypeInfo>();
            public bool showAdvancedDropdown = false;
            public bool showSearchMode = false;
        }

        private static Dictionary<int, DrawerState> stateMap = new Dictionary<int, DrawerState>();
        private List<ComponentTypeInfo> componentTypes;

        /// <summary>
        /// 获取或创建当前属性的状态
        /// </summary>
        private DrawerState GetState(SerializedProperty property)
        {
            int hashCode = property.propertyPath.GetHashCode();
            if (!stateMap.ContainsKey(hashCode))
            {
                stateMap[hashCode] = new DrawerState();
            }
            return stateMap[hashCode];
        }

        /// <summary>
        /// 清理状态缓存
        /// </summary>
        public static void ClearStateCache()
        {
            stateMap.Clear();
        }

        // 常用后缀名称
        private static readonly string[] commonSuffixes = new string[]
        {
            "_btn", "_txt", "_img", "_tgl", "_slr", "_inp", "_scr", "_grid", "_svr", "_canvas",
            "_rect", "_group", "_layout", "_raw", "_mask", "_anim", "_audio", "_camera"
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 每次都获取当前 property 的子属性，避免缓存导致的多实例共享问题
            SerializedProperty suffixProp = property.FindPropertyRelative("suffix");
            SerializedProperty componentTypeProp = property.FindPropertyRelative("componentType");

            // 扫描所有可用的组件类型
            if (componentTypes == null)
            {
                componentTypes = ComponentTypeScanner.ScanAllComponentTypes();
            }

            // 开始绘制
            EditorGUI.BeginProperty(position, label, property);

            // 第一行：后缀名称输入框
            Rect line1Rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawSuffixDropdown(line1Rect, suffixProp);

            // 第二行：组件类型选择（搜索 + 常用快速选择）
            float startY = position.y + EditorGUIUtility.singleLineHeight + 2;
            DrawComponentTypeSelection(new Rect(position.x, startY, position.width, EditorGUIUtility.singleLineHeight * 2), componentTypeProp, suffixProp, property);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 绘制后缀下拉选择
        /// </summary>
        private void DrawSuffixDropdown(Rect rect, SerializedProperty suffixProp)
        {
            // 绘制文本输入框
            string newSuffix = EditorGUI.DelayedTextField(rect, "后缀名称", suffixProp.stringValue);

            if (newSuffix != suffixProp.stringValue)
            {
                suffixProp.stringValue = newSuffix;
            }
        }

        /// <summary>
        /// 绘制组件类型选择区域
        /// </summary>
        private void DrawComponentTypeSelection(Rect rect, SerializedProperty componentTypeProp, SerializedProperty suffixProp, SerializedProperty property)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 获取 fullTypeName 属性
            SerializedProperty fullTypeNameProp = componentTypeProp.FindPropertyRelative("fullTypeName");
            if (fullTypeNameProp == null)
            {
                EditorGUI.LabelField(rect, "配置错误");
                return;
            }

            // 根据是否在搜索模式显示不同的内容
            if (state.showSearchMode)
            {
                // 显示搜索框和搜索结果
                DrawSearchBox(rect, fullTypeNameProp, suffixProp, property);
            }
            else
            {
                // 显示当前选中信息 + 搜索按钮
                DrawStaticSelection(rect, fullTypeNameProp, suffixProp, property);
            }
        }

        /// <summary>
        /// 绘制静态选择界面（显示当前选中信息和搜索按钮）
        /// </summary>
        private void DrawStaticSelection(Rect rect, SerializedProperty fullTypeNameProp, SerializedProperty suffixProp, SerializedProperty property)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 第一行：显示当前选中信息和搜索按钮
            float infoHeight = EditorGUIUtility.singleLineHeight;

            // 显示当前选中信息
            if (!string.IsNullOrEmpty(fullTypeNameProp.stringValue))
            {
                ComponentTypeInfo currentType = componentTypes.Find(t => t.displayName == fullTypeNameProp.stringValue);
                if (currentType != null)
                {
                    Rect infoRect = new Rect(rect.x, rect.y + 4, rect.width - 55, infoHeight);
                    GUIContent infoContent = new GUIContent($"选中：{currentType.displayName}", currentType.displayName);
                    EditorGUI.LabelField(infoRect, infoContent, EditorStyles.miniLabel);
                }
            }
            else
            {
                Rect infoRect = new Rect(rect.x, rect.y + 4, rect.width - 55, infoHeight);
                EditorGUI.LabelField(infoRect, "未选择组件类型", EditorStyles.miniLabel);
            }

            // 搜索按钮
            Rect searchButtonRect = new Rect(rect.x + rect.width - 50, rect.y + 4, 45, infoHeight);
            if (GUI.Button(searchButtonRect, "搜索", EditorStyles.miniButton))
            {
                state.showSearchMode = true;
                state.searchText = "";
                FilterComponentTypes(state);
            }
        }

        /// <summary>
        /// 绘制搜索框和搜索结果
        /// </summary>
        private void DrawSearchBox(Rect rect, SerializedProperty fullTypeNameProp, SerializedProperty suffixProp, SerializedProperty property)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 搜索框区域
            Rect searchLabelRect = new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight);
            Rect searchRect = new Rect(rect.x + 65, rect.y, rect.width - 160, EditorGUIUtility.singleLineHeight);
            Rect backButtonRect = new Rect(rect.x + rect.width - 45, rect.y, 40, EditorGUIUtility.singleLineHeight);

            // 搜索结果区域
            Rect resultRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 4, rect.width, EditorGUIUtility.singleLineHeight);

            // 绘制搜索框标签
            EditorGUI.LabelField(searchLabelRect, "搜索：", EditorStyles.miniLabel);

            // 绘制搜索框
            GUI.SetNextControlName("ComponentTypeSearch_" + property.propertyPath.GetHashCode());
            string newSearchText = EditorGUI.TextField(searchRect, state.searchText);
            if (newSearchText != state.searchText)
            {
                state.searchText = newSearchText;
                FilterComponentTypes(state);
            }

            // 绘制返回按钮
            if (GUI.Button(backButtonRect, "返回", EditorStyles.miniButton))
            {
                state.showSearchMode = false;
                state.searchText = "";
                GUI.FocusControl(null);
            }

            // 显示搜索结果的下拉框
            DrawSearchResultsDropdown(resultRect, fullTypeNameProp, suffixProp, property);
        }

        /// <summary>
        /// 绘制完整下拉列表
        /// </summary>
        private void DrawFullDropdown(Rect rect, SerializedProperty fullTypeNameProp, SerializedProperty suffixProp, SerializedProperty property)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 获取当前选中类型的索引
            int currentIndex = FindMatchingComponentType(fullTypeNameProp.stringValue);
            if (currentIndex < 0 && componentTypes.Count > 0) currentIndex = 0;

            // 获取选项
            GUIContent[] options = GetComponentTypeOptions();

            // 显示下拉框（分页显示）
            int pageSize = 15;
            int page = 1;
            int maxPage = Mathf.CeilToInt((float)componentTypes.Count / pageSize);

            // 显示当前页的选项
            int startIndex = (page - 1) * pageSize;
            int displayCount = Mathf.Min(pageSize, componentTypes.Count - startIndex);

            GUIContent[] currentPageOptions = new GUIContent[displayCount];
            for (int i = 0; i < displayCount; i++)
            {
                currentPageOptions[i] = options[startIndex + i];
            }

            // 下拉框（使用GUIContent[]保留Tooltip）
            Rect dropdownRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            int newIndex = EditorGUI.Popup(dropdownRect, new GUIContent("完整列表"), currentIndex - startIndex, currentPageOptions);

            if (newIndex >= 0 && newIndex < displayCount)
            {
                ComponentTypeInfo selectedType = componentTypes[startIndex + newIndex];
                fullTypeNameProp.stringValue = selectedType.displayName;
                AddToRecentlyUsed(selectedType, state);

                // 自动建议后缀
                if (string.IsNullOrEmpty(suffixProp.stringValue) || commonSuffixes.Contains(suffixProp.stringValue))
                {
                    string suggestedSuffix = GetSuggestedSuffix(selectedType.componentType);
                    if (!string.IsNullOrEmpty(suggestedSuffix))
                    {
                        suffixProp.stringValue = suggestedSuffix;
                    }
                }
                state.showAdvancedDropdown = false; // 选择后关闭
            }

            // 显示当前选中项的完整信息
            if (currentIndex >= 0 && currentIndex < componentTypes.Count)
            {
                ComponentTypeInfo currentType = componentTypes[currentIndex];
                Rect infoRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(infoRect, $"当前选中：{currentType.displayName}", EditorStyles.miniLabel);
            }

            // 显示分页信息
            Rect pageRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2 + 4, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(pageRect, $"显示 {startIndex + 1}-{startIndex + displayCount} / 共 {componentTypes.Count} 个类型 (按命名空间分组)", EditorStyles.miniLabel);
        }

        /// <summary>
        /// 绘制搜索结果下拉框
        /// </summary>
        private void DrawSearchResultsDropdown(Rect rect, SerializedProperty fullTypeNameProp, SerializedProperty suffixProp, SerializedProperty property)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            if (state.filteredTypes == null || state.filteredTypes.Count == 0)
            {
                EditorGUI.LabelField(rect, "未找到匹配的类型，试试其他关键词", EditorStyles.miniLabel);
                return;
            }

            // 获取当前选中类型在过滤列表中的索引
            int currentIndex = state.filteredTypes.FindIndex(t => t.displayName == fullTypeNameProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            // 显示最多15个结果
            int displayCount = Mathf.Min(state.filteredTypes.Count, 15);
            GUIContent[] displayOptions = new GUIContent[displayCount];
            for (int i = 0; i < displayCount; i++)
            {
                string typeName = state.filteredTypes[i].componentType;
                string namespaceName = state.filteredTypes[i].namespaceName;
                // 简短的显示文本，包含命名空间前缀
                string shortName = typeName.Length > 25 ? typeName.Substring(0, 23) + ".." : typeName;
                string namespacePrefix = string.IsNullOrEmpty(namespaceName) ? "" : namespaceName.Split('.').Last() + ".";
                string displayText = namespacePrefix + shortName;
                // Tooltip中显示完整信息
                string tooltip = $"{namespaceName}.{typeName}";
                displayOptions[i] = new GUIContent(displayText, tooltip);
            }

            // 下拉框（使用GUIContent[]保留Tooltip）
            Rect dropdownRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            int newIndex = EditorGUI.Popup(dropdownRect, new GUIContent($"搜索结果 ({state.filteredTypes.Count})"), currentIndex, displayOptions);

            if (newIndex >= 0 && newIndex < displayCount && newIndex != currentIndex)
            {
                ComponentTypeInfo selectedType = state.filteredTypes[newIndex];
                fullTypeNameProp.stringValue = selectedType.displayName;
                AddToRecentlyUsed(selectedType, state);

                // 自动建议后缀
                if (string.IsNullOrEmpty(suffixProp.stringValue) || commonSuffixes.Contains(suffixProp.stringValue))
                {
                    string suggestedSuffix = GetSuggestedSuffix(selectedType.componentType);
                    if (!string.IsNullOrEmpty(suggestedSuffix))
                    {
                        suffixProp.stringValue = suggestedSuffix;
                    }
                }

                // 选择后退出搜索模式
                state.showSearchMode = false;
                state.searchText = "";
            }

            // 显示当前选中项的完整信息
            if (currentIndex >= 0 && currentIndex < state.filteredTypes.Count)
            {
                ComponentTypeInfo currentType = state.filteredTypes[currentIndex];
                Rect infoRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(infoRect, $"当前选中：{currentType.displayName}", EditorStyles.miniLabel);
            }

            // 显示搜索结果数量提示
            if (state.filteredTypes.Count > displayCount)
            {
                Rect countRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2 + 4, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(countRect, $"显示 {displayCount} / {state.filteredTypes.Count} 个结果，输入更具体的关键词筛选", EditorStyles.miniLabel);
            }

            // 如果结果很少，显示完整的列表详情
            if (state.filteredTypes.Count <= 8)
            {
                Rect listRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 3 + 6, rect.width, EditorGUIUtility.singleLineHeight * 2);
                DrawDetailedList(listRect, state.filteredTypes, 8);
            }
        }

        /// <summary>
        /// 绘制详细的类型列表（用于显示少量搜索结果）
        /// </summary>
        private void DrawDetailedList(Rect rect, List<ComponentTypeInfo> types, int maxRows)
        {
            float rowHeight = EditorGUIUtility.singleLineHeight;
            float startY = rect.y;

            for (int i = 0; i < Mathf.Min(types.Count, maxRows); i++)
            {
                ComponentTypeInfo type = types[i];
                string typeLabel = $"{type.displayName}";

                // 绘制类型标签（可点击）
                Rect itemRect = new Rect(rect.x, startY + i * rowHeight, rect.width, rowHeight);
                if (GUI.Button(itemRect, typeLabel, EditorStyles.miniLabel))
                {
                    // 可以在这里实现点击选择
                }
            }
        }

        /// <summary>
        /// 过滤组件类型
        /// </summary>
        private void FilterComponentTypes(DrawerState state)
        {
            if (string.IsNullOrEmpty(state.searchText))
            {
                state.filteredTypes = componentTypes;
                return;
            }

            state.filteredTypes = componentTypes.FindAll(t =>
            {
                string searchLower = state.searchText.ToLower();
                return t.componentType.ToLower().Contains(searchLower) ||
                       t.namespaceName.ToLower().Contains(searchLower) ||
                       t.displayName.ToLower().Contains(searchLower);
            });
        }

        /// <summary>
        /// 添加到最近使用列表
        /// </summary>
        private void AddToRecentlyUsed(ComponentTypeInfo typeInfo, DrawerState state)
        {
            // 移除已存在的
            state.recentlyUsed.RemoveAll(t => t.displayName == typeInfo.displayName);
            // 添加到开头
            state.recentlyUsed.Insert(0, typeInfo);
            // 保持最近10个
            if (state.recentlyUsed.Count > 10)
            {
                state.recentlyUsed.RemoveAt(state.recentlyUsed.Count - 1);
            }
        }

        /// <summary>
        /// 绘制组件类型下拉选择（优化版 - 保留兼容）
        /// </summary>
        private void DrawComponentTypeDropdown(Rect rect, SerializedProperty componentTypeProp, SerializedProperty suffixProp)
        {
            // 获取 fullTypeName 属性
            SerializedProperty fullTypeNameProp = componentTypeProp.FindPropertyRelative("fullTypeName");
            if (fullTypeNameProp == null)
            {
                // 回退到旧版本兼容
                EditorGUI.LabelField(rect, "组件类型", "配置错误，请检查");
                return;
            }

            // 获取当前类型的索引
            int currentIndex = FindMatchingComponentType(fullTypeNameProp.stringValue);

            // 如果没找到匹配的类型，使用第一个（默认值）
            if (currentIndex < 0 && componentTypes.Count > 0)
            {
                currentIndex = 0;
            }

            // 绘制带搜索的下拉框
            string currentDisplay = currentIndex >= 0 ? componentTypes[currentIndex].displayName : fullTypeNameProp.stringValue;
            bool hasFocus = GUI.GetNameOfFocusedControl() == "ComponentTypeSearch";

            // 搜索框区域
            Rect searchRect = new Rect(rect.x, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);
            // 下拉框区域
            Rect dropdownRect = new Rect(rect.x + rect.width * 0.4f + 5, rect.y, rect.width * 0.6f - 5, EditorGUIUtility.singleLineHeight);

            // 绘制下拉框
            GUIContent[] options = GetComponentTypeOptions();
            string[] stringOptions = System.Array.ConvertAll(options, x => x.text);
            int newIndex = EditorGUI.Popup(dropdownRect, currentDisplay, currentIndex, stringOptions);

            if (newIndex >= 0 && newIndex < componentTypes.Count && newIndex != currentIndex)
            {
                ComponentTypeInfo selectedType = componentTypes[newIndex];
                fullTypeNameProp.stringValue = selectedType.displayName;

                // 自动建议后缀（如果当前后缀为空或与默认后缀匹配）
                if (string.IsNullOrEmpty(suffixProp.stringValue) || commonSuffixes.Contains(suffixProp.stringValue))
                {
                    string suggestedSuffix = GetSuggestedSuffix(selectedType.componentType);
                    if (!string.IsNullOrEmpty(suggestedSuffix))
                    {
                        suffixProp.stringValue = suggestedSuffix;
                    }
                }
            }
        }

        /// <summary>
        /// 根据组件类型获取建议的后缀
        /// </summary>
        private string GetSuggestedSuffix(string componentName)
        {
            var suffixMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Button", "_btn" },
                { "Text", "_txt" },
                { "TextMeshProUGUI", "_txt" },
                { "Image", "_img" },
                { "RawImage", "_raw" },
                { "Toggle", "_tgl" },
                { "Slider", "_slr" },
                { "InputField", "_inp" },
                { "TMP_InputField", "_inp" },
                { "ScrollRect", "_scr" },
                { "ScrollView", "_svr" },
                { "GridLayoutGroup", "_grid" },
                { "Canvas", "_canvas" },
                { "RectTransform", "_rect" },
                { "CanvasGroup", "_group" },
                { "LayoutElement", "_layout" },
                { "CanvasScaler", "_scaler" },
                { "VerticalLayoutGroup", "_vlg" },
                { "HorizontalLayoutGroup", "_hlg" },
                { "ContentSizeFitter", "_fitter" },
                { "AspectRatioFitter", "_arf" },
                { "Animator", "_anim" },
                { "Animation", "_anim" },
                { "AudioSource", "_audio" },
                { "Camera", "_cam" },
                { "Light", "_light" },
                { "Rigidbody", "_rb" },
                { "Rigidbody2D", "_rb2d" },
                { "Collider", "_col" },
                { "BoxCollider", "_box" },
                { "SphereCollider", "_sphere" },
                { "CapsuleCollider", "_capsule" }
            };

            if (suffixMap.TryGetValue(componentName, out string suffix))
            {
                return suffix;
            }

            // 如果没有预定义的后缀，生成一个简单的后缀（取前3个字母并添加下划线）
            if (componentName.Length >= 3)
            {
                return "_" + componentName.Substring(0, 3).ToLower();
            }

            return "_" + componentName.ToLower();
        }

        /// <summary>
        /// 获取组件类型选项（按命名空间分组）
        /// </summary>
        private GUIContent[] GetComponentTypeOptions()
        {
            List<GUIContent> options = new List<GUIContent>();
            string currentNamespace = "";

            foreach (var componentType in componentTypes)
            {
                // 添加命名空间分隔符
                if (componentType.namespaceName != currentNamespace)
                {
                    if (!string.IsNullOrEmpty(currentNamespace))
                    {
                        options.Add(new GUIContent("/")); // 分隔符
                    }
                    options.Add(new GUIContent(componentType.namespaceName ?? "无命名空间", componentType.namespaceName));
                    currentNamespace = componentType.namespaceName;
                }

                // 添加组件类型（带完整信息作为tooltip）
                string displayName = $"  {componentType.componentType}";
                string tooltip = componentType.displayName;
                options.Add(new GUIContent(displayName, tooltip));
            }

            return options.ToArray();
        }

        /// <summary>
        /// 查找匹配的组件类型（优化版）
        /// </summary>
        private int FindMatchingComponentType(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return -1;

            for (int i = 0; i < componentTypes.Count; i++)
            {
                if (componentTypes[i].displayName == fullTypeName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 计算属性高度
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 获取当前属性的状态
            DrawerState state = GetState(property);

            // 基础高度：后缀名称
            float height = EditorGUIUtility.singleLineHeight + 2;

            // 获取当前选中的类型信息
            SerializedProperty componentTypeProp = property.FindPropertyRelative("componentType");
            SerializedProperty fullTypeNameProp = componentTypeProp?.FindPropertyRelative("fullTypeName");

            if (state.showSearchMode)
            {
                // 搜索模式：搜索框 + 搜索结果
                height += EditorGUIUtility.singleLineHeight * 2; // 搜索框行 + 搜索结果行

                // 如果搜索结果很少，显示详细列表
                if (state.filteredTypes != null && state.filteredTypes.Count <= 8)
                {
                    height += EditorGUIUtility.singleLineHeight * 2; // 详细列表
                }
            }
            else
            {
                // 常用类型模式：常用类型按钮 + 当前选中信息
                height += EditorGUIUtility.singleLineHeight * 2; // 常用类型按钮行 + 选中信息行
            }

            return height;
        }
    }
}
