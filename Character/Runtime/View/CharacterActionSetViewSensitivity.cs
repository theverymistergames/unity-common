using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterActionSetViewSensitivity : ICharacterAction {

        [Min(0.001f)] public float sensitivityHorizontal = 0.15f;
        [Min(0.001f)] public float sensitivityVertical = 0.15f;

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var sensitivity = characterAccess
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorVector2Sensitivity>();

            sensitivity.sensitivity = new Vector2(sensitivityHorizontal, sensitivityVertical);

            return default;
        }
    }
    
}
