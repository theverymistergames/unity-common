using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace MisterGames.Logic.ShadersWarmup {

    [DefaultExecutionOrder(-100_000)]
    internal sealed class ShaderWarmupService : MonoBehaviour, IUpdate {

        [SerializeField] private ShaderWarmupSettings _settings;
        
        public event Action<float> OnWarmupProgress = delegate { };
        public event Action OnWarmupStarted = delegate { };
        public event Action OnWarmupCompleted = delegate { };
        public bool IsWarmupCompleted { get; private set; }

        public static ShaderWarmupService Instance { get; private set; }

        private CancellationTokenSource _cts;
        private UnityEngine.Rendering.GraphicsStateCollection _graphicsStateCollection;
        private bool _isTracing;
        private float _saveTimer;
        private bool _shaderWarmupScenePassCompleted;

        private void Awake() {
            Instance = this;
            
            AsyncExt.RecreateCts(ref _cts);
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDestroy() {
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

        public async UniTask Load() {
#if UNITY_EDITOR
            await LoadForEditor(_cts.Token);
            return;
#endif

#if DEVELOPMENT_BUILD
            await LoadForDevelopmentBuild(_cts.Token);
            return;
#endif

            await LoadForReleaseBuild(_cts.Token);
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

            NotifyWarmupStarted();
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
            _graphicsStateCollection = new UnityEngine.Rendering.GraphicsStateCollection();
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

            await StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, false, cancellationToken);
        }

        private void UnloadForDevelopmentBuild() {
            EndTracingAndSaveToLocalFile();
        }

        private UniTask LoadForReleaseBuild(CancellationToken ct) {
            return StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, false, ct);
        }

        private UnityEngine.Rendering.GraphicsStateCollection LoadLocalTracingCollection() {
            var graphicsStateCollection = new UnityEngine.Rendering.GraphicsStateCollection();

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
            localTracingCollection.Append(_graphicsStateCollection);
            SaveToLocalFile(localTracingCollection);
        }

        private void SaveToLocalFile(UnityEngine.Rendering.GraphicsStateCollection collection) {
            string filePath = _settings.GetTracingFilePath();
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (!File.Exists(filePath))
                using (File.Create(filePath)) { }

            collection.SaveToFile(filePath);
        }

        private UnityEngine.Rendering.GraphicsStateCollection LoadReleaseCollection() {
            return _settings.GetReleaseGraphicsStateCollection();
        }

        private async UniTask StartWarmup(
            UnityEngine.Rendering.GraphicsStateCollection graphicsStateCollection,
            VisualEffectAsset[] visualEffectAssets,
            bool traceCacheMisses,
            CancellationToken ct) 
        {
            NotifyWarmupStarted();
            
            int totalPso = graphicsStateCollection.totalGraphicsStateCount;
            int totalVisualEffects = visualEffectAssets.Length;
            int total = totalPso + totalVisualEffects;

            var handle = new JobHandle();

            while (!ct.IsCancellationRequested && graphicsStateCollection.completedWarmupCount < totalPso) {
                handle = graphicsStateCollection.WarmUpProgressively(_settings.ProgressiveWarmupBatchCountPso, handle, traceCacheMisses);
                handle.Complete();

                NotifyWarmupProgress((float) graphicsStateCollection.completedWarmupCount / total);

                await UniTask.Yield();
            }

            int warmedVisualEffects = 0;
            while (!ct.IsCancellationRequested && warmedVisualEffects < totalVisualEffects) {
                int batch = Mathf.Min(_settings.ProgressiveWarmupBatchCountVisualEffectAssets, totalVisualEffects - warmedVisualEffects);
                for (int i = 0; i < batch; i++) {
                    visualEffectAssets[warmedVisualEffects + i].PrewarmComputeShaders();
                }

                warmedVisualEffects += batch;

                NotifyWarmupProgress((float) (totalPso + warmedVisualEffects) / total);

                await UniTask.Yield();
            }

            NotifyWarmupCompleted();
        }

        private void NotifyWarmupStarted() {
            OnWarmupStarted.Invoke();
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