using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Common.Tick {

    public interface ITimeSourceProvider {
        bool ShowDebugInfo { get; }
        ITimeSource Get(PlayerLoopStage stage);
    }

    public static class TimeSources {

        public static int frameCount =>
#if UNITY_EDITOR
            !Application.isPlaying && _isRunningEditorUpdates
                ? _editorUpdatesFrameCount 
                : Time.frameCount;
#else
        Time.frameCount;
#endif

        public static float time =>
#if UNITY_EDITOR
            !Application.isPlaying && _isRunningEditorUpdates 
                ? _editorUpdatesTime 
                : Time.time;
#else
        Time.time;
#endif
        
        public static float scaledTime =>
#if UNITY_EDITOR
            !Application.isPlaying && _isRunningEditorUpdates 
                ? _editorUpdatesTime 
                : Get(PlayerLoopStage.Update).ScaledTime;
#else
        Time.time;
#endif

#if UNITY_EDITOR
        internal static bool ShowDebugInfo => _provider?.ShowDebugInfo ?? false;
#endif
        
        public static ITimeSource Get(this PlayerLoopStage stage) {
#if UNITY_EDITOR
            return Application.isPlaying ? _provider.Get(stage) : GetOrCreateEditorTimeSource(stage);
#endif
            return _provider.Get(stage);
        }

        public static void Subscribe(this PlayerLoopStage stage, IUpdate sub) {
            Get(stage).Subscribe(sub);
        }

        public static void Unsubscribe(this PlayerLoopStage stage, IUpdate sub) {
            Get(stage).Unsubscribe(sub);
        }

        public static float DeltaTime(PlayerLoopStage stage = PlayerLoopStage.Update) {
            return Get(stage).DeltaTime;
        }
        
        private static ITimeSourceProvider _provider;

        internal static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }

#if UNITY_EDITOR
        private static EditorDeltaTimeProvider _editorDeltaTimeProvider;
        private static ITimeScaleProvider _editorTimeScaleProvider;
        private static ITimeScaleProvider _editorUnscaledTimeScaleProvider;

        private static TimeSource _editorPreUpdateTimeSource;
        private static TimeSource _editorUpdateTimeSource;
        private static TimeSource _editorUnscaledUpdateTimeSource;
        private static TimeSource _editorLateUpdateTimeSource;

        private static bool _isRunningEditorUpdates;
        private static int _editorUpdatesFrameCount;
        private static float _editorUpdatesTime;

        private static ITimeSource GetOrCreateEditorTimeSource(PlayerLoopStage stage) {
            CheckEditorUpdatesAreStarted().Forget();

            switch (stage) {
                case PlayerLoopStage.PreUpdate:
                    if (_editorPreUpdateTimeSource != null) return _editorPreUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorPreUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor pre update");

                    return _editorPreUpdateTimeSource;

                case PlayerLoopStage.Update:
                    if (_editorUpdateTimeSource != null) return _editorUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor update");

                    return _editorUpdateTimeSource;

                case PlayerLoopStage.UnscaledUpdate:
                    if (_editorUnscaledUpdateTimeSource != null) return _editorUnscaledUpdateTimeSource;

                    _editorUnscaledTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorUnscaledUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorUnscaledTimeScaleProvider, "editor unscaled");

                    return _editorUnscaledUpdateTimeSource;

                case PlayerLoopStage.LateUpdate:
                    if (_editorLateUpdateTimeSource != null) return _editorLateUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorLateUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor late update");

                    return _editorLateUpdateTimeSource;

                default:
                    throw new NotImplementedException($"Cannot create TimeSource for {nameof(PlayerLoopStage)} {stage} in edit mode");
            }
        }

        private static async UniTaskVoid CheckEditorUpdatesAreStarted() {
            if (_isRunningEditorUpdates &&
                (_editorPreUpdateTimeSource != null ||
                _editorUpdateTimeSource != null ||
                _editorUnscaledUpdateTimeSource != null ||
                _editorLateUpdateTimeSource != null)) 
            {
                return;
            }
            
            _isRunningEditorUpdates = true;
            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.time;
            _editorDeltaTimeProvider = new EditorDeltaTimeProvider();
            
            while (true) {
                _editorDeltaTimeProvider.UpdateDeltaTime();
                _editorUpdatesTime += _editorDeltaTimeProvider.DeltaTime;
                
                _editorPreUpdateTimeSource?.Tick();
                _editorUpdateTimeSource?.Tick();
                _editorUnscaledUpdateTimeSource?.Tick();
                _editorLateUpdateTimeSource?.Tick();

                await UniTask.Yield();

                _editorUpdatesFrameCount++;
                
                bool canStopEditorUpdates =
                    CanStopUpdate(_editorPreUpdateTimeSource) &&
                    CanStopUpdate(_editorUpdateTimeSource) &&
                    CanStopUpdate(_editorUnscaledUpdateTimeSource) &&
                    CanStopUpdate(_editorLateUpdateTimeSource);

                if (canStopEditorUpdates) break;
            }

            _editorDeltaTimeProvider = null;
            _editorPreUpdateTimeSource = null;
            _editorUpdateTimeSource = null;
            _editorUnscaledUpdateTimeSource = null;
            _editorLateUpdateTimeSource = null;

            _isRunningEditorUpdates = false;
            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.time;
        }

        private static bool CanStopUpdate(ITimeSourceApi timeSource) {
            return timeSource is not { SubscribersCount: > 0 };
        }

        private sealed class EditorDeltaTimeProvider : IDeltaTimeProvider {

            public float DeltaTime { get; private set; }

            private float _lastTimeSinceStartup = Time.realtimeSinceStartup;

            public void UpdateDeltaTime() {
                float timeSinceStartup = Time.realtimeSinceStartup;
                DeltaTime = timeSinceStartup - _lastTimeSinceStartup;
                _lastTimeSinceStartup = timeSinceStartup;
            }
        }
#endif
    }

}
