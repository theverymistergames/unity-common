using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionPlayMocapClip : IActorAction {
        
        public MotionCaptureClip motionCaptureClip;
        public bool applyPosition;
        public bool applyRotation;
        [MinMaxSlider(0f, 1f)] public Vector2 _crop = new Vector2(0f, 1f);

        private CharacterViewPipeline _view;
        private ITransformAdapter _bodyAdapter;
        private float _totalDuration;
        private float _croppedDuration;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Quaternion _startRotation;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            _view = context.GetComponent<CharacterViewPipeline>();
            _bodyAdapter = context.GetComponent<CharacterBodyAdapter>();
            
            _totalDuration = motionCaptureClip.Duration;
            _croppedDuration = (_crop.y - _crop.x) * _totalDuration;

            _startRotation = _view.Rotation;
            _lastRotation = Quaternion.identity;
            _lastPosition = Vector3.zero;
            
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

            if (applyPosition) {
                var lastPos = _lastPosition;
                _lastPosition = motionCaptureClip.EvaluatePosition(t);
                _bodyAdapter.Move(_lastPosition - lastPos);
            }

            if (applyRotation) {
                var lastRot = _lastRotation;
                _lastRotation = motionCaptureClip.EvaluateRotation(t);

                var relativeRot = _startRotation * _lastRotation;
                _view.SetClampCenter(relativeRot);
                
                _view.Rotation *= _lastRotation * Quaternion.Inverse(lastRot);
            }
        }
    }
    
}