using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionInputEnableDisable : IAsyncAction, IDependency {

        public bool isEnabled;

        private ICharacterInputPipeline _input;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _input = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterInputPipeline>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _input.SetEnabled(isEnabled);
            return default;
        }
    }
    
}
