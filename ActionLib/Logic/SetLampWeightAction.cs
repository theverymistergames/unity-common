using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Logic.Interactives;
using UnityEngine;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class SetLampWeightAction : IActorAction {
    
        public LampBehaviour lamp;
        [Min(0f)] public float weight;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            lamp.Weight = weight;
            return default;
        }
    }
    
}