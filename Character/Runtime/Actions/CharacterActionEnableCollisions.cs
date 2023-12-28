using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionEnableCollisions : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterCollisionPipeline>().IsEnabled = isEnabled;
            return default;
        }
    }
    
}
