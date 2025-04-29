using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Tweens {
    
    public sealed class TweenProgressEvaluator  : MonoBehaviour, IUpdate {
    
        [SerializeReference] [SubclassSelector] private ITweenProgress _tweenProgress;
        [SerializeField] private AnimationCurve _progressCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeReference] [SubclassSelector] private IProgressModulator _progressModulator;
        [SerializeReference] [SubclassSelector] private ITweenProgressAction _tweenProgressAction;

        private float _progress;
        private bool _force;
        
        private void OnEnable() {
            _force = true;
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float oldProgress = _progress;
            
            float p = _progressCurve.Evaluate(_tweenProgress.GetProgress());
            _progress = _progressModulator?.Modulate(p) ?? p;
            
            if (oldProgress.IsNearlyEqual(_progress) && !_force) return;

            _force = false;
            _tweenProgressAction.OnProgressUpdate(_progress);
        }
    }
    
}