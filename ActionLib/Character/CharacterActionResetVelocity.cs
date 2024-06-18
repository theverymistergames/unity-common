using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<Rigidbody>().velocity = Vector3.zero;
            return default;
        }
    }
    
}
