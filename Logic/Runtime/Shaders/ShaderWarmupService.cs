using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace MisterGames.Logic.Shaders {

    internal sealed class ShaderWarmupService : IDisposable, IUpdate {
        
        public event Action<float> OnWarmupProgress = delegate { };
        public event Action OnWarmupCompleted = delegate { };
        public bool IsWarmupCompleted { get; private set; }

        internal static ShaderWarmupService Instance { get; private set; }

        private CancellationTokenSource _cts;
        private readonly ShaderWarmupSettings _settings;
        private GraphicsStateCollection _graphicsStateCollection;
        private bool _isTracing;
        private float _saveTimer;
        private bool _shaderWarmupScenePassCompleted;

        public ShaderWarmupService(ShaderWarmupSettings settings) {
            _settings = settings;
        }

        public void Initialize() {
            Instance = this;

            AsyncExt.RecreateCts(ref _cts);
            PlayerLoopStage.LateUpdate.Subscribe(this);

#if UNITY_EDITOR
            LoadForEditor(_cts.Token).Forget();
            return;
#endif

#if DEVELOPMENT_BUILD
            LoadForDevelopmentBuild(_cts.Token).Forget();
            return;
#endif

            LoadForReleaseBuild();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

#if UNITY_EDITOR
            return;
#endif

#if DEVELOPMENT_BUILD
            UnloadForDevelopmentBuild();
            return;
#endif
        }

        public void NotifyShaderWarmupScenePassCompleted() {
            _shaderWarmupScenePassCompleted = true;
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isTracing) return;

            _saveTimer += dt;
            if (_saveTimer < _settings.SavePeriod) return;

            _saveTimer = 0f;
            EndTracingAndSaveToLocalFile();
            BeginTracing();
        }

        private async UniTask LoadForEditor(CancellationToken cancellationToken) {
            if (_settings.EnterShadersWarmupSceneOnBootstrapInEditor) {
                if (IsSceneIncludedInBuildList(_settings.WarmupSceneName)) {
                    Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                     $"open shaders warmup scene {_settings.WarmupSceneName} for testing. " +
                                     $"It can be disabled in {_settings}.");

                    await SceneManager.LoadSceneAsync(_settings.WarmupSceneName, LoadSceneMode.Additive);

                    while (!_shaderWarmupScenePassCompleted && !cancellationToken.IsCancellationRequested) {
                        await UniTask.Yield();
                    }

                    await SceneManager.UnloadSceneAsync(_settings.WarmupSceneName);
                }
                else {
                    Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                     $"tried to open shaders warmup scene {_settings.WarmupSceneName} for testing, " +
                                     $"but it is not included in build scene list. Skip.");
                }
            }

            NotifyWarmupCompleted();
        }

        private static bool IsSceneIncludedInBuildList(string sceneName) {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                string name = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (string.Equals(name, sceneName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private async UniTask LoadForDevelopmentBuild(CancellationToken cancellationToken) {
            _graphicsStateCollection = new GraphicsStateCollection();
            BeginTracing();

            if (_settings.EnterShadersWarmupSceneOnBootstrapInDevBuild) {
                if (IsSceneIncludedInBuildList(_settings.WarmupSceneName)) {
                    Debug.LogWarning($"ShaderWarmupService.LoadForDevelopmentBuild: f {Time.frameCount}, " +
                                     $"open shaders warmup scene {_settings.WarmupSceneName} for dev build tracing. " +
                                     $"It can be disabled in {_settings}.");

                    await SceneManager.LoadSceneAsync(_settings.WarmupSceneName, LoadSceneMode.Additive);

                    while (!_shaderWarmupScenePassCompleted && !cancellationToken.IsCancellationRequested) {
                        await UniTask.Yield();
                    }

                    await SceneManager.UnloadSceneAsync(_settings.WarmupSceneName);
                }
                else {
                    Debug.LogWarning($"ShaderWarmupService.LoadForDevelopmentBuild: f {Time.frameCount}, " +
                                     $"tried to open shaders warmup scene {_settings.WarmupSceneName} for dev build tracing, " +
                                     $"but it is not included in build scene list. Skip.");
                }
            }

            EndTracingAndSaveToLocalFile();
            BeginTracing();

            StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, false, _cts.Token).Forget();
        }

        private void UnloadForDevelopmentBuild() {
            EndTracingAndSaveToLocalFile();
        }

        private void LoadForReleaseBuild() {
            StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, false, _cts.Token).Forget();
        }

        private GraphicsStateCollection LoadLocalTracingCollection() {
            var graphicsStateCollection = new GraphicsStateCollection();

            string filePath = _settings.GetTracingFilePath();
            if (File.Exists(filePath)) graphicsStateCollection.LoadFromFile(filePath);

            return graphicsStateCollection;
        }

        private void BeginTracing() {
            _isTracing = _graphicsStateCollection.BeginTrace();

            Debug.Log($"ShaderWarmupService.BeginTracing: f {Time.frameCount}, " +
                      $"begin PSO tracing: {_isTracing}, " +
                      $"API {SystemInfo.graphicsDeviceType}");
        }

        private void EndTracingAndSaveToLocalFile() {
            _isTracing = false;
            _graphicsStateCollection.EndTrace();

            Debug.Log($"ShaderWarmupService.EndTracingAndSaveToLocalFile: f {Time.frameCount}, " +
                      $"end PSO tracing, " +
                      $"total count {_graphicsStateCollection.totalGraphicsStateCount}");

            var localTracingCollection = LoadLocalTracingCollection();
            //localTracingCollection.Append(_graphicsStateCollection);
            SaveToLocalFile(localTracingCollection);
        }

        private void SaveToLocalFile(GraphicsStateCollection collection) {
            string filePath = _settings.GetTracingFilePath();
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (!File.Exists(filePath))
                using (File.Create(filePath)) { }

            collection.SaveToFile(filePath);
        }

        private GraphicsStateCollection LoadReleaseCollection() {
            return _settings.GetReleaseGraphicsStateCollection();
        }

        private async UniTask StartWarmup(
            GraphicsStateCollection graphicsStateCollection,
            VisualEffectAsset[] visualEffectAssets,
            bool traceCacheMisses,
            CancellationToken ct) {
            int totalPso = graphicsStateCollection.totalGraphicsStateCount;
            int totalVisualEffects = visualEffectAssets.Length;
            int total = totalPso + totalVisualEffects;

            var handle = new JobHandle();

            while (!ct.IsCancellationRequested && graphicsStateCollection.completedWarmupCount < totalPso) {
                //handle = graphicsStateCollection.WarmUpProgressively(_settings.ProgressiveWarmupBatchCountPso, handle, traceCacheMisses);
                handle.Complete();

                NotifyWarmupProgress((float) graphicsStateCollection.completedWarmupCount / total);

                await UniTask.Yield();
            }

            int warmedVisualEffects = 0;
            while (!ct.IsCancellationRequested && warmedVisualEffects < totalVisualEffects) {
                int batch = Mathf.Min(_settings.ProgressiveWarmupBatchCountVisualEffectAssets, totalVisualEffects - warmedVisualEffects);
                for (int i = 0; i < batch; i++) {
                    //visualEffectAssets[warmedVisualEffects + i].PrewarmComputeShaders();
                }

                warmedVisualEffects += batch;

                NotifyWarmupProgress((float) (totalPso + warmedVisualEffects) / total);

                await UniTask.Yield();
            }

            NotifyWarmupCompleted();
        }

        private void NotifyWarmupProgress(float progress) {
            OnWarmupProgress.Invoke(progress);
        }

        private void NotifyWarmupCompleted() {
            IsWarmupCompleted = true;
            OnWarmupCompleted.Invoke();
        }
    }
    
}