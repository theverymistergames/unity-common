using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Lists;
using MisterGames.Scenes.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        [Header("Scenes")]
        [SerializeField] private SceneReference _splashScreenScene;
        [SerializeField] private SceneReference _startScene;
        [SerializeField] private SceneReference _gameplayScene;
        [SerializeField] private SceneReference _loadingScene;

        [Header("Fader")]
        [SerializeField] [Min(-1f)] private float _fadeIn = -1f;
        [SerializeField] [Min(-1f)] private float _fadeOut = -1f;
        [SerializeField] private Optional<AnimationCurve> _fadeInCurve = Optional<AnimationCurve>.WithDisabled(EasingType.Linear.ToAnimationCurve());
        [SerializeField] private Optional<AnimationCurve> _fadeOutCurve = Optional<AnimationCurve>.WithDisabled(EasingType.Linear.ToAnimationCurve());
        
        public static ApplicationLaunchMode LaunchMode => _instance._applicationLaunchMode;
        
        private static SceneLoader _instance;
        
        private ApplicationLaunchMode _applicationLaunchMode;
        private static string _rootScene;
        
        private void Awake() {
            _instance = this;
            
            LoadStartScenes(destroyCancellationToken).Forget();
        }

        private async UniTask LoadStartScenes(CancellationToken cancellationToken) {
            _rootScene = SceneManager.GetActiveScene().name;
            _applicationLaunchMode = ApplicationLaunchMode.FromRootScene;

            bool playSplashScreen = _splashScreenScene.IsValid();
            
#if UNITY_EDITOR
            if (_rootScene != SceneLoaderSettings.Instance.rootScene.scene) {
                Debug.LogError($"{nameof(SceneLoader)}: loaded not on the root scene {SceneLoaderSettings.Instance.rootScene.scene}, " + 
                               $"make sure {nameof(SceneLoader)} is on the root scene that should be selected in {nameof(SceneLoaderSettings)} asset.");
            }

            playSplashScreen &= _playSplashScreenInEditor;
#endif
            
            if (playSplashScreen) {
                await Fader.Main.FadeInAsync(duration: 0f);
                if (cancellationToken.IsCancellationRequested) return;
                
                if (_splashScreenScene.scene == _loadingScene.scene) {
                    LoadingService.Instance.ShowLoadingScreen(true);
                }
                
                await LoadSceneAsync(_splashScreenScene.scene, makeActive: true);
                if (cancellationToken.IsCancellationRequested) return;
                
                await Fader.Main.FadeOutAsync(_fadeOut, _fadeOutCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            LoadSceneAsync(_loadingScene.scene, makeActive: false).Forget();
            
            var startScenes = new List<string> { _startScene.scene };
            
#if UNITY_EDITOR
            if (SceneLoaderSettings.Instance.enablePlayModeStartSceneOverride) {
                var playmodeStartScenes = SceneLoaderSettings.GetPlaymodeStartScenes();
                playmodeStartScenes?.Remove(_rootScene);
                
                if (playmodeStartScenes is { Count: > 0 }) {
                    startScenes = playmodeStartScenes; 
                }
                
                // Force load gameplay scene in Unity Editor's playmode
                // if app is launched from custom scene.
                if (!startScenes.Contains(_startScene.scene)) {
                    _applicationLaunchMode = ApplicationLaunchMode.FromCustomEditorScene;
                
                    await LoadSceneAsync(_gameplayScene.scene, makeActive: false);
                    if (cancellationToken.IsCancellationRequested) return;
                }   
            }
#endif

            await LoadScenesAsync(startScenes);
            
            if (cancellationToken.IsCancellationRequested) return;

            if (playSplashScreen) {
                await Fader.Main.FadeInAsync(_fadeIn, _fadeInCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
                
                if (_splashScreenScene.scene != _loadingScene.scene) {
                    UnloadSceneAsync(_splashScreenScene.scene).Forget();
                }
                
                LoadingService.Instance.ShowLoadingScreen(false);
            }

            MakeSceneActive(startScenes[0]);
            
            await Fader.Main.FadeOutAsync(_fadeOut, _fadeOutCurve.GetOrDefault());
        }

        public static bool IsSceneLoaded(string sceneName) {
            return SceneManager.GetSceneByName(sceneName) is { isLoaded: true };
        }

        public static bool MakeSceneActive(string sceneName) {
            return SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public static void LoadScene(string sceneName, bool makeActive = false) {
            LoadSceneAsync(sceneName, makeActive).Forget();
        }

        public static void LoadScenes(IReadOnlyList<string> sceneNames, string activeScene = null) {
            LoadScenesAsync(sceneNames, activeScene).Forget();
        }

        public static void UnloadScene(string sceneName) {
            UnloadSceneAsync(sceneName).Forget();
        }
        
        public static void UnloadScenes(IReadOnlyList<string> sceneNames) {
            UnloadScenesAsync(sceneNames).Forget();
        }

        public static async UniTask LoadSceneAsync(string sceneName, bool makeActive) {
            if (sceneName == _rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is not { isLoaded: true }) {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public static async UniTask LoadScenesAsync(IReadOnlyList<string> sceneNames, string activeScene = null) {
            int count = sceneNames.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                string sceneName = sceneNames[i];
                tasks[i] = LoadSceneAsync(sceneName, makeActive: sceneName == activeScene);
            }

            await UniTask.WhenAll(tasks);

            tasks.ResetArrayElements(count);
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
            if (sceneName == _rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is { isLoaded: true }) {
                await SceneManager.UnloadSceneAsync(sceneName);
            }
        }
        
        public static async UniTask UnloadScenesAsync(IReadOnlyList<string> sceneNames) {
            int count = sceneNames.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                tasks[i] = UnloadSceneAsync(sceneNames[i]);
            }

            await UniTask.WhenAll(tasks);

            tasks.ResetArrayElements(count);
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _playSplashScreenInEditor = false;
#endif
    }
    
}
