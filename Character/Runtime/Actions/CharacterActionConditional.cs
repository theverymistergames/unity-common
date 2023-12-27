using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionConditional : ICharacterAction {

        [SerializeReference] [SubclassSelector] public ICharacterCondition condition;
        [SerializeReference] [SubclassSelector] public ICharacterAction actionOnTrue;
        [SerializeReference] [SubclassSelector] public ICharacterAction actionOnFalse;

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            var nextAction = (condition?.IsMatch(context) ?? false) ? actionOnTrue : actionOnFalse;
            return nextAction?.Apply(context, cancellationToken) ?? default;
        }
    }
}
