﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Audio;
using MisterGames.Common.Lists;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlaySoundAction : IActorAction {
        
        public bool attach = true;
        [Min(0f)] public float volume = 1f;
        [Min(0f)] public float pitch = 1f;
        public float pitchRandomAdd;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public bool loop;
        public WeightedValue<AudioClip>[] audioClipVariants;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants.Length == 0) return default;
            
            var clip = audioClipVariants.GetRandom().value;
            float resultPitch = pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd);
            
            if (attach) {
                AudioPool.Main.Play(clip, context.Transform, localPosition: default, volume, resultPitch, spatialBlend, loop, cancellationToken);    
            }
            else {
                AudioPool.Main.Play(clip, context.Transform.position, volume, resultPitch, spatialBlend, loop, cancellationToken);
            }
            
            return default;
        }
    }
    
}