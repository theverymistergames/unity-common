using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedMultiplier : IAsyncAction, IDependency {

        [Min(0f)] public float speed;

        private CharacterProcessorVector2Multiplier _multiplier;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<ICharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _multiplier = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorVector2Multiplier>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _multiplier.multiplier = speed;
            return default;
        }
    }
    
}
