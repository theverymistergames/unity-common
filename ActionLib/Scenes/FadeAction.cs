using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Scenes.Loading;
using UnityEngine;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class FadeAction : IActorAction {

        [Header("Fade")]
        [SerializeField] private FadeMode _fadeMode;
        [SerializeField] [Min(-1f)] private float _duration = -1f;
        [SerializeField] private Optional<AnimationCurve> _curve = Optional<AnimationCurve>.WithDisabled(EasingType.Linear.ToAnimationCurve());

        public UniTask Apply(IActor context, CancellationToken cancellationToken) {
            return Fader.Main.FadeAsync(_fadeMode, _duration, _curve.GetOrDefault());
        }
    }
    
}