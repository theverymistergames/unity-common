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

        public void Initialize(MonoBehaviour owner) {
            _tasks = new UniTask[_tweens.Length];

            for (int i = 0; i < _tweens.Length; i++) {
                _tweens[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _tweens.Length; i++) {
                _tweens[i].DeInitialize();
            }
        }

        public async UniTask Play(CancellationToken token) {
            if (_tweens.Length == 0) return;

            for (int i = 0; i < _tweens.Length; i++) {
                _tasks[i] = _tweens[i].Play(token);
            }

            await UniTask.WhenAll(_tasks);
        }

        public void Wind(bool reportProgress = true) {
            for (int i = 0; i < _tweens.Length; i++) {
                _tweens[i].Wind(reportProgress);
            }
        }

        public void Rewind(bool reportProgress = true) {
            for (int i = 0; i < _tweens.Length; i++) {
                _tweens[i].Rewind(reportProgress);
            }
        }

        public void Invert(bool isInverted) {
            for (int i = 0; i < _tweens.Length; i++) {
                _tweens[i].Invert(isInverted);
            }
        }
    }

}
