using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace MisterGames.Tick.Core {

    public interface ITimeSourceProvider {
        ITimeSource Get(PlayerLoopStage stage);
    }

    public static class TimeSources {

        public static ITimeSource Get(PlayerLoopStage stage) {
#if UNITY_EDITOR
            return Application.isPlaying ? _provider.Get(stage) : GetOrCreateEditorTimeSource(stage);
#endif
            return _provider.Get(stage);
        }

        private static ITimeSourceProvider _provider;

        public static void InjectProvider(ITimeSourceProvider provider) {
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

        private static ITimeSource GetOrCreateEditorTimeSource(PlayerLoopStage stage) {
            _editorDeltaTimeProvider ??= new EditorDeltaTimeProvider();

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            switch (stage) {
                case PlayerLoopStage.PreUpdate:
                    if (_editorPreUpdateTimeSource != null) return _editorPreUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorPreUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider);

                    return _editorPreUpdateTimeSource;

                case PlayerLoopStage.Update:
                    if (_editorUpdateTimeSource != null) return _editorUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider);

                    return _editorUpdateTimeSource;

                case PlayerLoopStage.UnscaledUpdate:
                    if (_editorUnscaledUpdateTimeSource != null) return _editorUnscaledUpdateTimeSource;

                    _editorUnscaledTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorUnscaledUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorUnscaledTimeScaleProvider);

                    return _editorUnscaledUpdateTimeSource;

                case PlayerLoopStage.LateUpdate:
                    if (_editorLateUpdateTimeSource != null) return _editorLateUpdateTimeSource;

                    _editorTimeScaleProvider ??= TimeScaleProviders.Create();
                    _editorLateUpdateTimeSource = new TimeSource(_editorDeltaTimeProvider, _editorTimeScaleProvider);

                    return _editorLateUpdateTimeSource;

                default:
                    throw new NotImplementedException($"Cannot create TimeSource for {nameof(PlayerLoopStage)} {stage} in edit mode");
            }
        }

        private static void OnEditorUpdate() {
            _editorDeltaTimeProvider.UpdateDeltaTime();

            _editorPreUpdateTimeSource?.Tick();
            _editorUpdateTimeSource?.Tick();
            _editorUnscaledUpdateTimeSource?.Tick();
            _editorLateUpdateTimeSource?.Tick();
        }

        private sealed class EditorDeltaTimeProvider : IDeltaTimeProvider {

            public float DeltaTime => _deltaTime;

            private float _deltaTime;
            private float _lastTimeSinceStartup;

            public EditorDeltaTimeProvider() {
                _lastTimeSinceStartup = Time.realtimeSinceStartup;
            }

            public void UpdateDeltaTime() {
                float timeSinceStartup = Time.realtimeSinceStartup;
                _deltaTime = timeSinceStartup - _lastTimeSinceStartup;
                _lastTimeSinceStartup = timeSinceStartup;
            }
        }
#endif
    }

}
