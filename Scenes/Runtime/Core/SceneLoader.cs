using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        [SerializeField] private bool _useStartSceneFromSceneStorage = true;
        [SerializeField] private SceneReference _startScene;

        private void Awake() {
#if UNITY_EDITOR
            string firstScene = SceneManager.GetActiveScene().name;
            string rootScene = SceneStorage.Instance.RootScene;

            if (firstScene != rootScene) {
                Debug.LogWarning($"First loaded scene [{firstScene}] is not root scene [{rootScene}]. " +
                                 $"Move {nameof(SceneLoader)} prefab to root scene.");
            }

            string startScene = _useStartSceneFromSceneStorage ? SceneStorage.Instance.EditorStartScene : _startScene.scene;
#else
            string startScene = _startScene.scene;
#endif

            LoadScene(startScene, true);
        }

        public static void LoadScene(string sceneName, bool makeActive = false) {
            LoadSceneAsync(sceneName, makeActive).Forget();
        }

        public static void UnloadScene(string sceneName) {
            UnloadSceneAsync(sceneName).Forget();
        }

        public static async UniTask LoadSceneAsync(string sceneName, bool makeActive) {
            string rootScene = SceneStorage.Instance.RootScene;
            if (sceneName == rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is not { isLoaded: true }) {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
            string rootScene = SceneStorage.Instance.RootScene;
            if (sceneName == rootScene) return;

            if (SceneManager.GetSceneByName(sceneName) is { isLoaded: true }) {
                await SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
    
}
