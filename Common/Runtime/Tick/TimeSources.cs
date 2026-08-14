using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public static class TimeSources {

        private static TimeSource _timeSource;
        
        public static float deltaTime => GetTimeSource().DeltaTime;
        public static float unscaledDeltaTime => GetTimeSource().UnscaledDeltaTime;
        public static float fixedDeltaTime => GetTimeSource().FixedDeltaTime;
        public static float fixedUnscaledDeltaTime => GetTimeSource().FixedUnscaledDeltaTime;
        public static float scaledTime => GetTimeSource().ScaledTime;
        public static float unscaledTime => GetTimeSource().UnscaledTime;
        public static bool isAppFocused => Application.isFocused;
        public static bool isAppPaused => GetTimeSource().IsAppPaused;

        public static int frameCount {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    CheckEditorUpdatesAreStarted().Forget();
                    return _editorUpdatesFrameCount;   
                }
#endif
                return Time.frameCount;
            }
        }
        
        public static void Subscribe(this PlayerLoopStage stage, IUpdate sub) {
            GetTimeSource().Subscribe(sub, stage);
        }

        public static void Unsubscribe(this PlayerLoopStage stage, IUpdate sub) { 
            GetTimeSource().Unsubscribe(sub);
        }

        public static void Unsubscribe(IUpdate sub) { 
            GetTimeSource().Unsubscribe(sub);
        }
        
        internal static void InjectTimeSource(TimeSource timeSource) {
            _timeSource = timeSource;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSource GetTimeSource() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                CheckEditorUpdatesAreStarted().Forget();
                return _editorTimeSource;
            }
#endif
            
            return _timeSource;
        }
        
#if UNITY_EDITOR
        private static TimeSource _editorTimeSource;

        private static int _editorUpdatesFrameCount;
        private static float _editorUpdatesTime;
        private static byte _editorUpdatesId;

        private static async UniTaskVoid CheckEditorUpdatesAreStarted() {
            if (_editorTimeSource != null) return;
            
            byte id = _editorUpdatesId.IncrementUncheckedRef();
            
            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.realtimeSinceStartup;
            _editorTimeSource = new TimeSource();
            
            while (id == _editorUpdatesId) {
                float dt = Time.realtimeSinceStartup - _editorUpdatesTime;
                _editorUpdatesTime = Time.realtimeSinceStartup;
                
                _editorTimeSource.TickUpdate(dt, dt);
                _editorTimeSource.TickLateUpdate(dt, dt);
                _editorTimeSource.TickFixedUpdate(dt, dt);

                await UniTask.Yield();

                _editorUpdatesFrameCount++;

                if (_editorTimeSource.SubscribersCount <= 0) break;
            }

            if (id != _editorUpdatesId) return;

            _editorTimeSource = null;
            _editorUpdatesFrameCount = Time.frameCount;
            _editorUpdatesTime = Time.realtimeSinceStartup;
        }
#endif
    }

}
