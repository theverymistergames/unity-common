using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionGravityEnableDisable : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var mass = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();

            mass.isGravityEnabled = isEnabled;
            return default;
        }
    }
    
}
