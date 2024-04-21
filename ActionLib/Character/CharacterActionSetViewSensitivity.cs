using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Processors;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetViewSensitivity : IActorAction {

        [Min(0f)] public float sensitivityHorizontal = 0.15f;
        [Min(0f)] public float sensitivityVertical = 0.15f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context
                .GetComponent<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorVector2Sensitivity>()
                .sensitivity = new Vector2(sensitivityVertical, sensitivityHorizontal);
            
            return default;
        }
    }
    
}
