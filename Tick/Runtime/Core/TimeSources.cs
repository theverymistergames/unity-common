using System.Collections.Generic;

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
        private static readonly Dictionary<PlayerLoopStage, TimeSource> _editorTimeSourcesMap = new Dictionary<PlayerLoopStage, TimeSource>();
        private static EditorDeltaTimeProvider _editorDeltaTimeProvider;

        private static ITimeSource GetOrCreateEditorTimeSource(PlayerLoopStage stage) {
            if (_editorTimeSourcesMap.TryGetValue(stage, out var timeSource)) return timeSource;

            _editorDeltaTimeProvider ??= new EditorDeltaTimeProvider();

            timeSource = new TimeSource(_editorDeltaTimeProvider, TimeScaleProviders.Create());
            _editorTimeSourcesMap[stage] = timeSource;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            return timeSource;
        }

        private static void OnEditorUpdate() {
            _editorDeltaTimeProvider.UpdateDeltaTime();
            foreach (var timeSource in _editorTimeSourcesMap.Values) {
                timeSource.Tick();
            }
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
