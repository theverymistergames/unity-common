using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetMassSettings : ICharacterAction {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = 15f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 10f;
        [Min(0.001f)] public float groundInertialFactor = 20f;
        [Min(0f)] public float inputInfluenceFactor = 1f;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var mass = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();

            mass.gravityForce = gravityForce;
            mass.airInertialFactor = airInertialFactor;
            mass.groundInertialFactor = groundInertialFactor;
            mass.inputInfluenceFactor = inputInfluenceFactor;

            return default;
        }
    }
    
}
