using System;
using System.Globalization;
using System.IO;
using UnityEditor;

namespace vietlabs.fr2
{
    /// <summary>
    /// Extension methods for asset path processing operations
    /// </summary>
    internal static class AssetPathExtensions
    {
        /// <summary>
        /// Split an asset path into its components
        /// </summary>
        public static void SplitPath(this string assetPath, out string assetName, out string assetExtension, out string assetFolder)
        {
            assetName = string.Empty;
            assetFolder = string.Empty;
            assetExtension = string.Empty;

            if (string.IsNullOrEmpty(assetPath)) return;

            assetExtension = Path.GetExtension(assetPath);
            assetName = Path.GetFileNameWithoutExtension(assetPath);
            int lastSlash = assetPath.LastIndexOf("/", StringComparison.Ordinal) + 1;
            assetFolder = assetPath.Substring(0, lastSlash);
        }
        
        
        /// <summary>
        /// Check if asset is in Editor folder
        /// </summary>
        public static bool IsInEditor(this string assetPath)
        {
            return assetPath.Contains("/Editor/") || assetPath.Contains("/Editor Default Resources/");
        }
        
        /// <summary>
        /// Check if asset is in Resources folder
        /// </summary>
        public static bool IsInResources(this string assetPath)
        {
            return assetPath.Contains("/Resources/");
        }
        
        /// <summary>
        /// Check if asset is in StreamingAssets folder
        /// </summary>
        public static bool IsInStreamingAssets(this string assetPath)
        {
            return assetPath.Contains("/StreamingAssets/");
        }
        
        /// <summary>
        /// Check if asset is in Plugins folder
        /// </summary>
        public static bool IsInPlugins(this string assetPath)
        {
            return assetPath.Contains("/Plugins/");
        }
        
        /// <summary>
        /// Check if asset is in Packages folder
        /// </summary>
        public static bool IsInPackages(this string assetPath)
        {
            return assetPath.StartsWith("Packages/");
        }
        
        /// <summary>
        /// Check if string starts with any of the provided prefixes
        /// </summary>
        public static bool StartsWithAny(this string source, params string[] prefixes)
        {
            if (string.IsNullOrEmpty(source)) return false;
            for (var i = 0; i < prefixes.Length; i++)
            {
                if (source.StartsWith(prefixes[i])) return true;
            }

            return false;
        }
        
        /// <summary>
        /// Normalize asset folder path by removing "Assets/" prefix if present
        /// </summary>
        public static string NormalizeAssetFolder(this string assetFolder, string assetPath)
        {
            if (assetFolder.StartsWith("Assets/"))
            {
                return assetFolder.Substring(7);
            }
            
            if (!assetPath.StartsWithAny("Project Settings/", "Library/"))
            {
                return "built-in/";
            }
            
            return assetFolder;
        }
        
        /// <summary>
        /// Get comprehensive path information for an asset
        /// </summary>
        public static AssetPathInfo GetPathInfo(this string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return new AssetPathInfo();
            }
            
            assetPath.SplitPath(out string assetName, out string extension, out string assetFolder);
            
            return new AssetPathInfo
            {
                AssetPath = assetPath,
                AssetName = assetName,
                Extension = extension,
                AssetFolder = assetFolder.NormalizeAssetFolder(assetPath),
                InEditor = assetPath.IsInEditor(),
                InResources = assetPath.IsInResources(),
                InStreamingAssets = assetPath.IsInStreamingAssets(),
                InPlugins = assetPath.IsInPlugins(),
                InPackages = assetPath.IsInPackages()
            };
        }
    }
    
    /// <summary>
    /// Container for asset path information
    /// </summary>
    internal struct AssetPathInfo
    {
        public string AssetPath;
        public string AssetName;
        public string Extension;
        public string AssetFolder;
        public bool InEditor;
        public bool InResources;
        public bool InStreamingAssets;
        public bool InPlugins;
        public bool InPackages;
    }
}
