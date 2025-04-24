using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SetTransformRotationAction : IActorAction {
        
        public Transform target;
        public Vector3 rotation;
        public bool local = true;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (local) {
                target.localEulerAngles = rotation;
            }
            else {
                target.eulerAngles = rotation;
            }
            
            return default;
        }
    }
    
}