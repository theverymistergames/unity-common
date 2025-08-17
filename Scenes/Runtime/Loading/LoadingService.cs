using System;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.SceneRoots;
using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    [Serializable]
    public sealed class LoadingService : ILoadingService {

        [Header("Scene Settings")]
        [SerializeField] private SceneReference _loadingScene;
        [SerializeField] private bool _makeSceneActiveOnShow = true;
        
        [Header("Load Settings")]
        [SerializeField] private bool _loadOnStart;
#if UNITY_EDITOR
        [SerializeField] private bool _loadOnStartIfPlaymodeStartScenesOverriden = true;  
#endif
        [SerializeField] private bool _canUnloadLoadingScene;
        
        public string LoadingScene => _loadingScene.scene;
        
        private bool _showLoadingScreen;

        public void Initialize() {
            Services.Get<ISceneRootService>().SetSceneRootEnabled(_loadingScene.scene, false);
            
            if (CanLoadOnInitialize()) SceneLoader.LoadScene(_loadingScene.scene);
        }

        public void ShowLoadingScreen(bool show) {
            _showLoadingScreen = show;
            Services.Get<ISceneRootService>().SetSceneRootEnabled(_loadingScene.scene, GetState());

            if (!show) {
                if (_canUnloadLoadingScene) SceneLoader.UnloadScene(_loadingScene.scene);
                return;
            }

            if (!SceneLoader.IsSceneLoaded(_loadingScene.scene)) {
                SceneLoader.LoadScene(_loadingScene.scene, _makeSceneActiveOnShow);
                return;
            }

            if (_makeSceneActiveOnShow) {
                SceneLoader.SetActiveScene(_loadingScene.scene);
            }
        }
        
        private bool GetState() {
            return _showLoadingScreen;
        }
        
        private bool CanLoadOnInitialize() {
            bool canLoad = _loadOnStart;

#if UNITY_EDITOR
            canLoad |= _loadOnStartIfPlaymodeStartScenesOverriden;
#endif
            
            return canLoad;
        }
    }
    
}