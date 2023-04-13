using System;
using MisterGames.Character.Access;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Jump {

    [Serializable]
    public sealed class CharacterActionSetJumpForceMultiplier : ICharacterAction {

        [Min(0f)] public float jumpForceMultiplier = 1f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.JumpPipeline.ForceMultiplier = jumpForceMultiplier;
        }
    }

}
