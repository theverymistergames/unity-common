using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        [SerializeField] private SceneReference _startScene;
        [SerializeField] private SceneReference _gameplayScene;
        
        private static string _rootScene;
        
        private void Awake() {
            LoadStartScenes().Forget();
        }

        private async UniTask LoadStartScenes() {
            _rootScene = SceneManager.GetActiveScene().name;
            
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
                await LoadSceneAsync(_gameplayScene.scene, makeActive: false);
            }
#endif
            
            await LoadSceneAsync(startScene, makeActive: true);
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
