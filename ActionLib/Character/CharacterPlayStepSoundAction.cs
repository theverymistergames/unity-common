using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Steps;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterPlayStepSoundAction : IActorAction {

        [Range(0f, 2f)] public float volumeMul = 1f;
        [Min(-1f)] public float cooldown = -1f;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (context.TryGetComponent(out CharacterStepSounds stepSounds)) {
                stepSounds.PlayStepSound(volumeMul, cooldown);
            }

            return default;
        }
    }
    
}