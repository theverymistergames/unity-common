using System;
using MisterGames.Character.Core2.Modifiers;

namespace MisterGames.Character.Core2.Jump {

    [Serializable]
    public sealed class CharacterActionResetJumpForceMultiplier : ICharacterAction {

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.JumpPipeline.ResetForceMultiplier(source);
        }
    }

}
