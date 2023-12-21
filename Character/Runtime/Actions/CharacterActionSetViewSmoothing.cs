using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionSetViewSmoothing : ICharacterAction {

        [Min(0.001f)] public float viewSmoothFactor = 20f;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var smoothing = characterAccess
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorQuaternionSmoothing>();

            smoothing.smoothFactor = viewSmoothFactor;
            return default;
        }
    }
    
}
