using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public class SceneLoader : MonoBehaviour {

        public static SceneLoader Instance { get; private set; }

        private void Awake() {
            Instance = this;
            ValidateFirstLoadedScene();
            LoadScene(ScenesStorage.Instance.SceneStart, true).Forget();
        }

        private void OnDestroy() {
            Instance = null;
        }

        public async UniTaskVoid CommitTransaction(ISceneTransaction transaction) {
            await transaction.Perform(this);
        }

        public async UniTask LoadScene(string sceneName, bool makeActive = false) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return;

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public async UniTask UnloadScene(string sceneName) {
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
