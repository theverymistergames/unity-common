using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class InstantTween : ITween {

        [SerializeReference] [SubclassSelector] public ITweenInstantAction action;

        public float Progress => _progress;

        private int _progress;
        private int _progressDirection = 1;

        public void Initialize(MonoBehaviour owner) {
            action.Initialize(owner);
        }

        public void DeInitialize() {
            action.DeInitialize();
        }

        public UniTask Play(CancellationToken token) {
            if (HasReachedTargetProgress()) return default;

            action.InvokeAction();

            _progress = Math.Clamp(_progress + _progressDirection, 0, 1);

            return default;
        }

        public void Wind(bool reportProgress = true) {
            _progress = 1;
        }

        public void Rewind(bool reportProgress = true) {
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
