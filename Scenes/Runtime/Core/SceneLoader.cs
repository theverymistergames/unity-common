using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class SceneLoader : MonoBehaviour {

        [Header("Actions")]
        [SubclassSelector]
        [SerializeReference] private ISceneLoaderAction[] _startActions;
        
        private struct SceneLoadData
        {
            public AsyncOperation handle;
            public CancellationTokenSource cts;
            public bool isLoading;
        }
        
        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(SceneLoader).FormatColorOnlyForEditor(Color.white);
        
        public static ApplicationLaunchMode LaunchMode {
            get => _instance._applicationLaunchMode;
            internal set => _instance._applicationLaunchMode = value;
        }

        public static string RootScene => _instance._rootScene;

        private static SceneLoader _instance;
        
        private readonly Dictionary<string, SceneLoadData> _loadSceneDataMap = new();
        private ApplicationLaunchMode _applicationLaunchMode;
        private string _rootScene;
        
        private void Awake() {
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            _rootScene = SceneManager.GetActiveScene().name;
            
            LoadStartScenes(destroyCancellationToken).Forget();
        }

        private void OnDestroy() {
            _loadSceneDataMap.Clear();
        }

        private async UniTask LoadStartScenes(CancellationToken cancellationToken) {
            _applicationLaunchMode = ApplicationLaunchMode.FromRootScene;
            
#if UNITY_EDITOR
            if (RootScene != SceneLoaderSettings.Instance.rootScene.scene) {
                if (EnableLogs) LogError($"loaded not on the root scene {SceneLoaderSettings.Instance.rootScene.scene}, " + 
                                         $"make sure {nameof(SceneLoader)} is on the root scene that should be selected in {nameof(SceneLoaderSettings)} asset.");
            }
#endif

            for (int i = 0; i < _startActions?.Length && !cancellationToken.IsCancellationRequested; i++) {
                await _startActions[i].Apply(cancellationToken);
            }
        }

        public static bool IsSceneLoaded(string sceneName) {
            return SceneManager.GetSceneByName(sceneName) is { isLoaded: true };
        }

        public static bool SetActiveScene(string sceneName) {
            bool result = SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            if (result) {
                if (EnableLogs) LogInfo($"set active scene {sceneName.FormatColorOnlyForEditor(Color.green)}");
            }
            else {
                if (EnableLogs) LogWarning($"failed to set active scene {sceneName.FormatColorOnlyForEditor(Color.red)}");
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
            if (string.IsNullOrEmpty(sceneName) || sceneName == RootScene) {
                return;
            }
            
            if (_instance._loadSceneDataMap.TryGetValue(sceneName, out var data)) {
                if (data.isLoading) {
                    await data.handle.ToUniTask();
                    return;
                }
                
                data.cts.Cancel();
                data.cts.Dispose();
            }

            var handle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            data = new SceneLoadData { handle = handle, cts = new CancellationTokenSource(), isLoading = true }; 
            _instance._loadSceneDataMap[sceneName] = data;
            
            await handle.WithCancellation(data.cts.Token);

            if (data.cts.IsCancellationRequested) return;
            
            if (EnableLogs) LogInfo($"loaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)} ({SceneManager.GetSceneByName(sceneName).buildIndex})");

            if (makeActive) SetActiveScene(sceneName);
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
            if (string.IsNullOrEmpty(sceneName) || sceneName == RootScene || 
                !_instance._loadSceneDataMap.TryGetValue(sceneName, out var data)) 
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
                _instance._loadSceneDataMap.Remove(sceneName);    
                return;
            }
            
            data = new SceneLoadData { handle = handle, cts = new CancellationTokenSource(), isLoading = false }; 
            _instance._loadSceneDataMap[sceneName] = data;
            
            await handle.WithCancellation(data.cts.Token);
            
            if (data.cts.IsCancellationRequested) return;

            if (EnableLogs) LogInfo($"unloaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}");
            
            _instance._loadSceneDataMap.Remove(sceneName);
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
            Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogWarning(string message) {
            Debug.LogWarning($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
        
        private static void LogError(string message) {
            Debug.LogError($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }
    
}
