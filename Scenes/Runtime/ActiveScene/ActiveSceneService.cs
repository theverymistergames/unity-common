using System;
using System.Collections.Generic;
using MisterGames.Common.Strings;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.SceneRoots;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.ActiveScene {
    
    public sealed class ActiveSceneService : IDisposable {

        private static readonly string LogPrefix = nameof(ActiveSceneService).FormatColorOnlyForEditor(Color.white);

        private readonly Dictionary<string, float> _loadTimeMap = new();
        
        private ActiveSceneSettings _settings;
        private ISceneRootService _sceneRootService;
        
        public void Initialize(ActiveSceneSettings settings, ISceneRootService sceneRootService) {
            _settings = settings;
            _sceneRootService = sceneRootService;

            _sceneRootService.OnSceneRootsEnableStateChanged += OnSceneRootsEnableStateChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void Dispose() {
            _sceneRootService.OnSceneRootsEnableStateChanged -= OnSceneRootsEnableStateChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneRootsEnableStateChanged(string sceneName, bool enabled) {
            if (enabled && SceneLoader.IsSceneLoaded(sceneName)) {
                _loadTimeMap[sceneName] = Time.realtimeSinceStartup;
            }
            else {
                _loadTimeMap.Remove(sceneName);
            }
            
            UpdateActiveScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1) {
            string sceneName = scene.name;
            
            if (!_sceneRootService.HasSceneRootState(sceneName, out bool enabled) || enabled) {
                _loadTimeMap[sceneName] = Time.realtimeSinceStartup;
            }
            else {
                _loadTimeMap.Remove(sceneName);
            }
            
            UpdateActiveScene();
        }

        private void OnSceneUnloaded(Scene scene) {
            _loadTimeMap.Remove(scene.name);
            
            UpdateActiveScene();
        }

        private void UpdateActiveScene() {
            var loadedScenes = SceneLoader.GetLoadedScenes();
            var loadedScenesSet = HashSetPool<string>.Get();
            
            foreach (string scene in loadedScenes) {
                if (!SceneLoader.IsSceneRequestedToLoad(scene) || 
                    _sceneRootService.HasSceneRootState(scene, out bool enabled) && !enabled)
                {
                    continue;
                }
                
                loadedScenesSet.Add(scene);
            }

            ProcessLoadedScenes(loadedScenesSet);
            
            HashSetPool<string>.Release(loadedScenesSet);
        }

        private void ProcessLoadedScenes(HashSet<string> loadedScenesSet) {
            if (TryGetCustomActiveScene(loadedScenesSet, out string activeScene)) {
                if (SceneLoader.GetRequestedActiveScene() != activeScene) {
                    LogInfo($"found custom active scene {activeScene.FormatColorOnlyForEditor(Color.green)}, setting up...");
                    SceneLoader.SetActiveScene(activeScene);
                }
                return;
            }
            
            ExcludeScenesFromSet(_settings.neverSetActiveOnLoad, loadedScenesSet);
            
            for (int i = 0; i < _settings.setActiveOnLoadHighPriority?.Length; i++) {
                ref var sceneReference = ref _settings.setActiveOnLoadHighPriority[i];
                if (!loadedScenesSet.Contains(sceneReference.scene)) continue;
                
                if (SceneLoader.GetRequestedActiveScene() != sceneReference.scene) {
                    LogInfo($"found high priority active scene {sceneReference.scene.FormatColorOnlyForEditor(Color.green)}, setting up...");
                    SceneLoader.SetActiveScene(sceneReference.scene);
                }
                return;
            }

            string topLoadedSceneFromLowPriority = null;
            for (int i = 0; i < _settings.setActiveOnLoadLowPriority?.Length; i++) {
                ref var sceneReference = ref _settings.setActiveOnLoadLowPriority[i];
                if (!loadedScenesSet.Remove(sceneReference.scene) || topLoadedSceneFromLowPriority != null) continue;

                topLoadedSceneFromLowPriority = sceneReference.scene;
            }

            if (TryGetLastLoadedScene(loadedScenesSet, out string lastLoadedScene)) {
                switch (_settings.defaultPriorityScenesMode) {
                    case ActiveSceneSettings.Mode.DoNothing:
                        break;

                    case ActiveSceneSettings.Mode.SetLastLoadedAsActive:
                        if (SceneLoader.GetRequestedActiveScene() != lastLoadedScene) {
                            LogInfo($"using last loaded scene {lastLoadedScene.FormatColorOnlyForEditor(Color.green)} as active, setting up...");
                            SceneLoader.SetActiveScene(lastLoadedScene);
                        }
                        return;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            if (topLoadedSceneFromLowPriority == null) return;
            
            if (SceneLoader.GetRequestedActiveScene() != topLoadedSceneFromLowPriority) {
                LogInfo($"found low priority active scene {topLoadedSceneFromLowPriority.FormatColorOnlyForEditor(Color.green)}, setting up...");
                SceneLoader.SetActiveScene(topLoadedSceneFromLowPriority);
            } 
        }

        private bool TryGetCustomActiveScene(HashSet<string> loadedScenes, out string activeScene) {
            int topPriority = int.MinValue;
            activeScene = null;
            
            for (int i = 0; i < _settings.customActiveScenes?.Length; i++) {
                ref var data = ref _settings.customActiveScenes[i];
                if (!HasScenesInSet(data.forTheseScenes, loadedScenes) || data.priority < topPriority) continue;
                
                topPriority = data.priority;
                activeScene = data.setActiveScene.scene;
            }
            
            return activeScene != null;
        }

        private bool TryGetLastLoadedScene(HashSet<string> loadedScenes, out string lastScene) {
            float lastTime = -1f;
            lastScene = null;
            
            foreach ((string sceneName, float time) in _loadTimeMap) {
                if (time < lastTime || !loadedScenes.Contains(sceneName)) continue;
                
                lastTime = time;
                lastScene = sceneName;
            }
            
            return lastScene != null;
        }
        
        private static void ExcludeScenesFromSet(SceneReference[] sceneReferences, HashSet<string> scenesSet) {
            for (int i = 0; i < sceneReferences?.Length; i++) {
                ref var sceneReference = ref sceneReferences[i];
                scenesSet.Remove(sceneReference.scene);
            }
        }

        private static bool HasScenesInSet(SceneReference[] sceneReferences, HashSet<string> scenesSet) {
            for (int i = 0; i < sceneReferences?.Length; i++) {
                ref var sceneReference = ref sceneReferences[i];
                if (scenesSet.Contains(sceneReference.scene)) return true;
            }
            return false;
        }
        
        private static void LogInfo(string message) {
            Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }
    
}