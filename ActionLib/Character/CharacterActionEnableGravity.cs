using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableGravity : IActorAction {

        public bool useGravity;
        public bool isKinematic;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var rb = context.GetComponent<Rigidbody>();
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            return default;
        }
    }
    
}
