using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.Audio;
using Object = System.Object;
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
        [VisibleIf(nameof(position), 2)]
        public LabelValue<UnityEngine.Object> libraryObject;
        
        [Header("Settings")]
        [MinMaxSlider(0f, 1f)] public Vector2 startTime;
        [Range(0f, 3f)] public float volume = 1f;
        [Min(0f)] public float fadeIn;
        [Min(-1f)] public float fadeOut = -1f;
        [Range(0f, 3f)] public float pitch = 1f;
        [Range(0f, 3f)] public float pitchRandomAdd;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public bool loop;
        public bool affectedByTimeScale = true;
        public bool occlusion = true;
        public bool affectedByVolumes = true;
        
        [Tooltip("Set to true to avoid stopping sound when action is canceled")] 
        public bool useActorDestroyToken;
        
        [Tooltip("Leave null to use default group")]
        public AudioMixerGroup mixerGroup;
        
        [Space]
        public AudioClip[] audioClipVariants;

        public enum PositionMode {
            ActorTransform,
            ExplicitTransform,
            LibraryObject,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (audioClipVariants is not { Length: > 0 } || AudioPool.Main is not {} pool) return default;

            var clip = pool.ShuffleClips(audioClipVariants);
            float resultPitch = pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd);
            float resultStartTime = startTime.GetRandomInRange();

            var trf = position switch {
                PositionMode.ActorTransform => context.Transform,
                PositionMode.ExplicitTransform => transform,
                PositionMode.LibraryObject => libraryObject.TryGetData(out var obj) && obj is Component c ? c.transform : null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (trf == null) {
                Debug.LogError($"PlaySoundAction.Apply: f {UnityEngine.Time.frameCount}, cannot find transform (mode {position}) to play sound. Audio clip variants: {audioClipVariants.AsString()}");
                return default;
            }

            var options = AudioOptions.None;
            options |= loop ? AudioOptions.Loop : AudioOptions.None;
            options |= affectedByTimeScale ? AudioOptions.AffectedByTimeScale : AudioOptions.None;
            options |= occlusion ? AudioOptions.ApplyOcclusion : AudioOptions.None;
            options |= affectedByVolumes ? AudioOptions.AffectedByVolumes : AudioOptions.None;

            cancellationToken = useActorDestroyToken ? context.DestroyToken : cancellationToken;
            
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