using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Build;
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

        [Header("Debug")]
        [SerializeField] private LogLevel _logLevel = LogLevel.Short;

        private enum LogLevel {
            Off,
            Short,
            Full,
        }
        
        private struct SceneLoadData
        {
            public AsyncOperation handle;
            public CancellationTokenSource cts;
            public bool isLoading;
        }

        private static readonly string LogPrefix = nameof(SceneLoader).FormatColorOnlyForEditor(Color.white);

        private static SceneLoader _instance;
        private static bool _destroyed = true;
        private static readonly HashSet<ISceneLoadHook> _sceneLoadHooks = new();
        
        private readonly Dictionary<string, SceneLoadData> _loadSceneDataMap = new();
        private ApplicationLaunchMode _applicationLaunchMode;
        private string _rootScene;
        private string _requestedActiveScene;
        private int _loadId;
        
        private void Awake() {
            _destroyed = false;
            
            if (_instance != null) {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            _rootScene = SceneManager.GetActiveScene().name;
            _requestedActiveScene = _rootScene;
            _applicationLaunchMode = ApplicationLaunchMode.FromRootScene;
            
            LoadStartScenes(destroyCancellationToken).Forget();
        }

        private void OnDestroy() {
            _destroyed = true;
            _instance = null;

            _loadSceneDataMap.Clear();
            _sceneLoadHooks.Clear();
        }

        private async UniTask LoadStartScenes(CancellationToken cancellationToken) {
#if UNITY_EDITOR
            if (GetRootScene() != SceneLoaderSettings.Instance.rootScene.scene) {
                if (_logLevel >= LogLevel.Short) LogError($"loaded not on the root scene {SceneLoaderSettings.Instance.rootScene.scene}, " + 
                                                          $"make sure {nameof(SceneLoader)} is on the root scene that should be selected in {nameof(SceneLoaderSettings)} asset.");
            }
#endif

            for (int i = 0; i < _startActions?.Length && !cancellationToken.IsCancellationRequested; i++) {
                await _startActions[i].Apply(cancellationToken);
            }
        }

        public static ApplicationLaunchMode GetApplicationLaunchMode() {
            return _destroyed ? ApplicationLaunchMode.FromRootScene : _instance._applicationLaunchMode;
        }

        public static void SetApplicationLaunchMode(ApplicationLaunchMode mode) {
            if (!_destroyed) _instance._applicationLaunchMode = mode;
        }
        
        public static string GetRootScene() {
            return _destroyed ? null : _instance._rootScene;
        }
        
        public static bool IsSceneLoaded(string sceneName) {
            return SceneManager.GetSceneByName(sceneName) is { isLoaded: true };
        }

        public static bool IsSceneRequestedToLoad(string sceneName) {
            return !_destroyed && 
                   _instance._loadSceneDataMap.TryGetValue(sceneName, out var data) && data.isLoading;
        }
        
        public static void SetActiveScene(string sceneName) {
            if (!_destroyed) _instance.SetActiveSceneInternal(sceneName);
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

        public static UniTask LoadSceneAsync(string sceneName, bool makeActive = false) {
            return _destroyed ? default : _instance.LoadSceneAsyncInternal(sceneName, makeActive);
        }

        public static UniTask UnloadSceneAsync(string sceneName) {
            return _destroyed ? default : _instance.UnloadSceneAsyncInternal(sceneName);
        }

        public static async UniTask LoadScenesAsync(IReadOnlyList<string> sceneNames, string activeScene = null) {
            int count = sceneNames.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                string sceneName = sceneNames[i];
                tasks[i] = LoadSceneAsync(sceneName, makeActive: sceneName == activeScene);
            }

            await UniTask.WhenAll(tasks);

            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public static async UniTask UnloadScenesAsync(IReadOnlyList<string> sceneNames) {
            int count = sceneNames.Count;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                tasks[i] = UnloadSceneAsync(sceneNames[i]);
            }

            await UniTask.WhenAll(tasks);

            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
        }
        
        public static void AddSceneLoadHook(ISceneLoadHook hook) {
            _sceneLoadHooks.Add(hook);
        }
        
        public static void RemoveSceneLoadHook(ISceneLoadHook hook) {
            _sceneLoadHooks.Remove(hook);
        }
        
        private void SetActiveSceneInternal(string sceneName) {
            if (SceneManager.GetActiveScene().name == sceneName) return;
            
            _requestedActiveScene = sceneName;
            
            bool result = SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            
            if (_logLevel >= LogLevel.Short) {
                if (result) {
                    LogInfo($"set active scene {sceneName.FormatColorOnlyForEditor(Color.green)}");
                }
                else {
                    LogWarning($"failed to set active scene {sceneName.FormatColorOnlyForEditor(Color.red)}");
                }   
            }
        }
        
        private async UniTask LoadSceneAsyncInternal(string sceneName, bool makeActive = false) {
            if (string.IsNullOrEmpty(sceneName) || sceneName == GetRootScene()) {
                return;
            }

            int id = GetNextLoadId();

            if (_logLevel >= LogLevel.Full) {
                LogInfo($"#{id}, requested to load scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}, make active {makeActive}");
            }

            if (makeActive) {
                _requestedActiveScene = sceneName;
                
                if (IsSceneLoaded(sceneName)) {
                    SetActiveSceneInternal(sceneName);
                    return;
                }
            }
            
            if (_loadSceneDataMap.TryGetValue(sceneName, out var data)) {
                if (data.isLoading) {
                    await data.handle.ToUniTask();
                    return;
                }
                
                data.cts.Cancel();
                data.cts.Dispose();
            }
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            await ProcessSceneHooksLoadRequest(sceneName, token);
            if (token.IsCancellationRequested) return;
            
            var handle = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            data = new SceneLoadData { handle = handle, cts = cts, isLoading = true }; 
            _loadSceneDataMap[sceneName] = data;
            
            await handle.WithCancellation(token);
            if (token.IsCancellationRequested) return;

            if (_logLevel >= LogLevel.Short) {
                LogInfo($"#{id}, loaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}");
            }
            
            if (IsSceneLoaded(_requestedActiveScene)) {
                SetActiveSceneInternal(_requestedActiveScene);
            }
        }

        private async UniTask UnloadSceneAsyncInternal(string sceneName) {
            if (string.IsNullOrEmpty(sceneName) || sceneName == GetRootScene() || 
                !_loadSceneDataMap.TryGetValue(sceneName, out var data)) 
            {
                return;
            }

            int id = GetNextLoadId();

            if (_logLevel >= LogLevel.Full) {
                LogInfo($"#{id}, requested to unload scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}");
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
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            data = new SceneLoadData { handle = handle, cts = cts, isLoading = false }; 
            _loadSceneDataMap[sceneName] = data;

            await AsyncExt.WhenAll(handle.WithCancellation(token), ProcessSceneHooksUnloadRequest(sceneName, token));
            if (token.IsCancellationRequested) return;

            if (_logLevel >= LogLevel.Short) {
                LogInfo($"unloaded scene {sceneName.FormatColorOnlyForEditor(Color.yellow)}");
            }
            
            _loadSceneDataMap.Remove(sceneName);
        }

        private static async UniTask ProcessSceneHooksLoadRequest(string sceneName, CancellationToken cancellationToken) {
            int count = _sceneLoadHooks.Count;
            
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            int index = 0;
            
            foreach (var hook in _sceneLoadHooks) {
                tasks[index++] = hook.OnSceneLoadRequest(sceneName, cancellationToken);
            }
            
            await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
        }
        
        private static async UniTask ProcessSceneHooksUnloadRequest(string sceneName, CancellationToken cancellationToken) {
            int count = _sceneLoadHooks.Count;
            
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);
            int index = 0;
            
            foreach (var hook in _sceneLoadHooks) {
                tasks[index++] = hook.OnSceneUnloadRequest(sceneName, cancellationToken);
            }
            
            await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask>.Shared.Return(tasks);
        }
        
        private int GetNextLoadId() {
            int id;
            
            unchecked {
                id = _loadId++;
            }
            
            return id;
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
