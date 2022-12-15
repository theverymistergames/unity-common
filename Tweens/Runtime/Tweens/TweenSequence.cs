using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenSequence : ITween {

        [SerializeField] private bool _loop;
        [SerializeField] private bool _yoyo;

        [SerializeReference] [SubclassSelector]
        private ITween[] _tweens;

        private bool _isInverted;
        private int _tweensCount;
        private int _currentTweenIndex;

        public void Initialize(MonoBehaviour owner) {
            _tweensCount = _tweens.Length;

            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (_tweensCount == 0) return;

            while (!token.IsCancellationRequested) {
                var tween = _tweens[_currentTweenIndex];
                tween.Invert(_isInverted);

                await tween.Play(token);

                if (token.IsCancellationRequested) break;

                if (_isInverted) {
                    if (_currentTweenIndex - 1 >= 0) {
                        _currentTweenIndex--;
                        continue;
                    }

                    if (_loop) {
                        _isInverted = false;
                        continue;
                    }

                    break;
                }

                if (_currentTweenIndex + 1 < _tweensCount) {
                    _currentTweenIndex++;
                    continue;
                }

                if (_yoyo) {
                    _isInverted = true;
                    continue;
                }

                if (_loop) {
                    _currentTweenIndex = 0;
                    continue;
                }

                break;
            }
        }

        public void Wind() {
            if (_tweensCount == 0) return;

            for (int i = _currentTweenIndex; i < _tweensCount; i++) {
                var tween = _tweens[i];
                tween.Wind();
            }
            _currentTweenIndex = _tweensCount - 1;
        }

        public void Rewind() {
            if (_tweensCount == 0) return;

            for (int i = _currentTweenIndex; i >= 0; i--) {
                var tween = _tweens[i];
                tween.Rewind();
            }
            _currentTweenIndex = 0;
        }

        public void Invert(bool isInverted) {
            if (_tweensCount == 0) return;

            _tweens[_currentTweenIndex].Invert(isInverted);
            _isInverted = isInverted;
        }
    }

}
