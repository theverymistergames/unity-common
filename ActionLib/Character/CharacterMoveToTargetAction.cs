using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveToTargetAction : IActorAction {

        public Transform target;
        public Vector3 localOffset;
        public float duration;
        public AnimationCurve durationByDi;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return default;
        }
    }
    
}