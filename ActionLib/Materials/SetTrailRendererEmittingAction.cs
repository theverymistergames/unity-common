using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Materials {

    [Serializable]
    public sealed class SetTrailRendererEmittingAction : IActorAction {

        public TrailRenderer[] trailRenderers;
        public bool emitting;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < trailRenderers.Length; i++) {
                trailRenderers[i].emitting = emitting;
            }
            
            return default;
        }
    }

}
