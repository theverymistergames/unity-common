using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Colors;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Scenes.Loading {
    
    [DefaultExecutionOrder(-200_000)]
    public sealed class Fader : MonoBehaviour, IFader {

        [SerializeField] private bool _mainFader = true;
        [SerializeField] private Image _image;
        [SerializeField] private Color _defaultColor = Color.black;
        [SerializeField] [Min(0f)] private float _defaultFadeInDuration = 0.5f;
        [SerializeField] [Min(0f)] private float _defaultFadeOutDuration = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _curveMatchingTransition = 0.5f;
        [SerializeField] private AnimationCurve _fadeInCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _fadeOutCurve = EasingType.Linear.ToAnimationCurve();

        public static IFader Main { get; private set; }
        
        public float Progress { get; private set; }

        private CancellationToken _destroyToken;
        private byte _fadeId;

        private void Awake() {
            _destroyToken = destroyCancellationToken;
            
            if (_mainFader) Main = this;
            
            _image.color = _defaultColor.WithAlpha(0f);
        }

        private void OnDestroy() {
            if (_mainFader) Main = null;
        }

        public void Fade(FadeMode mode, float duration = -1f, AnimationCurve curve = null) {
            FadeAsync(mode, duration, curve).Forget();
        }

        public void FadeIn(float duration = -1f, AnimationCurve curve = null) {
            FadeInAsync(duration, curve).Forget();
        }

        public void FadeOut(float duration = -1f, AnimationCurve curve = null) {
            FadeOutAsync(duration, curve).Forget();
        }

        public UniTask FadeAsync(FadeMode mode, float duration = -1f, AnimationCurve curve = null) {
            return mode switch {
                FadeMode.FadeIn => FadeInAsync(duration, curve),
                FadeMode.FadeOut => FadeOutAsync(duration, curve),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public UniTask FadeInAsync(float duration = -1f, AnimationCurve curve = null) {
            return Fade(
                progressEnd: 1f,
                duration: duration >= 0f ? duration : _defaultFadeInDuration,
                curve ?? _fadeInCurve, 
                _destroyToken
            );
        }

        public UniTask FadeOutAsync(float duration = -1f, AnimationCurve curve = null) {
            return Fade(
                progressEnd: 0f,
                duration: duration >= 0f ? duration : _defaultFadeOutDuration, 
                curve ?? _fadeOutCurve, 
                _destroyToken
            );
        }
        
        private async UniTask Fade(float progressEnd, float duration, AnimationCurve curve, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) return;
            
            byte id = ++_fadeId;

            float dir;
            float min;
            float max;

            if (Progress <= progressEnd) {
                dir = 1f;
                min = 0f;
                max = Mathf.Min(1f, progressEnd);
            }
            else {
                dir = -1f;
                min = Mathf.Max(0f, progressEnd);
                max = 1f;
            }

            var color = _image.color;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float progressStart = Progress;
            float alphaDiff = color.a - curve.Evaluate(Progress);

            while (id == _fadeId && !cancellationToken.IsCancellationRequested && 
                   (dir > 0f && Progress < max || dir < 0f && Progress > min)) 
            {
                Progress = Mathf.Clamp(Progress + Time.deltaTime * speed * dir, min, max);
                
                float t = progressStart.IsNearlyEqual(progressEnd) 
                    ? 1f 
                    : (Progress - progressStart) / (progressEnd - progressStart);
                
                float curveMatch = _curveMatchingTransition <= 0f 
                    ? 1f
                    : Mathf.Clamp01(t / _curveMatchingTransition);

                float transitionAlpha = Mathf.Lerp(alphaDiff, 0f, curveMatch);

                _image.color = color.WithAlpha(curve.Evaluate(Progress) + transitionAlpha);
                
                await UniTask.Yield();
            }
        }
    }
    
}