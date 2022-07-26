using MisterGames.Common.Lists;
using MisterGames.Common.Routines;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public class SceneLoader : MonoBehaviour {

        public static IAsyncTaskReadOnly CurrentLoading => Instance._task;
        private static SceneLoader Instance;

        private Scene _pendingActiveScene;
        private string _requestedActiveSceneName;
        private readonly AsyncTask _task = new AsyncTask();

        public static IAsyncTaskReadOnly LoadScene(string sceneName, bool makeActive = true) {
            return Instance.LoadSceneAdditive(sceneName, makeActive);
        }
        
        private void Awake() {
            Instance = this;
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            LoadSceneAdditive(ScenesStorage.Instance.SceneStart, true);
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private IAsyncTaskReadOnly LoadSceneAdditive(string sceneName, bool makeActive) {
            var loadedScenes = GetLoadedScenes();
            if (loadedScenes.Contains(sceneName)) return AsyncTask.Done;

            var task = new AsyncTask();
            string rootScene = ScenesStorage.Instance.SceneRoot;
            
            for (int i = 0; i < loadedScenes.Length; i++) {
                string loadedScene = loadedScenes[i];
                if (loadedScene == rootScene || loadedScene == sceneName) continue;

                var unloadOperation = SceneManager.UnloadSceneAsync(loadedScene);
                task.Add(unloadOperation);
            }
            
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            task.Add(loadOperation);

            if (makeActive) {
                RequestActiveScene(sceneName);
            }
            
            return task;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            CheckSetRequestedActiveScene(scene);
        }

        private void RequestActiveScene(string sceneName) {
            if (SceneManager.GetActiveScene().name == sceneName) {
                return;
            }
            
            _requestedActiveSceneName = sceneName;
        }

        private void CheckSetRequestedActiveScene(Scene scene) {
            if (scene.name != _requestedActiveSceneName) {
                return;
            }
            
            if (SceneManager.GetActiveScene().name == scene.name) {
                return;
            }
            
            SceneManager.SetActiveScene(scene);
        }

        private static string[] GetLoadedScenes() {
            int sceneCount = SceneManager.sceneCount;
            var result = new string[sceneCount];
            
            for (int i = 0; i < sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                result[i] = scene.name;
            }

            return result;
        }
    }
    
}