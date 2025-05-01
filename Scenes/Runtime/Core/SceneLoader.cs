using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
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
            
            await Fader.Main.FadeInAsync(duration: 0f);
            if (cancellationToken.IsCancellationRequested) return;
            
            if (_splashScreenScene.IsValid()) {
                if (_splashScreenScene.scene == _loadingScene.scene) {
                    LoadingService.Instance.ShowLoadingScreen(true);
                }
                
                await LoadSceneAsync(_splashScreenScene.scene, makeActive: true);
                if (cancellationToken.IsCancellationRequested) return;
                
                await Fader.Main.FadeOutAsync(_fadeOut, _fadeOutCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            await LoadSceneAsync(_loadingScene.scene, makeActive: false);
            if (cancellationToken.IsCancellationRequested) return;
            
            string startScene = _startScene.scene;
            
#if UNITY_EDITOR
            if (!SceneLoaderSettings.Instance.enablePlayModeStartSceneOverride) {
                return;
            }
            
            if (_rootScene != SceneLoaderSettings.Instance.rootScene.scene) {
                Debug.LogWarning($"{nameof(SceneLoader)}: loaded not on the root scene {SceneLoaderSettings.Instance.rootScene.scene}, " +
                                 $"make sure {nameof(SceneLoader)} is on the root scene that should be selected in {nameof(SceneLoaderSettings)} asset.");
            }

            string playModeStartScene = SceneLoaderSettings.GetPlaymodeStartScene();
            
            if (!string.IsNullOrEmpty(playModeStartScene) && playModeStartScene != _rootScene) {
                startScene = playModeStartScene;
            }
            
            // Force load gameplay scene in Unity Editor's playmode,
            // if playmode start scene is not selected start scene.
            if (startScene != _startScene.scene) {
                _applicationLaunchMode = ApplicationLaunchMode.FromCustomEditorScene;
                
                await LoadSceneAsync(_gameplayScene.scene, makeActive: false);
                if (cancellationToken.IsCancellationRequested) return;
            }
#endif

            await LoadSceneAsync(startScene, makeActive: false);
            if (cancellationToken.IsCancellationRequested) return;

            if (_splashScreenScene.IsValid()) {
                await Fader.Main.FadeInAsync(_fadeIn, _fadeInCurve.GetOrDefault());
                if (cancellationToken.IsCancellationRequested) return;
                
                if (_splashScreenScene.scene != _loadingScene.scene) {
                    await UnloadSceneAsync(_splashScreenScene.scene);
                    if (cancellationToken.IsCancellationRequested) return;
                }
            }

            MakeSceneActive(startScene);
            LoadingService.Instance.ShowLoadingScreen(false);
            
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

        public static void UnloadScene(string sceneName) {
            UnloadSceneAsync(sceneName).Forget();
        }

        public static async UniTask LoadSceneAsync(string sceneName, bool makeActive) {
            if (sceneName == _rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is not { isLoaded: true }) {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
            if (sceneName == _rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is { isLoaded: true }) {
                await SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
    
}
