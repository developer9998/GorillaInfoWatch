using GorillaInfoWatch.Extensions;
using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaInfoWatch.Tools
{
    internal class AssetLoader(string bundleResourceName)
    {
        public AssetBundle AssetBundle => _bundleLoaded ? _bundle : null;

        private readonly string _bundleResourceName = bundleResourceName;

        private bool _bundleLoaded;

        private AssetBundle _bundle;

        private Task _bundleLoadTask = null;

        private readonly Dictionary<string, object> _loadedAssets = [];

        private async Task LoadBundle()
        {
            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream(_bundleResourceName);
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            // AssetBundleCreateRequest is a YieldInstruction !!
            await bundleLoadRequest.AsAwaitable();

            _bundle = bundleLoadRequest.assetBundle;
            _bundleLoaded = true;
        }

        public async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (_loadedAssets.ContainsKey(name) && _loadedAssets[name] is Object _loadedObject) return _loadedObject as T;

            if (!_bundleLoaded)
            {
                _bundleLoadTask ??= LoadBundle();
                await _bundleLoadTask;
            }

            var assetLoadRequest = _bundle.LoadAssetAsync<T>(name);

            // AssetBundleRequest is a YieldInstruction !!
            await assetLoadRequest.AsAwaitable();

            var asset = assetLoadRequest.asset as T;
            _loadedAssets.AddOrUpdate(name, asset);
            return asset;
        }

        public async Task<T[]> LoadAssetsWithSubAssets<T>(string name) where T : Object
        {
            if (_loadedAssets.ContainsKey(name) && _loadedAssets[name] is T[] cachedArray) return cachedArray;

            if (!_bundleLoaded)
            {
                _bundleLoadTask ??= LoadBundle();
                await _bundleLoadTask;
            }

            var assetLoadRequest = _bundle.LoadAssetWithSubAssetsAsync<T>(name);
            await assetLoadRequest.AsAwaitable();

            var assets = assetLoadRequest.allAssets.Cast<T>().ToArray();
            _loadedAssets.AddOrUpdate(name, assets);

            return assets;
        }
    }
}
