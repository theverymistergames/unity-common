using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Breath {
    
    [Serializable]
    public sealed class CharacterActionSetBreathSettings : IAsyncAction, IDependency {

        public Optional<float> period;
        public Optional<float> amplitude;

        private ICharacterBreathPipeline _breath;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _breath = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterBreathPipeline>();
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            if (period.HasValue) _breath.Period = period.Value;
            if (amplitude.HasValue) _breath.Amplitude = amplitude.Value;

            return default;
        }
    }
    
}
