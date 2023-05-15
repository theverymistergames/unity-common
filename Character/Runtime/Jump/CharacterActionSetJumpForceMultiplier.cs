using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Jump {

    [Serializable]
    public sealed class CharacterActionSetJumpForceMultiplier : ICharacterAction {

        [Min(0f)] public float jumpForceMultiplier = 1f;

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterJumpPipeline>().ForceMultiplier = jumpForceMultiplier;
            return default;
        }
    }

}
