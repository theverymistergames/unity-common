using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : IAsyncAction, IDependency {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        private CharacterProcessorBackSideSpeedCorrection _correction;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<ICharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _correction = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorBackSideSpeedCorrection>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _correction.speedCorrectionBack = backCorrection;
            _correction.speedCorrectionSide = sideCorrection;

            return default;
        }
    }
    
}
