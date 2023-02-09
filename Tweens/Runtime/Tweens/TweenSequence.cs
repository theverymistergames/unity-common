using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenSequence : ITween {

        public bool loop;
        public bool yoyo;
        [SerializeReference] [SubclassSelector] public ITween[] tweens;

        private bool _isInverted;
        private int _currentTweenIndex;

        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < tweens.Length; i++) {
                tweens[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < tweens.Length; i++) {
                tweens[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (tweens.Length == 0) return;

            while (!token.IsCancellationRequested) {
                var tween = tweens[_currentTweenIndex];
                tween.Invert(_isInverted);

                if (loop && !_isInverted) tween.ResetProgress();

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

                if (_currentTweenIndex + 1 < tweens.Length) {
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

        public void Wind() {
            if (tweens.Length == 0) return;

            for (int i = _currentTweenIndex; i < tweens.Length; i++) {
                tweens[i].Wind();
            }

            _currentTweenIndex = tweens.Length - 1;
        }

        public void Rewind() {
            if (tweens.Length == 0) return;

            for (int i = _currentTweenIndex; i >= 0; i--) {
                tweens[i].Rewind();
            }

            _currentTweenIndex = 0;
        }

        public void Invert(bool isInverted) {
            if (tweens.Length == 0) return;

            tweens[_currentTweenIndex].Invert(isInverted);
            _isInverted = isInverted;
        }

        public void ResetProgress() {
            for (int i = 0; i < tweens.Length; i++) {
                tweens[i].ResetProgress();
            }
        }
    }

}
