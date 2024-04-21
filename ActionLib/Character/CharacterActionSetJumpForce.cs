﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Jump;
using UnityEngine;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionSetJumpForce : IActorAction {

        [Min(0f)] public float jumpForce = 1f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context.GetComponent<ICharacterJumpPipeline>().Force = jumpForce;
            return default;
        }
    }

}
