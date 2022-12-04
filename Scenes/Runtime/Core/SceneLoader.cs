using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        public static SceneLoader Instance { get; private set; }

        private void Awake() {
            Instance = this;
            ValidateFirstLoadedScene();
            LoadScene(ScenesStorage.Instance.SceneStart, true);
        }

        private void OnDestroy() {
            Instance = null;
        }

        public void CommitTransaction(ISceneTransaction transaction) {
            transaction.Perform(this);
        }

        public void LoadScene(string sceneName, bool makeActive = false) {
            LoadSceneAsync(sceneName, makeActive).Forget();
        }

        public void UnloadScene(string sceneName) {
            UnloadSceneAsync(sceneName).Forget();
        }

        private async UniTaskVoid LoadSceneAsync(string sceneName, bool makeActive) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return;

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        private async UniTaskVoid UnloadSceneAsync(string sceneName) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return;

            await SceneManager.UnloadSceneAsync(sceneName);
        }

        private static void ValidateFirstLoadedScene() {
            string firstScene = SceneManager.GetActiveScene().name;
            string rootScene = ScenesStorage.Instance.SceneRoot;

            if (firstScene != rootScene) {
                Debug.LogWarning($"First loaded scene [{firstScene}] is not root scene [{rootScene}]. " +
                                 $"Move {nameof(SceneLoader)} prefab to root scene.");
            }
        }
    }
    
}
