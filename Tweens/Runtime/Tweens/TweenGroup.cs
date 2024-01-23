using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenGroup : ITween {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public List<ITween> tweens;

        public enum Mode {
            Sequential,
            Parallel,
        }

        public float Duration { get; private set; }

        public void CreateNextDuration() {
            Duration = TweenExtensions.CreateNextDurationGroup(mode, tweens);
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayGroup(mode, tweens, duration, startProgress, speed, cancellationToken);
        }
    }

}
