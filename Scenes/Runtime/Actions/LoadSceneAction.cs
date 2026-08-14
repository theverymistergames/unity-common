using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Loading;
using MisterGames.Scenes.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Actions {
    
    [Serializable]
    public sealed class LoadSceneAction : ISceneLoaderAction {

        [Header("Scene")]
        [SerializeField] private bool _bypass;
#if UNITY_EDITOR
        [SerializeField] private bool _bypassIfPlaymodeStartSceneOverriden = true;  
#endif
        [SerializeField] private SceneReference _scene;
        [SerializeField] private bool _makeActive = true;
        [SerializeField] [Min(0f)] private float _minDuration = 1f;
        [SerializeField] private bool _unloadOnFinish = true;
        
        [Header("Start Fade")]
        [SerializeField] private bool _applyFadeOutOnStart;
        [SerializeField] [Min(-1f)] private float _fadeOutOnStart = -1f;
        
        [Header("Finish Fade")]
        [SerializeField] private bool _applyFadeInOnFinish;
        [SerializeField] [Min(-1f)] private float _fadeInOnFinish = -1f;

        public async UniTask Apply(CancellationToken cancellationToken) {
            if (!CanShowScene()) return;

            var loadingService = Services.Get<ILoadingService>();
            
            float showStartTime = TimeSources.unscaledTime;
            bool isLoadingScene = _scene.scene == loadingService?.LoadingScene;
            
            if (isLoadingScene) {
                loadingService?.ShowLoadingScreen(true);
            }

            await SceneLoader.LoadSceneAsync(_scene.scene, _makeActive);
            if (cancellationToken.IsCancellationRequested) return;

            if (_applyFadeOutOnStart) {
                await Fader.Main.FadeOutAsync(_fadeOutOnStart);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            float wait = Mathf.Max(TimeSources.unscaledTime - showStartTime, _minDuration);
            
            if (wait > 0f) {
                await AsyncExt.Delay(TimeSpan.FromSeconds(wait), ignoreTimeScale: true, cancellationToken: cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }

            if (_applyFadeInOnFinish) {
                await Fader.Main.FadeInAsync(_fadeInOnFinish);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            if (!_unloadOnFinish) return;

            if (isLoadingScene) {
                loadingService?.ShowLoadingScreen(false);
                return;
            }

            SceneLoader.UnloadSceneAsync(_scene.scene).Forget();
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