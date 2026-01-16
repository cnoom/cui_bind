using UnityEngine;
using UnityEditor;
using System.IO;

namespace CUiAutoBind
{
    /// <summary>
    /// 配置管理器，提供统一的配置加载和创建方法
    /// </summary>
    public static class ConfigManager
    {
        private const string CONFIG_ASSET_PATH = "Assets/CUIBind/Resources/AutoBindConfig.asset";
        private const string CONFIG_FOLDER_PATH = "Assets/CUIBind/Resources";

        /// <summary>
        /// 加载配置（不创建）
        /// </summary>
        public static BindConfig LoadConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:BindConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BindConfig>(path);
            }

            return null;
        }

        /// <summary>
        /// 加载或创建配置
        /// </summary>
        public static BindConfig LoadOrCreateConfig()
        {
            BindConfig config = LoadConfig();
            if (config != null)
                return config;

            // 没有找到配置，创建新配置
            return CreateDefaultConfig();
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static BindConfig CreateDefaultConfig()
        {
            // 确保目录存在
            if (!Directory.Exists(CONFIG_FOLDER_PATH))
            {
                Directory.CreateDirectory(CONFIG_FOLDER_PATH);
                AssetDatabase.Refresh();
            }

            // 创建配置文件
            BindConfig newConfig = ScriptableObject.CreateInstance<BindConfig>();
            AssetDatabase.CreateAsset(newConfig, CONFIG_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newConfig;
        }

        /// <summary>
        /// 重新生成配置
        /// </summary>
        public static BindConfig RegenerateConfig()
        {
            // 删除旧配置
            if (File.Exists(CONFIG_ASSET_PATH))
            {
                AssetDatabase.DeleteAsset(CONFIG_ASSET_PATH);
            }

            return CreateDefaultConfig();
        }
    }
}
