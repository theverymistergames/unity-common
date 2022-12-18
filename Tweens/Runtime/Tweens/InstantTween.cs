using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class InstantTween : ITween {

        [SerializeReference] [SubclassSelector]
        private ITweenInstantAction[] _actions;

        private int _progress;
        private int _progressDirection = 1;

        public void Initialize(MonoBehaviour owner) {
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].Initialize(owner);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].DeInitialize();
            }
        }

        public UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) {
                return default;
            }

            for (int i = 0; i < _actions.Length; i++) {
                _actions[i].InvokeAction();
            }

            _progress = Math.Clamp(_progress + _progressDirection, 0, 1);

            return default;
        }

        public void Wind() {
            _progress = 1;
        }

        public void Rewind() {
            _progress = 0;
        }

        public void ResetProgress() {
            _progress = 0;
        }

        public void Invert(bool isInverted) {
            _progressDirection = isInverted ? -1 : 1;
        }

        private bool HasReachedTargetProgress() {
            return _progressDirection > 0 && _progress >= 1 ||
                   _progressDirection < 0 && _progress <= 0;
        }
    }

}
