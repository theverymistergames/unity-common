using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetMassSettings : IActorAction {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = 15f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 10f;
        [Min(0.001f)] public float groundInertialFactor = 20f;
        [Min(0f)] public float inputInfluenceFactor = 1f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var mass = context.GetComponent<CharacterMotionPipeline>().GetProcessor<CharacterMassProcessor>();

            mass.gravityForce = gravityForce;
            mass.airInertialFactor = airInertialFactor;
            mass.groundInertialFactor = groundInertialFactor;
            mass.inputInfluenceFactor = inputInfluenceFactor;

            return default;
        }
    }
    
}
