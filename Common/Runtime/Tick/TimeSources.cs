using System;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Tick {

    public interface ITimeSourceProvider {
        
        bool ShowDebugInfo { get; }
        
        ITimeSource Get(PlayerLoopStage stage);
        
        ITimeSource UpdateStage();
        ITimeSource FixedUpdateStage();
    }

    public static class TimeSources {

        public static float time {
            get {
#if UNITY_EDITOR
                if (Application.isPlaying) return Time.time;
                
                if (_editorDeltaTimeProvider == null) CheckEditorUpdatesAreStarted().Forget();
                
                return _editorUpdatesTime;
#else
                return Time.time;
#endif
            }
        }
        
        public static float scaledTime => GetUpdateStage().ScaledTime;
        
        public static float deltaTime => GetUpdateStage().DeltaTime;
        
        public static int frameCount {
            get {
#if UNITY_EDITOR
                if (Application.isPlaying) return Time.frameCount;
                
                if (_editorDeltaTimeProvider == null) CheckEditorUpdatesAreStarted().Forget();
                
                return _editorUpdatesFrameCount;
#else
                return Time.frameCount;
#endif
            }
        }
        
        public static int fixedFrameCount => GetFixedUpdateStage().FrameCount;

#if UNITY_EDITOR
        internal static bool ShowDebugInfo => _provider?.ShowDebugInfo ?? false;
#endif

        public static ITimeSource Get(this PlayerLoopStage stage) {
#if UNITY_EDITOR
            return Application.isPlaying ? _provider.Get(stage) : GetOrCreateEditorTimeSource(stage);
#endif
            return _provider.Get(stage);
        }
        
        public static ITimeSource GetUpdateStage() {
#if UNITY_EDITOR
            return Application.isPlaying ? _provider.UpdateStage() : GetOrCreateEditorTimeSource(PlayerLoopStage.Update);
#endif
            return _provider.UpdateStage();
        }

        public static ITimeSource GetFixedUpdateStage() {
#if UNITY_EDITOR
            return Application.isPlaying ? _provider.FixedUpdateStage() : GetOrCreateEditorTimeSource(PlayerLoopStage.FixedUpdate);
#endif
            return _provider.FixedUpdateStage();
        }
        
        public static void Subscribe(this PlayerLoopStage stage, IUpdate sub) {
            Get(stage).Subscribe(sub);
        }

        public static void Unsubscribe(this PlayerLoopStage stage, IUpdate sub) {
            Get(stage).Unsubscribe(sub);
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
        private static TimeSource _editorFixedUpdateTimeSource;

        private static int _editorUpdatesFrameCount;
        private static float _editorUpdatesTime;
        private static byte _editorUpdatesId;

        private static ITimeSource GetOrCreateEditorTimeSource(PlayerLoopStage stage) {
            CheckEditorUpdatesAreStarted().Forget();

            return stage switch {
                PlayerLoopStage.PreUpdate => _editorPreUpdateTimeSource ??= new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor pre update"),
                PlayerLoopStage.Update => _editorUpdateTimeSource ??= new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor update"),
                PlayerLoopStage.UnscaledUpdate => _editorUnscaledUpdateTimeSource ??= new TimeSource(_editorDeltaTimeProvider, _editorUnscaledTimeScaleProvider, "editor unscaled"),
                PlayerLoopStage.LateUpdate => _editorLateUpdateTimeSource ??= new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor late update"),
                PlayerLoopStage.FixedUpdate => _editorFixedUpdateTimeSource ??= new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider, "editor fixed update"),
                _ => throw new NotImplementedException($"Cannot create TimeSource for {nameof(PlayerLoopStage)} {stage} in edit mode")
            };
        }

        private static async UniTaskVoid CheckEditorUpdatesAreStarted() {
            if (_editorDeltaTimeProvider != null) return;
            
            byte id = _editorUpdatesId.IncrementUnchecked();
            
            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.realtimeSinceStartup;
            _editorDeltaTimeProvider = new EditorDeltaTimeProvider();
            _editorTimeScaleProvider = TimeScaleProviders.Create();
            
            while (id == _editorUpdatesId) {
                _editorDeltaTimeProvider.UpdateDeltaTime();
                _editorUpdatesTime += _editorDeltaTimeProvider.DeltaTime;
                
                _editorPreUpdateTimeSource?.Tick();
                _editorFixedUpdateTimeSource?.Tick();
                _editorUpdateTimeSource?.Tick();
                _editorUnscaledUpdateTimeSource?.Tick();
                _editorLateUpdateTimeSource?.Tick();

                await UniTask.Yield();

                _editorUpdatesFrameCount++;
                
                bool canStopEditorUpdates =
                    CanStopUpdate(_editorPreUpdateTimeSource) &&
                    CanStopUpdate(_editorFixedUpdateTimeSource) &&
                    CanStopUpdate(_editorUpdateTimeSource) &&
                    CanStopUpdate(_editorUnscaledUpdateTimeSource) &&
                    CanStopUpdate(_editorLateUpdateTimeSource);

                if (canStopEditorUpdates) break;
            }

            if (id != _editorUpdatesId) return;
            
            _editorDeltaTimeProvider = null;
            _editorTimeScaleProvider = null;
            
            _editorPreUpdateTimeSource = null;
            _editorUpdateTimeSource = null;
            _editorUnscaledUpdateTimeSource = null;
            _editorLateUpdateTimeSource = null;
            _editorFixedUpdateTimeSource = null;

            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.realtimeSinceStartup;
        }

        private static bool CanStopUpdate(ITimeSourceApi timeSource) {
            return timeSource is not { SubscribersCount: > 0 };
        }

        private sealed class EditorDeltaTimeProvider : IDeltaTimeProvider {

            public float DeltaTime { get; private set; }

            private double _lastTimeSinceStartup = Time.realtimeSinceStartup;

            public void UpdateDeltaTime() {
                double timeSinceStartup = Time.realtimeSinceStartupAsDouble;
                DeltaTime = Mathf.Max(0f, (float) (timeSinceStartup - _lastTimeSinceStartup));
                
                _lastTimeSinceStartup = timeSinceStartup;
            }
        }
#endif
    }

}
