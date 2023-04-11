using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Character.Core2.Actions;
using MisterGames.Character.Core2.Processors;
using UnityEngine;

namespace MisterGames.Character.Core2.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSensitivity : ICharacterAction {

        [Min(0.001f)] public float sensitivityHorizontal = 0.15f;
        [Min(0.001f)] public float sensitivityVertical = 0.15f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var sensitivity = characterAccess.ViewPipeline.GetProcessor<CharacterProcessorVector2Sensitivity>();
            if (sensitivity == null) return;

            sensitivity.sensitivity = new Vector2(sensitivityHorizontal, sensitivityVertical);
        }
    }
    
}
