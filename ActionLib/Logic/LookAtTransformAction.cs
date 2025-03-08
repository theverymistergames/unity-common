using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Logic.Transforms;
using UnityEngine;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class LookAtTransformAction : IActorAction {
        
        public LookAtBehaviour lookAtBehaviour;
        public Transform target;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            lookAtBehaviour.LookAt(target);
            return default;
        }
    }
    
}