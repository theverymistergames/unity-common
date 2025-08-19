using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Libs {
    
    [Serializable]
    public sealed class UseLabelContextAction : IActorAction {

        public LabelValue<IActor> context;
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return action?.Apply(this.context.GetData(), cancellationToken) ?? default;
        }
    }
    
}