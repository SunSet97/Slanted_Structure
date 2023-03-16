using System.Collections.Generic;
using UnityEngine;

namespace Utility.Core
{
    public static class AssetBundleMap
    {
        private static readonly Dictionary<string, AssetBundle> AssetBundleDB = new Dictionary<string, AssetBundle>();

        public static void AddAssetBundle(string key, string filePath)
        {
            if (AssetBundleDB.ContainsKey(key))
            {
                return;
            }
            AssetBundleDB.Add(key, AssetBundle.LoadFromFile(filePath));
        }

        public static void RemoveAssetBundle(string key)
        {
            if (!AssetBundleDB.ContainsKey(key))
            {
                return;
            }
            AssetBundleDB[key].Unload(true);
            AssetBundleDB.Remove(key);
        }

        public static AssetBundle GetAssetBundle(string key)
        {
            return AssetBundleDB[key];
        }
    }
}