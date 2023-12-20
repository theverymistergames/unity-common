using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;

namespace MisterGames.Character.Breath {
    
    [Serializable]
    public sealed class CharacterActionSetBreathSettings : ICharacterAction {

        public Optional<float> period;
        public Optional<float> amplitude;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var breath = characterAccess.GetPipeline<ICharacterBreathPipeline>();

            if (period.HasValue) breath.Period = period.Value;
            if (amplitude.HasValue) breath.Amplitude = amplitude.Value;

            return default;
        }
    }
    
}
