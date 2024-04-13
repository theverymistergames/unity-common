using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionPlayMocapClip : ICharacterAction {
        
        public MotionCaptureClip motionCaptureClip;
        public bool applyPosition;
        public bool applyRotation;
        [MinMaxSlider(0f, 1f, show: true)] public Vector2 _crop = new Vector2(0f, 1f);

        private ICharacterAccess _characterAccess;
        private float _totalDuration;
        private float _croppedDuration;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            _characterAccess = context;
            _totalDuration = motionCaptureClip.Duration;
            _croppedDuration = (_crop.y - _crop.x) * _totalDuration;
            _lastRotation = Quaternion.identity;
            _lastPosition = Vector3.zero;
            
            return TweenExtensions.Play(
                this,
                _croppedDuration,
                progressCallback: (t, p) => t.OnProgressUpdate(p),
                0f,
                1f,
                PlayerLoopStage.Update,
                cancellationToken
            );
        }

        private void OnProgressUpdate(float progress) {
            float t = _totalDuration > 0f ? (_crop.x * _totalDuration + progress * _croppedDuration) / _totalDuration : 0f;

            if (applyPosition) {
                var lastPos = _lastPosition;
                _lastPosition = motionCaptureClip.EvaluatePosition(t);
                _characterAccess.BodyAdapter.Move(_lastPosition - lastPos);
            }

            if (applyRotation) {
                var lastRot = _lastRotation;
                _lastRotation = motionCaptureClip.EvaluateRotation(t);

                var diff = _lastRotation.eulerAngles - lastRot.eulerAngles;
                
                _characterAccess.HeadAdapter.Rotate(Quaternion.Euler(diff.x, 0f, 0f));
                _characterAccess.BodyAdapter.Rotate(Quaternion.Euler(0f, diff.y, 0f));
            }
        }
    }
    
}