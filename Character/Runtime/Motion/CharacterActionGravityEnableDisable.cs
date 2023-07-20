﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionGravityEnableDisable : IAsyncAction, IDependency {

        public bool isEnabled;

        private CharacterProcessorMass _mass;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _mass = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _mass.isGravityEnabled = isEnabled;
            return default;
        }
    }
    
}
