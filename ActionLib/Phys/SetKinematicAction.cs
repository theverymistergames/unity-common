using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class SetKinematicAction : IActorAction {

        public bool isKinematic;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<Rigidbody>().isKinematic = isKinematic;
            return default;
        }
    }
    
}