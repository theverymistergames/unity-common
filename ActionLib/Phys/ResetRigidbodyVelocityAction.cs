using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class ResetRigidbodyVelocityAction : IActorAction {

        public Rigidbody rigidbody;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            rigidbody.linearVelocity = default;
            rigidbody.angularVelocity = default;
            return default;
        }
    }
    
}