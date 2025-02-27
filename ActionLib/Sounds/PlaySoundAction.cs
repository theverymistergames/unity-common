using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlaySoundAction : IActorAction {
        
        [Header("Attach")]
        public bool attach;
        [VisibleIf(nameof(attach))] 
        public HashId attachId;
        public PositionMode position;
        [VisibleIf(nameof(position), 1)]
        public Transform transform;
        
        [Header("Settings")]
        [MinMaxSlider(0f, 1f)] public Vector2 startTime;
        [Range(0f, 2f)] public float volume = 1f;
        [Min(0f)] public float fadeIn;
        [Min(-1f)] public float fadeOut = -1f;
        [Range(0f, 2f)] public float pitch = 1f;
        [Range(0f, 2f)] public float pitchRandomAdd;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public bool loop;
        public bool affectedByTimeScale = true;
        
        [Tooltip("Leave null to use default group")]
        public AudioMixerGroup mixerGroup;
        
        [Space]
        public AudioClip[] audioClipVariants;

        public enum PositionMode {
            ActorTransform,
            ExplicitTransform,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants is not { Length: > 0 } || AudioPool.Main is not {} pool) return default;

            var clip = pool.ShuffleClips(audioClipVariants);
            float resultPitch = pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd);
            float resultStartTime = startTime.GetRandomInRange();

            var trf = position switch {
                PositionMode.ActorTransform => context.Transform,
                PositionMode.ExplicitTransform => transform,
                _ => throw new ArgumentOutOfRangeException()
            };

            var options = AudioOptions.None;
            options |= loop ? AudioOptions.Loop : AudioOptions.None;
            options |= affectedByTimeScale ? AudioOptions.AffectedByTimeScale : AudioOptions.None;
            
            if (attach) {
                pool.Play(clip, trf, localPosition: default, attachId, volume, fadeIn, fadeOut, resultPitch, spatialBlend, resultStartTime, mixerGroup, options, cancellationToken);    
            }
            else {
                pool.Play(clip, trf.position, volume, fadeIn, fadeOut, resultPitch, spatialBlend, resultStartTime, mixerGroup, options, cancellationToken);
            }
            
            return default;
        }
    }
    
}