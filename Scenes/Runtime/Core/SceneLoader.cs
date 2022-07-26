﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        private void Awake() {
            ValidateFirstLoadedScene();
            LoadScene(ScenesStorage.Instance.SceneStart, true);
        }

        public static void LoadScene(string sceneName, bool makeActive = false) {
            LoadSceneAsync(sceneName, makeActive).Forget();
        }

        public static void UnloadScene(string sceneName) {
            UnloadSceneAsync(sceneName).Forget();
        }

        public static async UniTask LoadSceneAsync(string sceneName, bool makeActive) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return;

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public static async UniTask UnloadSceneAsync(string sceneName) {
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
