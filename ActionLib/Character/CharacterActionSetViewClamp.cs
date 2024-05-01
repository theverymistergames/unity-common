using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : IActorAction {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var clamp = context.GetComponent<ICharacterViewPipeline>().GetProcessor<CharacterProcessorViewClamp>();

            if (horizontal.HasValue) clamp.ApplyHorizontalClamp(horizontal.Value);
            if (vertical.HasValue) clamp.ApplyVerticalClamp(vertical.Value);
            
            return default;
        }
    }
    
}
