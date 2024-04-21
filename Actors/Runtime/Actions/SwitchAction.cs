using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors.Actions {

    [Serializable]
    public sealed class SwitchAction : IActorAction {

        public Case[] cases;

        [Serializable]
        public struct Case {
            [SerializeReference] [SubclassSelector] public IActorCondition condition;
            [SerializeReference] [SubclassSelector] public IActorAction action;
        }

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
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
