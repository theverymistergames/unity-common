using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Jump {

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

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var _jump = characterAccess.GetPipeline<ICharacterJumpPipeline>();

            float sqrMagnitude = _jump.LastJumpImpulse.sqrMagnitude;

            for (int i = 0; i < cases.Length; i++) {
                var c = cases[i];
                if (c.minMagnitude * c.minMagnitude <= sqrMagnitude &&
                    sqrMagnitude < c.maxMagnitude * c.maxMagnitude
                ) {
                    return c.action.Apply(characterAccess, source, cancellationToken);
                }
            }

            return default;
        }
    }

}
