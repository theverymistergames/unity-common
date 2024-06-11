using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlaySoundOneShotAction : IActorAction {
        
        public AudioSource source;
        public float volume;
        public WeightedValue<AudioClip>[] audioClipVariants;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants.Length == 0) return default;

            var clip = audioClipVariants.GetRandom().value;
            source.PlayOneShot(clip, volume);
            
            return default;
        }
    }
    
}