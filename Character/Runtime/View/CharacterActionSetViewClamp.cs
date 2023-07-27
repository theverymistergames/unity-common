using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : IAsyncAction, IDependency {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        private CharacterProcessorViewClamp _clamp;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _clamp = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorViewClamp>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            if (horizontal.HasValue) _clamp.horizontal = horizontal.Value;
            if (vertical.HasValue) _clamp.vertical = vertical.Value;

            return default;
        }
    }
    
}
