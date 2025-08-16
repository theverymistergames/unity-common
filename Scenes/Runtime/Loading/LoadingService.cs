using System.Collections.Generic;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    [DefaultExecutionOrder(-99_999)]
    public sealed class LoadingService : MonoBehaviour, ILoadingService {
        
        [Header("Scene Settings")]
        [SerializeField] private SceneReference _loadingScene;
        [SerializeField] private bool _makeSceneActiveOnShow = true;
        
        [Header("Load Settings")]
        [SerializeField] private bool _loadOnAwake;
#if UNITY_EDITOR
        [SerializeField] private bool _loadOnAwakeIfPlaymodeStartScenesOverriden = true;  
#endif
        
        public static ILoadingService Instance { get; private set; }

        public string LoadingScene => _loadingScene.scene;
        
        private ILoadingScreen _loadingScreen;
        private readonly HashSet<int> _overlayBlockSources = new();
        private bool _showLoadingScreen;

        private void Awake() {
            Instance = this;
            
            if (CanLoadOnAwake()) SceneLoader.LoadScene(_loadingScene.scene);
        }

        public void ShowLoadingScreen(bool show) {
            _showLoadingScreen = show;
            _loadingScreen?.SetState(GetState());

            if (show && _loadingScreen == null) {
                SceneLoader.LoadScene(_loadingScene.scene, _makeSceneActiveOnShow);
                return;
            }

            if (show && _makeSceneActiveOnShow && SceneLoader.IsSceneLoaded(_loadingScene.scene)) {
                SceneLoader.SetActiveScene(_loadingScene.scene);
            }
        }

        public void BlockLoadingScreenOverlay(object source, bool block) {
            if (block) _overlayBlockSources.Add(source.GetHashCode());
            else _overlayBlockSources.Remove(source.GetHashCode());
            
            _loadingScreen?.SetState(GetState());
        }

        public void RegisterLoadingScreen(ILoadingScreen loadingScreen) {
            _loadingScreen = loadingScreen;
            _loadingScreen.SetState(GetState());
        }

        public void UnregisterLoadingScreen(ILoadingScreen loadingScreen) {
            _loadingScreen = null;
        }

        private LoadingScreenState GetState() {
            return _showLoadingScreen
                ? _overlayBlockSources.Count == 0 ? LoadingScreenState.Full : LoadingScreenState.Background
                : LoadingScreenState.Off;
        }

        private bool CanLoadOnAwake() {
            bool canLoad = _loadOnAwake;

#if UNITY_EDITOR
            canLoad |= _loadOnAwakeIfPlaymodeStartScenesOverriden;
#endif
            
            return canLoad;
        }
    }
    
}