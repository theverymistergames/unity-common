using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Jump {

    [Serializable]
    public sealed class CharacterActionSetJumpForceMultiplier : ICharacterAction {

        [Min(0f)] public float jumpForceMultiplier = 1f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess.GetPipeline<ICharacterJumpPipeline>().ForceMultiplier = jumpForceMultiplier;
        }
    }

}
