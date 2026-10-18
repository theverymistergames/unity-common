using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionPlayMocapClip : IActorAction {
        
        public MotionCaptureClip motionCaptureClip;
        public bool applyPosition;
        public bool applyRotation;
        [MinMaxSlider(0f, 1f)] public Vector2 _crop = new(0f, 1f);
        [Min(0f)] public float blendIn = 0f;
        [Min(0f)] public float blendOut = 0f;

        private CharacterViewPipeline _view;
        private CharacterMotionPipeline _motion;
        private float _totalDuration;
        private float _croppedDuration;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Quaternion _startRotation;
        private Quaternion _appliedRotation;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            _view = context.GetComponent<CharacterViewPipeline>();
            _motion = context.GetComponent<CharacterMotionPipeline>();
            
            _totalDuration = motionCaptureClip.Duration;
            _croppedDuration = (_crop.y - _crop.x) * _totalDuration;

            _startRotation = _view.HeadRotation;
            _appliedRotation = Quaternion.identity;
            _lastRotation = motionCaptureClip.EvaluateRotation(_crop.x);
            _lastPosition = motionCaptureClip.EvaluatePosition(_crop.x);
            
            return TweenExtensions.Play(
                this,
                _croppedDuration,
                progressCallback: (t, p, _) => t.OnProgressUpdate(p),
                progressModifier: null,
                0f,
                1f,
                cancellationToken
            );
        }

        private void OnProgressUpdate(float progress) {
            float t = _totalDuration > 0f ? (_crop.x * _totalDuration + progress * _croppedDuration) / _totalDuration : 0f;
            float weight = GetBlendWeight(progress * _croppedDuration);

            if (applyPosition) {
                var lastPos = _lastPosition;
                _lastPosition = motionCaptureClip.EvaluatePosition(t);
                _motion.Move((_lastPosition - lastPos) * weight);
            }

            if (applyRotation) {
                var lastRot = _lastRotation;
                _lastRotation = motionCaptureClip.EvaluateRotation(t);

                var delta = Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(lastRot) * _lastRotation, weight);
                _appliedRotation *= delta;
                
                _view.SetViewOrientation(_startRotation * _appliedRotation);
                _view.HeadRotation *= delta;
            }
        }
        
        private float GetBlendWeight(float time) {
            float blendInWeight = blendIn > 0f ? Mathf.Clamp01(time / blendIn) : 1f;
            float blendOutWeight = blendOut > 0f ? Mathf.Clamp01((_croppedDuration - time) / blendOut) : 1f;
            return Mathf.SmoothStep(0f, 1f, Mathf.Min(blendInWeight, blendOutWeight));
        }
    }
    
}