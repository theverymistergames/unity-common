using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.MotionFsm {
    
    [Serializable]
    public sealed class CharacterActionMotionFsmPipelineEnableDisable : IAsyncAction, IDependency {

        public bool isEnabled;

        private ICharacterMotionFsmPipeline _motionFsm;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _motionFsm = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterMotionFsmPipeline>();
        }

        public void Initialize() { }

        public void DeInitialize() { }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _motionFsm.SetEnabled(isEnabled);
            return default;
        }
    }
    
}
