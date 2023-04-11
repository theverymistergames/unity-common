using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using MisterGames.Character.Core2.Processors;
using UnityEngine;

namespace MisterGames.Character.Core2.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSmoothing : ICharacterAction {

        [Min(0.001f)] public float viewSmoothFactor = 20f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var smoothing = characterAccess.ViewPipeline.GetProcessor<CharacterProcessorQuaternionSmoothing>();
            if (smoothing == null) return;

            smoothing.smoothFactor = viewSmoothFactor;
        }
    }
    
}
