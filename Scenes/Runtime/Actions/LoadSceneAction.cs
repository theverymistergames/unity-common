using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
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
        [SerializeField] private Optional<AnimationCurve> _fadeOutCurve = Optional<AnimationCurve>.WithDisabled(EasingType.Linear.ToAnimationCurve());
        
        [Header("Finish Fade")]
        [SerializeField] private bool _applyFadeInOnFinish;
        [SerializeField] [Min(-1f)] private float _fadeInOnFinish = -1f;
        [SerializeField] private Optional<AnimationCurve> _fadeInCurve = Optional<AnimationCurve>.WithDisabled(EasingType.Linear.ToAnimationCurve());

        public async UniTask Apply(CancellationToken cancellationToken) {
            if (!CanShowScene()) return;
            
            float showStartTime = Time.realtimeSinceStartup;
            bool isLoadingScene = _scene.scene == LoadingService.Instance.LoadingScene;
            
            if (isLoadingScene) {
                LoadingService.Instance.ShowLoadingScreen(true);
            }

            await SceneLoader.LoadSceneAsync(_scene.scene, _makeActive);
            if (cancellationToken.IsCancellationRequested) return;

            if (_applyFadeOutOnStart) {
                await Fader.Main.FadeOutAsync(_fadeOutOnStart, _fadeOutCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            float wait = Mathf.Max(showStartTime - Time.realtimeSinceStartup, _minDuration);
            
            if (wait > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(wait), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
                if (cancellationToken.IsCancellationRequested) return;
            }

            if (_applyFadeInOnFinish) {
                await Fader.Main.FadeInAsync(_fadeInOnFinish, _fadeInCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            if (!_unloadOnFinish) return;

            if (isLoadingScene) {
                LoadingService.Instance.ShowLoadingScreen(false);
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