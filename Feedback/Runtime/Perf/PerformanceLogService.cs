using System;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace MisterGames.Feedback.Perf {

    /// <summary>
    /// Writes performance logs into the console and into the feedback service: hardware and software info once
    /// on start, then fps, memory and graphics settings each <see cref="PerformanceLogServiceConfig.LogPeriodSec"/>.
    ///
    /// Fps is collected the same way as the Graphy fps monitor does: a ring buffer of the last frames,
    /// where 1% low is an average of the capacity/100 worst frames and 0.1% low is an average
    /// of the capacity/1000 worst frames. Samples are sorted only at the moment of the log, not each frame.
    /// </summary>
    public sealed class PerformanceLogService : IPerformanceLogService, IUpdate, IDisposable {

        private const string LogHeader = "[PerformanceLog]";
        private const float BytesInMb = 1048576f;

        public short CurrentFps { get; private set; }
        public short AverageFps { get; private set; }
        public short OnePercentFps { get; private set; }
        public short Zero1PercentFps { get; private set; }

        private PerformanceLogServiceConfig _config;
        private short[] _fpsSamples;
        private short[] _fpsSamplesSorted;
        private int _fpsSamplesCount;
        private int _fpsSampleIndex;
        private float _timer;
        private bool _isEnabled;
        private bool _isSystemInfoLogged;

        public void Initialize(PerformanceLogServiceConfig config) {
            _config = config;

            if (config == null || Application.isEditor && !config.EnableInEditor) return;

            _isEnabled = true;
            _fpsSamples = new short[config.FpsSamplesCapacity];
            _fpsSamplesSorted = new short[config.FpsSamplesCapacity];
            _timer = 0f;
            _isSystemInfoLogged = false;

            ResetFpsSamples();

            PlayerLoopStage.UnscaledUpdate.Subscribe(this);
        }

        public void Dispose() {
            if (!_isEnabled) return;

            _isEnabled = false;
            PlayerLoopStage.UnscaledUpdate.Unsubscribe(this);
        }

        public void LogPerformance() {
            if (!_isEnabled) return;

            UpdateFpsStats();
            Log(GetPerformanceText());

            if (_config.ResetFpsSamplesOnLog) ResetFpsSamples();

            _timer = 0f;
        }

        void IUpdate.OnUpdate(float dt) {
            // System info is written on the first frame, not on initialization:
            // the feedback service can be launched later in the same Awake pass.
            if (!_isSystemInfoLogged) {
                _isSystemInfoLogged = true;
                if (_config.LogSystemInfoOnStart) Log(GetSystemInfoText());
            }

            // Frames of a paused or an unfocused app are reported with zero dt and are not real frames.
            if (dt <= 0f) return;

            CurrentFps = (short) Mathf.RoundToInt(1f / dt);

            _fpsSamples[_fpsSampleIndex++] = CurrentFps;
            if (_fpsSampleIndex >= _fpsSamples.Length) _fpsSampleIndex = 0;
            if (_fpsSamplesCount < _fpsSamples.Length) _fpsSamplesCount++;

            _timer += dt;
            if (_timer >= _config.LogPeriodSec) LogPerformance();
        }

        private void UpdateFpsStats() {
            if (_fpsSamplesCount <= 0) return;

            long sum = 0;
            for (int i = 0; i < _fpsSamplesCount; i++) {
                sum += _fpsSamples[i];
            }

            AverageFps = (short) (sum / _fpsSamplesCount);

            Array.Copy(_fpsSamples, _fpsSamplesSorted, _fpsSamplesCount);
            Array.Sort(_fpsSamplesSorted, 0, _fpsSamplesCount);

            OnePercentFps = GetWorstFpsAverage(Mathf.Max(1, _fpsSamples.Length / 100));
            Zero1PercentFps = GetWorstFpsAverage(Mathf.Max(1, _fpsSamples.Length / 1000));
        }

        private short GetWorstFpsAverage(int samples) {
            int count = Mathf.Min(samples, _fpsSamplesCount);
            long sum = 0;

            for (int i = 0; i < count; i++) {
                sum += _fpsSamplesSorted[i];
            }

            return (short) (sum / count);
        }

        private void ResetFpsSamples() {
            Array.Clear(_fpsSamples, 0, _fpsSamples.Length);

            _fpsSamplesCount = 0;
            _fpsSampleIndex = 0;
        }

        private static string GetSystemInfoText() {
            var res = Screen.currentResolution;

            string cpuText = $"CPU: {SystemInfo.processorType} [{SystemInfo.processorCount} cores]";
            string ramText = $"RAM: {SystemInfo.systemMemorySize} MB";
            string graphicsDeviceVersionText = $"Graphics API: {SystemInfo.graphicsDeviceVersion}";
            string graphicsDeviceNameText = $"GPU: {SystemInfo.graphicsDeviceName}";
            string graphicsMemorySizeText = $"VRAM: {SystemInfo.graphicsMemorySize} MB. " +
                                            $"Max texture size: {SystemInfo.maxTextureSize} px. " +
                                            $"Shader level: {SystemInfo.graphicsShaderLevel}";
            string screenResolutionText = $"Screen: {res.width}x{res.height}@{(int) res.refreshRateRatio.value}Hz";
            string operatingSystemText = $"OS: {SystemInfo.operatingSystem} [{SystemInfo.deviceType}]";
            string appText = $"App: {Application.productName} {Application.version} [{Application.platform}]";

            return $"{LogHeader}" + Environment.NewLine +
                   $"{cpuText} " + Environment.NewLine +
                   $"{ramText} " + Environment.NewLine +
                   $"{graphicsDeviceVersionText} " + Environment.NewLine +
                   $"{graphicsDeviceNameText} " + Environment.NewLine +
                   $"{graphicsMemorySizeText} " + Environment.NewLine +
                   $"{screenResolutionText} " + Environment.NewLine +
                   $"{operatingSystemText} " + Environment.NewLine +
                   $"{appText} ";
        }

        private string GetPerformanceText() {
            var res = Screen.currentResolution;
            int qualityLevel = QualitySettings.GetQualityLevel();
            string[] qualityNames = QualitySettings.names;
            string qualityName = qualityLevel >= 0 && qualityLevel < qualityNames.Length
                ? qualityNames[qualityLevel]
                : "unknown";

            string fpsText = $"FPS: {CurrentFps} current, {AverageFps} average, " +
                             $"{OnePercentFps} 1% low, {Zero1PercentFps} 0.1% low [{_fpsSamplesCount} frames]";

            // Profiler counters are filled in development builds only, managed heap size is always available.
            string memoryText = $"Memory: {Profiler.GetTotalAllocatedMemoryLong() / BytesInMb:0} MB allocated, " +
                                $"{Profiler.GetTotalReservedMemoryLong() / BytesInMb:0} MB reserved, " +
                                $"{Profiler.GetMonoUsedSizeLong() / BytesInMb:0} MB mono, " +
                                $"{GC.GetTotalMemory(false) / BytesInMb:0} MB managed heap, " +
                                $"{SystemInfo.systemMemorySize} MB system";

            string qualityText = $"Quality: {qualityName} [level {qualityLevel}], " +
                                 $"vSync {QualitySettings.vSyncCount}, " +
                                 $"target fps {Application.targetFrameRate}, " +
                                 $"MSAA {QualitySettings.antiAliasing}x, " +
                                 $"texture mipmap limit {QualitySettings.globalTextureMipmapLimit}, " +
                                 $"anisotropic {QualitySettings.anisotropicFiltering}";

            string shadowsText = $"Shadows: {QualitySettings.shadows} [{QualitySettings.shadowResolution}], " +
                                 $"distance {QualitySettings.shadowDistance:0}, " +
                                 $"cascades {QualitySettings.shadowCascades}";

            string screenText = $"Screen: {Screen.width}x{Screen.height} [{Screen.fullScreenMode}], " +
                                $"display {res.width}x{res.height}@{(int) res.refreshRateRatio.value}Hz";

            string sessionText = $"Session: {Time.realtimeSinceStartup:0} s, " +
                                 $"active scene [{SceneManager.GetActiveScene().name}], " +
                                 $"time scale {Time.timeScale:0.##}";

            return $"{LogHeader}" + Environment.NewLine +
                   $"{fpsText} " + Environment.NewLine +
                   $"{memoryText} " + Environment.NewLine +
                   $"{qualityText} " + Environment.NewLine +
                   $"{shadowsText} " + Environment.NewLine +
                   $"{screenText} " + Environment.NewLine +
                   $"{sessionText} ";
        }

        private void Log(string text) {
            if (_config.EnableLogs) Debug.Log(text);
            if (_config.SendToFeedback) FeedbackService.Log(text);
        }
    }

}
