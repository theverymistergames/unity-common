using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.SceneRoots {
    
    public sealed class SceneRootService : ISceneRootService, ISceneLoadHook, IDisposable {
        
        public event Action<string, bool> OnSceneRootsEnableStateChanged = delegate { };

        private readonly MultiValueDictionary<int, ISceneRoot> _sceneHashToRootsMap = new();
        private readonly Dictionary<ISceneRoot, int> _rootToSceneHashMap = new();
        private readonly Dictionary<int, bool> _sceneHashToEnableStateMap = new();

        public void Initialize() {
            SceneLoader.AddSceneLoadHook(this);
        }

        public void Dispose() {
            SceneLoader.RemoveSceneLoadHook(this);
        }

        public UniTask OnSceneLoadRequest(string sceneName, CancellationToken cancellationToken) {
            _sceneHashToEnableStateMap.TryAdd(sceneName.GetHashCode(), true);
            return default;
        }

        public UniTask OnSceneUnloadRequest(string sceneName, CancellationToken cancellationToken) {
            _sceneHashToEnableStateMap.Remove(sceneName.GetHashCode());
            return default;
        }

        public void Register(ISceneRoot sceneRoot, string sceneName) {
            int hash = sceneName.GetHashCode();
            
            _sceneHashToRootsMap.AddValue(hash, sceneRoot);
            _rootToSceneHashMap[sceneRoot] = hash;

            bool isSceneRootEnabled = _sceneHashToEnableStateMap.GetValueOrDefault(hash, false);
            
            SetSceneRootsEnableState(hash, isSceneRootEnabled);
            OnSceneRootsEnableStateChanged.Invoke(sceneName, isSceneRootEnabled);
        }

        public void Unregister(ISceneRoot sceneRoot) {
            if (!_rootToSceneHashMap.Remove(sceneRoot, out int hash)) return;
            
            _sceneHashToRootsMap.RemoveValue(hash, sceneRoot);
        }

        public bool HasSceneRootState(string sceneName, out bool enabled) {
            return _sceneHashToEnableStateMap.TryGetValue(sceneName.GetHashCode(), out enabled); 
        }

        public void SetSceneRootEnabled(string sceneName, bool enabled) {
            int hash = sceneName.GetHashCode();
            
            _sceneHashToEnableStateMap[hash] = enabled;
            SetSceneRootsEnableState(hash, enabled);
            
            OnSceneRootsEnableStateChanged.Invoke(sceneName, enabled);
        }

        private void SetSceneRootsEnableState(int sceneHash, bool enabled) {
            int count = _sceneHashToRootsMap.GetCount(sceneHash);
            
            for (int i = 0; i < count; i++) {
                _sceneHashToRootsMap.GetValue(sceneHash, i).SetEnabled(enabled);
            }
        }
    }
    
}