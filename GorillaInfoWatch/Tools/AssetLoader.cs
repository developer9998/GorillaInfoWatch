using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaInfoWatch.Tools
{
    public class AssetLoader
    {
        private bool _bundleLoaded;
        private AssetBundle _storedBundle;

        private Task _loadingTask = null;
        private readonly Dictionary<string, Object> _assetCache = [];

        private async Task LoadBundle()
        {
            var taskCompletionSource = new TaskCompletionSource<AssetBundle>();

            Stream str = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaInfoWatch.Content.watchbundle");
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(str);

            request.completed += operation =>
            {
                AssetBundleCreateRequest outRequest = operation as AssetBundleCreateRequest;
                taskCompletionSource.SetResult(outRequest.assetBundle);
            };

            _storedBundle = await taskCompletionSource.Task;
            _bundleLoaded = true;

            Logging.Info("Loaded asset bundle");
        }

        public async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (_assetCache.TryGetValue(name, out var _loadedObject)) return _loadedObject as T;

            if (!_bundleLoaded)
            {
                _loadingTask ??= LoadBundle();
                await _loadingTask;
            }

            var taskCompletionSource = new TaskCompletionSource<T>();
            AssetBundleRequest request = _storedBundle.LoadAssetAsync<T>(name);

            request.completed += operation =>
            {
                AssetBundleRequest outRequest = operation as AssetBundleRequest;
                if (outRequest.asset == null)
                {
                    taskCompletionSource.SetResult(null);
                    return;
                }

                taskCompletionSource.SetResult(outRequest.asset as T);
            };

            var _finishedTask = await taskCompletionSource.Task;
            _assetCache.Add(name, _finishedTask);

            Logging.Info(string.Concat("Loaded ", name, " of type ", typeof(T).Name));
            return _finishedTask;
        }
    }
}
