using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Breath;
using MisterGames.Character.Core;
using MisterGames.Common.Data;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetBreathSettings : IActorAction {

        public Optional<float> period;
        public Optional<float> amplitude;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var breath = context.GetComponent<ICharacterBreathPipeline>();

            if (period.HasValue) breath.Period = period.Value;
            if (amplitude.HasValue) breath.Amplitude = amplitude.Value;

            return default;
        }
    }
    
}
