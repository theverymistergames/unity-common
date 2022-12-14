using System.Collections.Generic;
using MisterGames.Scenes.Transactions;
using MisterGames.Tick.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public sealed class SceneLoader : MonoBehaviour {

        public static SceneLoader Instance { get; private set; }

        private readonly List<ReadOnlyJob> _loadJobs = new List<ReadOnlyJob>();

        private void Awake() {
            Instance = this;

            ValidateFirstLoadedScene();
            LoadScene(ScenesStorage.Instance.SceneStart, true);
        }

        private void OnDestroy() {
            Instance = null;

            CleanupAllJobs();
        }

        public ReadOnlyJob CommitTransaction(ISceneTransaction transaction) {
            return transaction.Perform(this);
        }

        public ReadOnlyJob LoadScene(string sceneName, bool makeActive = false) {
            CleanupCompletedJobs();

            var job = LoadSceneAsync(sceneName, makeActive);
            _loadJobs.Add(job);

            return job;
        }

        public ReadOnlyJob UnloadScene(string sceneName) {
            CleanupCompletedJobs();

            var job = UnloadSceneAsync(sceneName);
            _loadJobs.Add(job);

            return job;
        }

        private ReadOnlyJob LoadSceneAsync(string sceneName, bool makeActive) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return Jobs.Completed;

            return JobSequence.Create()
                .Wait(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive))
                .Action(() => { if (makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); })
                .Push()
                .Start();
        }

        private ReadOnlyJob UnloadSceneAsync(string sceneName) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) return Jobs.Completed;

            return Jobs.Wait(SceneManager.UnloadSceneAsync(sceneName));
        }

        private void CleanupCompletedJobs() {
            for (int i = _loadJobs.Count - 1; i >= 0; i--) {
                var job = _loadJobs[i];
                if (!job.IsCompleted) continue;

                job.Dispose();
                _loadJobs.RemoveAt(i);
            }
        }

        private void CleanupAllJobs() {
            for (int i = 0; i < _loadJobs.Count; i++) {
                _loadJobs[i].Dispose();
            }
            _loadJobs.Clear();
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
