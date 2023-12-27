using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionSwitch : ICharacterAction {

        public Case[] cases;

        [Serializable]
        public struct Case {
            [SerializeReference] [SubclassSelector] public ICharacterCondition condition;
            [SerializeReference] [SubclassSelector] public ICharacterAction action;
        }

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < cases.Length; i++) {
                var c = cases[i];
                if (c.condition?.IsMatch(context) ?? false) {
                    return c.action?.Apply(context, cancellationToken) ?? default;
                }
            }

            return default;
        }
    }

}
