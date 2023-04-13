using System;
using MisterGames.Character.Access;
using MisterGames.Character.Actions;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.Character.View {
    
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
