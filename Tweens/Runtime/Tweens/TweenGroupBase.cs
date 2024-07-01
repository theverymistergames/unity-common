using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public abstract class TweenGroupBase<TContext, TTween> : ITween<TContext> where TTween : class, ITween<TContext> {

        public ExecuteMode mode;
        [SerializeReference] [SubclassSelector] public List<TTween> tweens;

        public float Duration { get; private set; }

        public void CreateNextDuration() {
            Duration = TweenExtensions.CreateNextDurationGroup(mode, tweens);
        }

        public abstract UniTask Play(
            TContext context,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken = default
        );
    }
    
}