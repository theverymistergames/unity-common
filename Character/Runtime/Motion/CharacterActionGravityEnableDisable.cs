using System;
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

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _mass = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _mass.isGravityEnabled = isEnabled;
            return default;
        }
    }
    
}
