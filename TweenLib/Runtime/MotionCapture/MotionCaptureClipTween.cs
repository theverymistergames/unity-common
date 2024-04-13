using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.MotionCapture {
    
    [Serializable]
    public sealed class MotionCaptureClipTween : ITween {
        
        public Transform target;
        public MotionCaptureClip motionCaptureClip;
        public bool applyPosition;
        public bool applyRotation;
        [MinMaxSlider(0f, 1f, show: true)] public Vector2 _crop = new Vector2(0f, 1f);
        
        public float Duration { get; private set; }

        private float _totalDuration;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        public void CreateNextDuration() {
            _totalDuration = motionCaptureClip.Duration;
            Duration = (_crop.y - _crop.x) * _totalDuration;
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            _initialPosition = target.localPosition;
            _initialRotation = target.localRotation;
            
            return TweenExtensions.Play(
                this,
                duration,
                progressCallback: (t, p) => t.OnProgressUpdate(p),
                startProgress,
                speed,
                PlayerLoopStage.Update,
                cancellationToken
            );
        }

        private void OnProgressUpdate(float progress) {
            float t = _totalDuration > 0f ? (_crop.x * _totalDuration + progress * Duration) / _totalDuration : 0f;
            
            if (applyPosition) target.localPosition = _initialPosition + motionCaptureClip.EvaluatePosition(t);
            if (applyRotation) target.localRotation = _initialRotation * motionCaptureClip.EvaluateRotation(t);
        }
    }
    
}