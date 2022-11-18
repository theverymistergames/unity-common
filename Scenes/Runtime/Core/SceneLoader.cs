using System;
using System.Collections.Generic;
using MisterGames.Scenes.Transactions;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Core {
    
    public class SceneLoader : MonoBehaviour {

        [SerializeField] private TimeDomain _timeDomain;

        public static SceneLoader Instance { get; private set; }

        public IJobReadOnly TotalLoading => _totalLoadingJobs;

        private readonly JobObserver _totalLoadingJobs = new JobObserver();
        private readonly Dictionary<string, Scene> _loadedScenes = new Dictionary<string, Scene>();
        private readonly Dictionary<string, LoadingJob> _sceneLoadingJobMap = new Dictionary<string, LoadingJob>();

        private struct LoadingJob {
            public IJob job;
            public SceneTransactionType transactionType;
        }

        private enum SceneTransactionType {
            Load,
            Unload,
        }

        private void Awake() {
            ValidateFirstLoadedScene();

            Instance = this;
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            var rootScene = SceneManager.GetActiveScene();
            _loadedScenes[rootScene.name] = rootScene;

            LoadScene(ScenesStorage.Instance.SceneStart, true);
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            _loadedScenes.Clear();

            _totalLoadingJobs.StopAll();
            _totalLoadingJobs.Clear();
        }

        public IJobReadOnly CommitTransaction(ISceneTransaction transaction) {
            return transaction.Perform(this);
        }

        public IJobReadOnly LoadScene(string sceneName, bool makeActive = false) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) {
                Debug.LogWarning($"Trying to load root scene {rootScene}, it is not allowed.");
                return Jobs.Completed;
            }

            if (_loadedScenes.TryGetValue(sceneName, out var scene)) {
                if (_sceneLoadingJobMap.TryGetValue(sceneName, out var invalidLoadingJob)) {
                    invalidLoadingJob.job.Stop();
                    _sceneLoadingJobMap.Remove(sceneName);
                }

                if (makeActive) SceneManager.SetActiveScene(scene);
                return Jobs.Completed;
            }

            if (_sceneLoadingJobMap.TryGetValue(sceneName, out var loadingJob)) {
                if (loadingJob.transactionType == SceneTransactionType.Load) return loadingJob.job;

                loadingJob.job.Stop();
                _sceneLoadingJobMap.Remove(sceneName);
            }

            var job = JobSequence.Create()
                .WaitCompletion(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).AsReadOnlyJob())
                .Action(() => {
                    if (makeActive) SceneManager.SetActiveScene(_loadedScenes[sceneName]);
                    _sceneLoadingJobMap.Remove(sceneName);
                });

            _sceneLoadingJobMap[sceneName] = new LoadingJob {
                job = job,
                transactionType = SceneTransactionType.Load,
            };

            return job
                .StartFrom(_timeDomain.Source)
                .ObserveBy(_totalLoadingJobs);
        }

        public IJobReadOnly UnloadScene(string sceneName) {
            string rootScene = ScenesStorage.Instance.SceneRoot;
            if (sceneName == rootScene) {
                Debug.LogWarning($"Trying to unload root scene {rootScene}, it is not allowed.");
                return Jobs.Completed;
            }

            if (!_loadedScenes.TryGetValue(sceneName, out var scene)) {
                if (_sceneLoadingJobMap.TryGetValue(sceneName, out var invalidLoadingJob)) {
                    invalidLoadingJob.job.Stop();
                    _sceneLoadingJobMap.Remove(sceneName);
                }

                return Jobs.Completed;
            }

            if (_sceneLoadingJobMap.TryGetValue(sceneName, out var loadingJob)) {
                if (loadingJob.transactionType == SceneTransactionType.Unload) return loadingJob.job;

                loadingJob.job.Stop();
                _sceneLoadingJobMap.Remove(sceneName);
            }

            var job = JobSequence.Create()
                .WaitCompletion(SceneManager.UnloadSceneAsync(scene).AsReadOnlyJob())
                .Action(() => _sceneLoadingJobMap.Remove(sceneName));

            _sceneLoadingJobMap[sceneName] = new LoadingJob {
                job = job,
                transactionType = SceneTransactionType.Unload,
            };

            return job
                .StartFrom(_timeDomain.Source)
                .ObserveBy(_totalLoadingJobs);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            _loadedScenes[scene.name] = scene;
        }

        private void OnSceneUnloaded(Scene scene) {
            _loadedScenes.Remove(scene.name);
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
