using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlaySoundAction : IActorAction {
        
        public AudioSource source;
        public Mode mode;
        [VisibleIf(nameof(mode), 1)] public bool loop;
        [Min(0f)] public float volume = 1f;
        [Min(0f)] public float pitch = 1f;
        public float pitchRandomAdd;
        
        public WeightedValue<AudioClip>[] audioClipVariants;

        public enum Mode {
            OneShot,
            SetClip
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants.Length == 0 ||
                source == null && !context.TryGetComponent(out source)
            ) {
                return default;
            }
            
            var clip = audioClipVariants.GetRandom().value;
            
            switch (mode) {
                case Mode.OneShot:
                    source.PlayOneShot(clip, volume);
                    source.pitch = pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd);
                    break;
                
                case Mode.SetClip:
                    source.volume = volume;
                    source.clip = clip;
                    source.loop = loop;
                    source.Play();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return default;
        }
    }
    
}