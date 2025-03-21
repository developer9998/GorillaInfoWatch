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
        public static AssetBundle Bundle => _bundleLoaded ? _storedBundle : null;
        
        private static bool _bundleLoaded;
        private static AssetBundle _storedBundle;

        private static Task _loadingTask = null;
        private static readonly Dictionary<string, Object> _assetCache = [];

        private static async Task LoadBundle()
        {
            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaInfoWatch.Content.watchbundle");
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            // AssetBundleCreateRequest is a YieldInstruction !!
            await TaskUtils.YieldInstruction(bundleLoadRequest);

            _storedBundle = bundleLoadRequest.assetBundle;
            _bundleLoaded = true;
        }

        public static async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (_assetCache.TryGetValue(name, out var _loadedObject)) return _loadedObject as T;

            if (!_bundleLoaded)
            {
                _loadingTask ??= LoadBundle();
                await _loadingTask;
            }

            var assetLoadRequest = _storedBundle.LoadAssetAsync<T>(name);

            // AssetBundleRequest is a YieldInstruction !!
            await TaskUtils.YieldInstruction(assetLoadRequest);

            var asset = assetLoadRequest.asset as T;
            _assetCache.AddOrUpdate(name, asset);
            return asset;
        }
    }
}
