using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableMotionGraph : IActorAction {

        public bool isEnabled;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<ICharacterMotionGraphPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
