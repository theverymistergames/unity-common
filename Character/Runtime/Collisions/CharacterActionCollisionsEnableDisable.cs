using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Collisions {
    
    [Serializable]
    public sealed class CharacterActionCollisionsEnableDisable : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterCollisionPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
