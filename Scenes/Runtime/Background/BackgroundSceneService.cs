using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.SceneRoots;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.Scenes.Background {
    
    [Serializable]
    public sealed class BackgroundSceneService : ISceneLoadHook, IDisposable {

        [Header("Scene Loading Settings")]
        [SerializeField] private UnloadMode _defaultUnloadMode = UnloadMode.UnloadOnHide;
        [SerializeField] private SceneReference[] _preloadScenesOnAwake;
        [SerializeField] private OverrideMode[] _loadModeOverrides;
        
        [Header("Background Scenes")]
        [SerializeField] private BackgroundSceneData[] _backgroundScenesMap;
        
        [Serializable]
        private struct OverrideMode {
            public UnloadMode unloadMode;
            public SceneReference[] scenes;
        }

        [Serializable]
        private struct BackgroundSceneData {
            public bool activateFirstBackgroundScene;
            public SceneReference[] backgroundScenes;
            public SceneReference[] forScenes;
        }
        
        private enum UnloadMode {
            UnloadOnHide,
            HideRootGameObjects,
        }

        private readonly MultiValueDictionary<int, int> _sceneHashToBackgroundScenesIndexMap = new();
        private readonly Dictionary<int, UnloadMode> _unloadModeMap = new();
        private readonly Dictionary<int, byte> _processIdMap = new();

        private CancellationTokenSource _cts;
        private ISceneRootService _sceneRootService;
        
        public void Initialize() {
            AsyncExt.RecreateCts(ref _cts);
            
            _sceneRootService = Services.Get<ISceneRootService>();
            
            FetchBackgroundScenesMap();
            FetchUnloadModeMap();

            SceneLoader.AddSceneLoadHook(this);
            _sceneRootService.OnSceneRootsEnableStateChanged += SceneRootsEnableStateChanged;
            
            for (int i = 0; i < _preloadScenesOnAwake.Length; i++) {
                ref var sceneReference = ref _preloadScenesOnAwake[i];
                SceneLoader.LoadSceneAsync(sceneReference.scene).Forget();
            }
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            SceneLoader.RemoveSceneLoadHook(this);
            _sceneRootService.OnSceneRootsEnableStateChanged -= SceneRootsEnableStateChanged;
        }

        private void SceneRootsEnableStateChanged(string sceneName, bool enabled) {
            if (enabled) {
                OnSceneLoadRequest(sceneName, _cts.Token).Forget();
                return;
            }
            
            OnSceneUnloadRequest(sceneName, _cts.Token).Forget();
        }

        public async UniTask OnSceneLoadRequest(string sceneName, CancellationToken cancellationToken) {
            int hash = sceneName.GetHashCode();
            
            int backgroundIndexCount = _sceneHashToBackgroundScenesIndexMap.GetCount(hash);
            if (backgroundIndexCount <= 0) return;

            GetNextProcessId(hash);
            ProcessLoadRequest(hash, backgroundIndexCount, out var scenesToLoad, out string activeScene);

            if (scenesToLoad == null) {
                if (activeScene != null) await SceneLoader.LoadSceneAsync(activeScene, makeActive: true);
                return;
            }

            await SceneLoader.LoadScenesAsync(scenesToLoad, activeScene);
            
            ListPool<string>.Release(scenesToLoad);
        }

        public async UniTask OnSceneUnloadRequest(string sceneName, CancellationToken cancellationToken) {
            int hash = sceneName.GetHashCode();
            
            int backgroundIndexCount = _sceneHashToBackgroundScenesIndexMap.GetCount(hash);
            if (backgroundIndexCount <= 0) return;
            
            byte id = GetNextProcessId(hash);
            
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            if (id != GetCurrentProcessId(hash) || cancellationToken.IsCancellationRequested) return;

            ProcessUnloadRequest(hash, backgroundIndexCount);
        }

        private void ProcessLoadRequest(int hash, int count, out List<string> scenesToLoad, out string activeScene) {
            scenesToLoad = null;
            activeScene = null;
            
            for (int i = 0; i < count; i++) {
                int index = _sceneHashToBackgroundScenesIndexMap.GetValue(hash, i);
                ref var data = ref _backgroundScenesMap[index];

                for (int j = 0; j < data.backgroundScenes.Length; j++) {
                    ref string backgroundScene = ref data.backgroundScenes[j].scene;
                    _sceneRootService.SetSceneRootEnabled(backgroundScene, true);
                    
                    if (SceneLoader.IsSceneRequestedToLoad(backgroundScene)) continue;

                    scenesToLoad ??= ListPool<string>.Get();
                    scenesToLoad.Add(backgroundScene);
                }

                if (data.activateFirstBackgroundScene && data.backgroundScenes.Length > 0) {
                    activeScene = data.backgroundScenes[0].scene;
                }
            }
        }

        private void ProcessUnloadRequest(int hash, int count) {
            HashSet<string> scenesToKeep = null;
            HashSet<string> scenesToUnload = null;

            for (int i = 0; i < count; i++) {
                int index = _sceneHashToBackgroundScenesIndexMap.GetValue(hash, i);
                ref var data = ref _backgroundScenesMap[index];
                
                bool canUnloadBackgroundScenes = true;
                
                for (int j = 0; j < data.forScenes.Length; j++) {
                    ref string scene = ref data.forScenes[j].scene;
                    
                    if (_sceneRootService.HasSceneRootState(scene, out bool enabled) ? enabled : SceneLoader.IsSceneRequestedToLoad(scene)) 
                    {
                        canUnloadBackgroundScenes = false;
                        break;
                    }
                }
                
                ref var set = ref canUnloadBackgroundScenes ? ref scenesToUnload : ref scenesToKeep;
                
                set ??= HashSetPool<string>.Get();

                for (int j = 0; j < data.backgroundScenes.Length; j++) {
                    ref string backgroundScene = ref data.backgroundScenes[j].scene;
                    set.Add(backgroundScene);
                }
            }

            if (scenesToUnload == null) {
                if (scenesToKeep != null) HashSetPool<string>.Release(scenesToKeep);
                return;
            }
            
            foreach (string scene in scenesToUnload) {
                if (scenesToKeep?.Contains(scene) ?? false) continue;

                _sceneRootService.SetSceneRootEnabled(scene, false);
                
                if (GetUnloadMode(scene) == UnloadMode.UnloadOnHide) {
                    SceneLoader.UnloadScene(scene);
                }
            }
            
            HashSetPool<string>.Release(scenesToUnload);
            if (scenesToKeep != null) HashSetPool<string>.Release(scenesToKeep);
        }
        
        private UnloadMode GetUnloadMode(string sceneName) {
            return _unloadModeMap.GetValueOrDefault(sceneName.GetHashCode(), _defaultUnloadMode);
        }

        private byte GetCurrentProcessId(int hash) {
            return _processIdMap.GetValueOrDefault(hash, (byte) 1);
        }

        private byte GetNextProcessId(int hash) {
            if (!_processIdMap.TryGetValue(hash, out byte id)) id = 0;

            unchecked {
                id++;
            }
            
            _processIdMap[hash] = id;
            return id;
        }

        private void RemoveProcessId(int hash) {
            _processIdMap.Remove(hash);
        }
        
        private void FetchBackgroundScenesMap() {
            _sceneHashToBackgroundScenesIndexMap.Clear();

            for (int i = 0; i < _backgroundScenesMap.Length; i++) {
                ref var data = ref _backgroundScenesMap[i];
                
                for (int j = 0; j < data.forScenes.Length; j++) {
                    ref string scene = ref data.forScenes[j].scene;
                    _sceneHashToBackgroundScenesIndexMap.AddValue(scene.GetHashCode(), i);
                }
            }
        }

        private void FetchUnloadModeMap() {
            _unloadModeMap.Clear();

            for (int i = 0; i < _loadModeOverrides.Length; i++) {
                ref var loadModeOverride = ref _loadModeOverrides[i];
                
                for (int j = 0; j < loadModeOverride.scenes.Length; j++) {
                    ref var sceneReference = ref loadModeOverride.scenes[j];
                    _unloadModeMap[sceneReference.scene.GetHashCode()] = loadModeOverride.unloadMode;
                }
            }
        }
        
#if UNITY_EDITOR
        public void OnValidate() {
            FetchBackgroundScenesMap();
            FetchUnloadModeMap();
        }  
#endif
    }
    
}