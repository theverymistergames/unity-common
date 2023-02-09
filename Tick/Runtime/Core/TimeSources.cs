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
            return Application.isPlaying ? _provider.Get(stage) : GetOrCreateEditorTimeSource();
#endif
            return _provider.Get(stage);
        }

        private static ITimeSourceProvider _provider;

        public static void InjectProvider(ITimeSourceProvider provider) {
            _provider = provider;
        }

#if UNITY_EDITOR
        private static TimeSource _editorTimeSource;
        private static EditorDeltaTimeProvider _editorDeltaTimeProvider;

        private static ITimeSource GetOrCreateEditorTimeSource() {
            if (_editorTimeSource != null) return _editorTimeSource;

            _editorDeltaTimeProvider = new EditorDeltaTimeProvider();
            _editorTimeSource = new TimeSource(_editorDeltaTimeProvider, TimeScaleProviders.Create());

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            return _editorTimeSource;
        }

        private static void OnEditorUpdate() {
            _editorDeltaTimeProvider.UpdateDeltaTime();
            _editorTimeSource.Tick();
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
