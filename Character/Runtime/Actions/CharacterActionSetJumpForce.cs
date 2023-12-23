using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Jump;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionSetJumpForce : ICharacterAction {

        [Min(0f)] public float jumpForce = 1f;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess.GetPipeline<ICharacterJumpPipeline>().Force = jumpForce;
            return default;
        }
    }

}
