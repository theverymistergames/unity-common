using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
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
        private GraphicsStateCollection _graphicsStateCollection;
        private bool _isTracing;
        private float _saveTimer;
        private bool _shaderWarmupScenePassCompleted;
        
        private void Awake() {
            Instance = this;
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

        public ShaderWarmupSettings GetSettings() {
            return _settings;
        }

        public async UniTask Load() {
            AsyncExt.RecreateCts(ref _cts);
            
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

                    if (cancellationToken.IsCancellationRequested) return;
                    
                    await SceneManager.UnloadSceneAsync(_settings.WarmupSceneName);
                }
                else {
                    Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                     $"tried to open shaders warmup scene {_settings.WarmupSceneName} for testing, " +
                                     $"but it is not included in build scene list. Skip.");
                }
            }

            NotifyWarmupStarted();

            if (_settings.SimulateShadersWarmupInEditorDuration > 0f) 
            {
                Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                 $"simulate shaders warmup for testing. " +
                                 $"It can be disabled in {_settings}.");

                float t = 0f;
                float speed = 1f / _settings.SimulateShadersWarmupInEditorDuration;
                while (t < 1f && !cancellationToken.IsCancellationRequested) {
                    t = Mathf.Clamp01(t + Time.unscaledDeltaTime * speed);
                    NotifyWarmupProgress(t);
                    await UniTask.Yield();
                }
                
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            NotifyWarmupCompleted();

            if (_settings.SimulateShadersWarmupInEditorDuration > 0f) {
                await UniTask.Delay(
                        TimeSpan.FromSeconds(_settings.DisableViewDelay + _settings.DisableViewFader),
                        delayType: DelayType.UnscaledDeltaTime,
                        cancellationToken: cancellationToken
                    )
                    .SuppressCancellationThrow();
            }
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
            SaveToLocalFile(_graphicsStateCollection); // clear before tracing
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

            await StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            BeginTracing();
        }

        private void UnloadForDevelopmentBuild() {
            EndTracingAndSaveToLocalFile();
        }

        private UniTask LoadForReleaseBuild(CancellationToken ct) {
            return StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, ct);
        }

        private GraphicsStateCollection LoadLocalTracingCollection() {
            var graphicsStateCollection = new GraphicsStateCollection();

            string filePath = _settings.GetTracingFilePath();
            if (File.Exists(filePath)) graphicsStateCollection.LoadFromFile(filePath);

            return graphicsStateCollection;
        }

        private void BeginTracing() {
            _isTracing = _graphicsStateCollection.BeginTrace();
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            Debug.Log($"ShaderWarmupService.BeginTracing: f {Time.frameCount}, " +
                      $"begin PSO tracing: {_isTracing}, " +
                      $"API {SystemInfo.graphicsDeviceType}");
        }

        private void EndTracingAndSaveToLocalFile() {
            _isTracing = false;
            _graphicsStateCollection.EndTrace();
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            Debug.Log($"ShaderWarmupService.EndTracingAndSaveToLocalFile: f {Time.frameCount}, " +
                      $"end PSO tracing, " +
                      $"total count {_graphicsStateCollection.totalGraphicsStateCount}");

            var localTracingCollection = LoadLocalTracingCollection();
            localTracingCollection.Append(_graphicsStateCollection);
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
            CancellationToken ct) 
        {
            try {
                await WarmupRoutine(graphicsStateCollection, visualEffectAssets, ct);
            }
            catch (OperationCanceledException) {
                // 
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                NotifyWarmupCompleted();
            }
        }

        private async UniTask WarmupRoutine(
            GraphicsStateCollection graphicsStateCollection,
            VisualEffectAsset[] visualEffectAssets,
            CancellationToken ct)
        {
            if (_settings.WarmupStartDelayFrames > 0) {
                await UniTask.DelayFrame(_settings.WarmupStartDelayFrames, cancellationToken: ct);
            }

            int totalVariants = GetVariantCount(graphicsStateCollection);
            int totalVisualEffects = visualEffectAssets.Length;
            int total = totalVariants + totalVisualEffects;

            var handle = new JobHandle();
            int warmedVariants = 0;
            int batchVariants = _settings.ProgressiveWarmupBatchCountPso;

            while (!ct.IsCancellationRequested && warmedVariants < totalVariants) {
                handle = graphicsStateCollection.WarmUpProgressively(batchVariants, handle);
                handle.Complete();

                warmedVariants = Mathf.Min(warmedVariants + batchVariants, totalVariants);

                NotifyWarmupProgress((float) warmedVariants / total);

                await UniTask.Yield();
            }

            int warmedVisualEffects = 0;
            int batchVisualEffects = _settings.ProgressiveWarmupBatchCountVisualEffectAssets;

            while (!ct.IsCancellationRequested && warmedVisualEffects < totalVisualEffects) {
                int batch = Mathf.Min(batchVisualEffects, totalVisualEffects - warmedVisualEffects);

                for (int i = 0; i < batch; i++) visualEffectAssets[warmedVisualEffects + i].PrewarmComputeShaders();

                warmedVisualEffects += batch;

                NotifyWarmupProgress((float) (totalVariants + warmedVisualEffects) / total);

                await UniTask.Yield();
            }
        }

        private static int GetVariantCount(GraphicsStateCollection graphicsStateCollection)
        {
            var variants = ListPool<GraphicsStateCollection.ShaderVariant>.Get();

            try
            {
                graphicsStateCollection.GetVariants(variants);
                return variants.Count;
            }
            finally
            {
                ListPool<GraphicsStateCollection.ShaderVariant>.Release(variants);
            }
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