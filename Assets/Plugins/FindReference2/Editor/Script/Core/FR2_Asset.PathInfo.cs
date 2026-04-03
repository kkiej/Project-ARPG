using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    internal partial class FR2_Asset
    {
        // ----------------------- PATH INFO ------------------------

        [NonSerialized] private string m_assetFolder;
        [NonSerialized] private string m_assetName;
        [NonSerialized] private string m_assetPath;
        [NonSerialized] private string m_extension;
        [NonSerialized] private bool m_inEditor;
        [NonSerialized] private bool m_inPackage;
        [NonSerialized] private bool m_inPlugins;
        [NonSerialized] private bool m_inResources;
        [NonSerialized] private bool m_inStreamingAsset;
        [NonSerialized] private bool m_pathLoaded;

        public string assetName => LoadPathInfo().m_assetName;
        public string assetPath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_assetPath)) return m_assetPath;
                m_assetPath = FR2_Cache.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(m_assetPath)) state = AssetState.MISSING;
                return m_assetPath;
            }
        }

        public string parentFolderPath => LoadPathInfo().m_assetFolder;
        public string assetFolder => LoadPathInfo().m_assetFolder;
        public string extension => LoadPathInfo().m_extension;
        public bool inEditor => LoadPathInfo().m_inEditor;
        public bool inPlugins => LoadPathInfo().m_inPlugins;
        public bool inPackages => LoadPathInfo().m_inPackage;
        public bool inResources => LoadPathInfo().m_inResources;
        public bool inStreamingAsset => LoadPathInfo().m_inStreamingAsset;

        internal bool IsExcluded
        {
            get
            {
                if (excludeTS >= ignoreTS) return _isExcluded;

                excludeTS = ignoreTS;
                _isExcluded = false;

                var h = FR2_Setting.IgnoreAsset;
                foreach (string item in h)
                {
                    if (!m_assetPath.StartsWith(item, false, CultureInfo.InvariantCulture)) continue;
                    _isExcluded = true;
                    return true;
                }

                return false;
            }
        }

        public FR2_Asset LoadPathInfo()
        {
            if (m_pathLoaded) return this;
            
            // Invalidate draw cache when path info changes
            InvalidateDrawCache();
            m_pathLoaded = true;

            m_assetPath = FR2_Cache.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                state = AssetState.MISSING;
                return this;
            }

// #if FR2_DEBUG
// 			FR2_LOG.Log("LoadPathInfo ... " + fileInfoHash + ":" + AssetDatabase.GUIDToAssetPath(guid));
// #endif
            var pathInfo = m_assetPath.GetPathInfo();
            m_assetName = pathInfo.AssetName;
            m_extension = pathInfo.Extension;
            m_assetFolder = pathInfo.AssetFolder;
            m_inEditor = pathInfo.InEditor;
            m_inResources = pathInfo.InResources;
            m_inStreamingAsset = pathInfo.InStreamingAssets;
            m_inPlugins = pathInfo.InPlugins;
            m_inPackage = pathInfo.InPackages;
            return this;
        }
    }
} 