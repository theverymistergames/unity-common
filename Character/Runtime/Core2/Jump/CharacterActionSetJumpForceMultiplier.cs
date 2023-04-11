﻿using System;
using MisterGames.Character.Core2.Modifiers;
using UnityEngine;

namespace MisterGames.Character.Core2.Jump {

    [Serializable]
    public sealed class CharacterActionSetJumpForceMultiplier : ICharacterAction {

        [Min(0f)] public float jumpForceMultiplier = 1f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.JumpPipeline.SetForceMultiplier(source, jumpForceMultiplier);
        }
    }

}