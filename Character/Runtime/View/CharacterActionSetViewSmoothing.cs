using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSmoothing : IAsyncAction, IDependency {

        [Min(0.001f)] public float viewSmoothFactor = 20f;

        private CharacterProcessorQuaternionSmoothing _smoothing;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<ICharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _smoothing = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorQuaternionSmoothing>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _smoothing.smoothFactor = viewSmoothFactor;
            return default;
        }
    }
    
}
