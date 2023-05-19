using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Collisions {
    
    [Serializable]
    public sealed class CharacterActionCollisionsEnableDisable : IAsyncAction, IDependency {

        public bool isEnabled;

        private ICharacterCollisionPipeline _collisions;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _collisions = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterCollisionPipeline>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _collisions.SetEnabled(isEnabled);
            return default;
        }
    }
    
}
