using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character;
using MisterGames.Character.Motion;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionEnableGravity : IActorAction {

        public bool isEnabled;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>()
                .isGravityEnabled = isEnabled;
            
            return default;
        }
    }
    
}
