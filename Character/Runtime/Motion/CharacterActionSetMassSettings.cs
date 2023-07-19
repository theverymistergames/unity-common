using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetMassSettings : IAsyncAction, IDependency {

        [Header("Gravity")]
        [Min(0f)] public float gravityForce = 15f;

        [Header("Inertia")]
        [Min(0.001f)] public float airInertialFactor = 10f;
        [Min(0.001f)] public float groundInertialFactor = 20f;
        [Min(0f)] public float inputInfluenceFactor = 1f;

        private CharacterProcessorMass _mass;
        
        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<ICharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _mass = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _mass.gravityForce = gravityForce;
            _mass.airInertialFactor = airInertialFactor;
            _mass.groundInertialFactor = groundInertialFactor;
            _mass.inputInfluenceFactor = inputInfluenceFactor;

            return default;
        }
    }
    
}
