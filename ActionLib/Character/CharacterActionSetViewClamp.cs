using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Data;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : IActorAction {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out CharacterViewPipeline view)) return UniTask.CompletedTask;
            
            if (horizontal.HasValue) view.ApplyHorizontalClamp(horizontal.Value);
            if (vertical.HasValue) view.ApplyVerticalClamp(vertical.Value);
            
            return UniTask.CompletedTask;
        }
    }
    
}
