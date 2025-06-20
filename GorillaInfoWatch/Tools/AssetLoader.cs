using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaInfoWatch.Tools
{
    internal static class AssetLoader
    {
        public static AssetBundle Bundle => is_bundle_loaded ? asset_bundle : null;

        private static bool is_bundle_loaded;
        private static AssetBundle asset_bundle;
        private static Task bundle_load_task = null;

        private static readonly Dictionary<string, object> loaded_assets = [];

        private static async Task LoadBundle()
        {
            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaInfoWatch.Content.watchbundle");
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            // AssetBundleCreateRequest is a YieldInstruction !!
            await TaskUtils.YieldInstruction(bundleLoadRequest);

            asset_bundle = bundleLoadRequest.assetBundle;
            is_bundle_loaded = true;
        }

        public static async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (loaded_assets.ContainsKey(name) && loaded_assets[name] is Object _loadedObject) return _loadedObject as T;

            if (!is_bundle_loaded)
            {
                bundle_load_task ??= LoadBundle();
                await bundle_load_task;
            }

            var assetLoadRequest = asset_bundle.LoadAssetAsync<T>(name);

            // AssetBundleRequest is a YieldInstruction !!
            await TaskUtils.YieldInstruction(assetLoadRequest);

            var asset = assetLoadRequest.asset as T;
            loaded_assets.AddOrUpdate(name, asset);
            return asset;
        }

        public static async Task<T[]> LoadAssetsWithSubAssets<T>(string name) where T : Object
        {
            if (loaded_assets.ContainsKey(name) && loaded_assets[name] is T[] cachedArray) return cachedArray;

            if (!is_bundle_loaded)
            {
                bundle_load_task ??= LoadBundle();
                await bundle_load_task;
            }

            var assetLoadRequest = asset_bundle.LoadAssetWithSubAssetsAsync<T>(name);
            await TaskUtils.YieldInstruction(assetLoadRequest);

            var assets = assetLoadRequest.allAssets.Cast<T>().ToArray();
            loaded_assets.AddOrUpdate(name, assets);

            return assets;
        }
    }
}
