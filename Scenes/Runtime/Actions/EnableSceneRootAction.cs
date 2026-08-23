using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;
using MisterGames.Scenes.SceneRoots;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class EnableSceneRootAction : ISceneLoaderAction {

        [Header("Scene")]
        [SerializeField] private bool _bypass;
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeField] private SceneReference _scene;
        [SerializeField] private bool _enableSceneRoot = true;

        public UniTask Apply(CancellationToken cancellationToken) {
            if (!CanShowScene()) return default;

            var loadingService = Services.Get<ILoadingService>();
            bool isLoadingScene = _scene.scene == loadingService?.LoadingScene;
            
            if (isLoadingScene) {
                loadingService?.ShowLoadingScreen(_enableSceneRoot);
                return default;
            }
            
            Services.Get<ISceneRootService>().SetSceneRootEnabled(_scene.scene, _enableSceneRoot);
            return default;
        }

        private bool CanShowScene() {
            bool show = !_bypass && _scene.IsValid();
            
#if UNITY_EDITOR
            show &= !_bypassIfPlaymodeStartSceneOverriden || !PlaymodeStartScenesUtils.IsPlaymodeStartScenesOverrideEnabled(out _);
#endif

            return show;
        }
    }
    
}