using System;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Collisions {
    
    [Serializable]
    public sealed class CharacterActionCollisionsEnableDisable : ICharacterAction {

        public bool isEnabled;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.GetPipeline<ICharacterCollisionPipeline>().SetEnabled(isEnabled);
        }
    }
    
}
