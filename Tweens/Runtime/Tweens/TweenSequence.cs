using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenSequence : ITween {

        [SerializeReference] [SubclassSelector] public List<ITween> tweens;
        public bool loop;
        public bool yoyo;

        public float Progress {
            get {
                int count = tweens.Count;
                if (count == 0) return 1f;

                float sum = 0f;
                for (int i = 0; i < count; i++) {
                    sum += tweens[i].Progress;
                }
                return sum / count;
            }
        }

        private bool _isInverted;
        private int _currentTweenIndex;

        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < tweens.Count; i++) {
                tweens[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (tweens.Count == 0) return;

            while (!token.IsCancellationRequested) {
                var tween = tweens[_currentTweenIndex];

                if (loop && !_isInverted) tween.Rewind(reportProgress: false);
                tween.Invert(_isInverted);

                await tween.Play(token);

                if (token.IsCancellationRequested) break;

                if (_isInverted) {
                    if (_currentTweenIndex - 1 >= 0) {
                        _currentTweenIndex--;
                        continue;
                    }

                    if (loop) {
                        _isInverted = false;
                        continue;
                    }

                    break;
                }

                if (_currentTweenIndex + 1 < tweens.Count) {
                    _currentTweenIndex++;
                    continue;
                }

                if (yoyo) {
                    _isInverted = true;
                    continue;
                }

                if (loop) {
                    _currentTweenIndex = 0;
                    continue;
                }

                break;
            }
        }

        public void Wind(bool reportProgress = true) {
            if (tweens.Count == 0) return;

            for (int i = _currentTweenIndex; i < tweens.Count; i++) {
                tweens[i].Wind(reportProgress);
            }

            _currentTweenIndex = tweens.Count - 1;
        }

        public void Rewind(bool reportProgress = true) {
            if (tweens.Count == 0) return;

            for (int i = _currentTweenIndex; i >= 0; i--) {
                tweens[i].Rewind(reportProgress);
            }

            _currentTweenIndex = 0;
        }

        public void Invert(bool isInverted) {
            if (tweens.Count == 0) return;

            tweens[_currentTweenIndex].Invert(isInverted);
            _isInverted = isInverted;
        }
    }

}
