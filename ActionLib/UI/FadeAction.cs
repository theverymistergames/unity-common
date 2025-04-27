using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using MisterGames.UI.Services;
using UnityEngine;

namespace MisterGames.ActionLib.UI {
    
    [Serializable]
    public sealed class FadeAction : IActorAction {

        public FadeMode mode;
        public Optional<float> duration;
        public Optional<AnimationCurve> curve;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return Fader.Main?.FadeAsync(mode, duration.HasValue ? duration.Value : -1f, curve.HasValue ? curve.Value : null) ?? default;
        }
    }
    
}