using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SetTransformPositionAction : IActorAction {
        
        public Transform target;
        public Vector3 position;
        public bool local = true;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (local) {
                target.localPosition = position;
            }
            else {
                target.position = position;
            }
            
            return default;
        }
    }
    
}