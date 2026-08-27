using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Logic.Weather;
using UnityEngine;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class PerformLightningAction : IActorAction {

        public LightningController lightningController;
        [Min(-1f)] public float thunderDelay = -1f;
        public AudioClip[] thunderSounds;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return lightningController.PerformLightning(thunderDelay, thunderSounds, cancellationToken);
        }
    }
    
}