using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetMassSettings : ICharacterAction {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = 20f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 10f;
        [Min(0.001f)] public float groundInertialFactor = 20f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var mass = characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>();
            if (mass == null) return;

            mass.gravityForce = gravityForce;
            mass.airInertialFactor = airInertialFactor;
            mass.groundInertialFactor = groundInertialFactor;
        }
    }
    
}
