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

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<ICharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _motionFsm = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterMotionFsmPipeline>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            _motionFsm.SetEnabled(isEnabled);
            return default;
        }
    }
    
}
