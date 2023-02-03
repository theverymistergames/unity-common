using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenParallel : ITween {

        [SerializeReference] [SubclassSelector] private ITween[] _tweens;

        private UniTask[] _tasks;
        private int _tweensCount;

        public void Initialize(MonoBehaviour owner) {
            _tweensCount = _tweens.Length;
            _tasks = new UniTask[_tweensCount];

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

            for (int i = 0; i < _tweensCount; i++) {
                _tasks[i] = _tweens[i].Play(token);
            }

            await UniTask.WhenAll(_tasks);
        }

        public void Wind() {
            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].Wind();
            }
        }

        public void Rewind() {
            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].Rewind();
            }
        }

        public void Invert(bool isInverted) {
            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].Invert(isInverted);
            }
        }

        public void ResetProgress() {
            for (int i = 0; i < _tweensCount; i++) {
                _tweens[i].ResetProgress();
            }
        }
    }

}
