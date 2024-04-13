using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Jump;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionJumpReaction : ICharacterAction {

        public Case[] cases;

        [Serializable]
        public struct Case {
            public float minMagnitude;
            public float maxMagnitude;

            [SubclassSelector]
            [SerializeReference] public ICharacterAction action;
        }

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var _jump = characterAccess.GetPipeline<ICharacterJumpPipeline>();

            float sqrMagnitude = _jump.LastJumpImpulse.sqrMagnitude;

            for (int i = 0; i < cases.Length; i++) {
                var c = cases[i];
                if (c.minMagnitude * c.minMagnitude <= sqrMagnitude &&
                    sqrMagnitude < c.maxMagnitude * c.maxMagnitude
                ) {
                    return c.action.Apply(characterAccess, cancellationToken);
                }
            }

            return default;
        }
    }

}
