using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Tweens {
    
    public sealed class TweenProgressActionContainer : MonoBehaviour, ITweenProgressAction, IUpdate {

        [SerializeField] private AnimationCurve _curve = EasingType.Linear.ToAnimationCurve();
        [SerializeReference] [SubclassSelector] private ITweenProgressAction _action;
        [SerializeField] [Min(0f)] private float _smoothing;

        private bool _enabled;
        private bool _subscribedToUpdate;
        private float _smoothProgress;
        private float _targetProgress;

        private void OnEnable() {
            _enabled = true;
        }

        private void OnDisable() {
            _enabled = false;
            _subscribedToUpdate = false;
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        public void OnProgressUpdate(float progress) {
            _targetProgress = progress;
            
            if (!_enabled || _smoothing <= 0f) {
                NotifyProgress(progress);
                return;
            }

            if (_subscribedToUpdate) return;

            _smoothProgress = progress;
            _subscribedToUpdate = true;
            PlayerLoopStage.Update.Subscribe(this);
            
            NotifyProgress(progress);
        }

        void IUpdate.OnUpdate(float dt) {
            float oldProgress = _smoothProgress;
            _smoothProgress = _smoothProgress.SmoothExpNonZero(_targetProgress, _smoothing, dt);

            NotifyProgress(oldProgress);

            if (!_smoothProgress.IsNearlyEqual(oldProgress)) return;
            
            _subscribedToUpdate = false;
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        private void NotifyProgress(float progress) {
            _action?.OnProgressUpdate(_curve?.Evaluate(progress) ?? progress);
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _applyDebugProgress;
        [SerializeField] [Range(0f, 1f)] private float _debugProgress;

        private void OnValidate() {
            if (!_applyDebugProgress) return;
            
            OnProgressUpdate(_debugProgress);
        }
#endif
    }
    
}