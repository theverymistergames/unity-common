using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public interface IAudioPool {

        AudioHandle Play(
            AudioClip clip,
            Vector3 position,
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            AudioOptions options = default,
            CancellationToken cancellationToken = default
        );
        
        AudioHandle Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            int attachId = 0,
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            AudioOptions options = default,
            CancellationToken cancellationToken = default
        );

        AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips);

        AudioHandle GetAudioHandle(Transform attachedTo, int hash);
        void ReleaseAudioHandle(int handleId);
        void SetAudioHandlePitch(int handleId, float pitch);
        bool TryGetAudioSource(int handleId, out AudioSource source);
        
    }
    
}