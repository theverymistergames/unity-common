using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Sounds {

    [Serializable]
    public sealed class PlayMusicAction : IActorAction {

        [Header("Settings")]
        [MinMaxSlider(0f, 1f)] public Vector2 startTime;
        [Range(0f, 3f)] public float volume = 1f;
        [Range(0f, 3f)] public float pitch = 1f;
        [Range(0f, 3f)] public float pitchRandomAdd;
        public bool loop;
        public bool affectedByTimescale;

        [Header("Fade")]
        [Min(-1f)] public float fadeIn = -1f;
        [Min(-1f)] public float fadeOut = -1f;
        [Tooltip("Start the new music only after the music playing on the same mixer group " +
                 "has faded out completely, instead of crossfading them")]
        public bool waitForPreviousFadeOut = true;
        
        [Tooltip("Set to true to avoid stopping music when action is canceled")]
        public bool useActorDestroyToken;
        
        [Tooltip("Leave null to use default group")]
        public AudioMixerGroup mixerGroup;

        [Space]
        public AudioClip[] audioClipVariants;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            PlayMusic(context, cancellationToken);
            return default;
        }

        public AudioHandle PlayMusic(IActor context, CancellationToken cancellationToken) {
            if (audioClipVariants is not { Length: > 0 } ||
                AudioPool.Main is not { } pool ||
                !Services.TryGet(out IMusicService musicService))
            {
                return default;
            }

            var clip = pool.ShuffleClips(audioClipVariants);
            if (clip == null) return default;

            cancellationToken = useActorDestroyToken ? context.DestroyToken : cancellationToken;

            return musicService.StartMusic(
                clip,
                volume,
                pitch + Random.Range(-pitchRandomAdd, pitchRandomAdd),
                startTime.GetRandomInRange(),
                loop,
                affectedByTimescale,
                mixerGroup,
                fadeIn,
                fadeOut,
                waitForPreviousFadeOut,
                cancellationToken
            );
        }
    }

}
