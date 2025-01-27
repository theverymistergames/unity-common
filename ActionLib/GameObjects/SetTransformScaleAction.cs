using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SetTransformScaleAction : IActorAction {
        
        public Transform target;
        public Vector3 scale = Vector3.one;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            target.localScale = scale;
            return default;
        }
    }
    
}