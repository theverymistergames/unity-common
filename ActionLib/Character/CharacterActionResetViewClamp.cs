using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionResetViewClamp : IActorAction {

        public bool horizontal = true;
        public bool vertical = true;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            
            if (horizontal) view.ResetHorizontalClamp();
            if (vertical) view.ResetVerticalClamp();
            
            return default;
        }
    }
    
}
