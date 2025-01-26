using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlaySoundAction : IActorAction {
        
        public bool attach = true;
        [Min(0f)] public float fadeIn;
        [MinMaxSlider(0f, 1f)] public Vector2 startTime;
        [Range(0f, 2f)] public float volume = 1f;
        [Range(0f, 2f)] public float pitch = 1f;
        public float pitchRandomAdd;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public bool loop;
        public AudioClip[] audioClipVariants;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants is not { Length: > 0 }) return default;

            var clip = AudioPool.Main.ShuffleClips(audioClipVariants);
            float resultPitch = pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd);
            float resultStartTime = startTime.GetRandomInRange();
            
            if (attach) {
                AudioPool.Main.Play(clip, context.Transform, localPosition: default, fadeIn, volume, resultPitch, spatialBlend, resultStartTime, loop, cancellationToken);    
            }
            else {
                AudioPool.Main.Play(clip, context.Transform.position, fadeIn, volume, resultPitch, spatialBlend, resultStartTime, loop, cancellationToken);
            }
            
            return default;
        }
    }
    
}