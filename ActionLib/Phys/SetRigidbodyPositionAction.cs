using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class SetRigidbodyPositionAction : IActorAction {

        public Rigidbody rigidbody;
        public Transform position;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            position.GetPositionAndRotation(out var pos, out var rot);
            rigidbody.position = pos;
            rigidbody.rotation = rot;
            rigidbody.transform.SetPositionAndRotation(pos, rot);
            return default;
        }
    }
    
}