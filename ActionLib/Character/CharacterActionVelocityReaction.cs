using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionVelocityReaction : IActorAction {

        public Case[] cases;
        
        [Serializable]
        public struct Case {
            public float minMagnitude;
            public float maxMagnitude;

            [SubclassSelector]
            [SerializeReference] public IActorAction action;
        }

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var motion = context.GetComponent<CharacterMotionPipeline>();

            float sqrMagnitude = motion.PreviousVelocity.sqrMagnitude;

            for (int i = 0; i < cases.Length; i++) {
                var c = cases[i];
                if (c.minMagnitude * c.minMagnitude <= sqrMagnitude &&
                    sqrMagnitude < c.maxMagnitude * c.maxMagnitude
                ) {
                    return c.action.Apply(context, cancellationToken);
                }
            }

            return default;
        }
    }

}
