using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
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
        
        private struct SceneLoadData
        {
            public AsyncOperation handle;
            public CancellationTokenSource cts;
            public bool isLoading;
        }
        
        private const bool EnableLogs = true;
        private static readonly string LogPrefix = "SceneLoader".FormatColorOnlyForEditor(Color.white);
        
        public static ApplicationLaunchMode LaunchMode => _instance._applicationLaunchMode;
        
        private static SceneLoader _instance;
        private static string _rootScene;
        private static readonly Dictionary<string, SceneLoadData> _loadSceneDataMap = new();
        
        private ApplicationLaunchMode _applicationLaunchMode;
        
        private void Awake() {
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            LoadStartScenes(destroyCancellationToken).Forget();
        }

        private void OnDestroy() {
            _loadSceneDataMap.Clear();
        }

        private async UniTask LoadStartScenes(CancellationToken cancellationToken) {
            _rootScene = SceneManager.GetActiveScene().name;
            _applicationLaunchMode = ApplicationLaunchMode.FromRootScene;

            bool playSplashScreen = _splashScreenScene.IsValid();
            
#if UNITY_EDITOR
            if (_rootScene != SceneLoaderSettings.Instance.rootScene.scene) {
                LogError($"loaded not on the root scene {SceneLoaderSettings.Instance.rootScene.scene}, " + 
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

            SetActiveScene(startScenes[0]);
            
            await Fader.Main.FadeOutAsync(_fadeOut, _fadeOutCurve.GetOrDefault());
        }

        public static bool IsSceneLoaded(string sceneName) {
            return SceneManager.GetSceneByName(sceneName) is { isLoaded: true };
        }

        public static bool SetActiveScene(string sceneName) {
            bool result = SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            if (result) {
                LogInfo($"set active scene {sceneName.FormatColorOnlyForEditor(Color.green)}");
            }
            else {
                LogWarning($"failed to set active scene {sceneName.FormatColorOnlyForEditor(Color.red)}");
            }
            
            return result;
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
            if (string.IsNullOrEmpty(sceneName) || sceneName == _rootScene) {
                return;
            }
            
            if (_loadSceneDataMap.TryGetValue(sceneName, out var data)) {
                if (data.isLoading) {
                    await data.handle.ToUniTask();
                    return;
                }
                
                data.cts.Cancel();
                data.cts.Dispose();
            }

            var handle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            data = new SceneLoadData { handle = handle, cts = new CancellationTokenSource(), isLoading = true }; 
            _loadSceneDataMap[sceneName] = data;
            
            await handle.WithCancellation(data.cts.Token);

            if (data.cts.IsCancellationRequested) return;
            
            LogInfo($"loaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)} ({SceneManager.GetSceneByName(sceneName).buildIndex})");

            if (makeActive) SetActiveScene(sceneName);
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
            if (string.IsNullOrEmpty(sceneName) || sceneName == _rootScene || 
                !_loadSceneDataMap.TryGetValue(sceneName, out var data)) 
            {
                return;
            }

            if (!data.isLoading) {
                await data.handle.ToUniTask();
                return;
            }
            
            data.cts.Cancel();
            data.cts.Dispose();
            
            var handle = SceneManager.UnloadSceneAsync(sceneName);
            if (handle == null) {
                _loadSceneDataMap.Remove(sceneName);    
                return;
            }
            
            data = new SceneLoadData { handle = handle, cts = new CancellationTokenSource(), isLoading = false }; 
            _loadSceneDataMap[sceneName] = data;
            
            await handle.WithCancellation(data.cts.Token);
            
            if (data.cts.IsCancellationRequested) return;

            LogInfo($"unloaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}");
            
            _loadSceneDataMap.Remove(sceneName);
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

        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogWarning(string message) {
            if (EnableLogs) Debug.LogWarning($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogError(string message) {
            if (EnableLogs) Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _playSplashScreenInEditor = false;
#endif
    }
    
}
