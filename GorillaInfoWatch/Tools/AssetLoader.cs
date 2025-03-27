using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
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
        private static readonly Dictionary<string, Object> loaded_assets = [];

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
            if (loaded_assets.TryGetValue(name, out var _loadedObject)) return _loadedObject as T;

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
    }
}
