using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.ActionLib.Tweens {
    
    [Serializable]
    public sealed class TweenProgressCondition : IActorCondition {

        public TweenRunner tweenRunner;
        [MinMaxSlider(0f, 1f)] public Vector2 progress;
        public Mode mode;
        
        public enum Mode {
            InRange,
            OutOfRange,
        }
        
        public bool IsMatch(IActor context) {
            float p = tweenRunner.TweenPlayer.Progress;
            return mode switch {
                Mode.InRange => p.InRange(progress),
                Mode.OutOfRange => !p.InRange(progress),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}