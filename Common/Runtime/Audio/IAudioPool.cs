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
            bool loop = false,
            CancellationToken cancellationToken = default
        );
        
        AudioHandle Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            int hash = 0,
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            bool loop = false,
            CancellationToken cancellationToken = default
        );

        AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips);

        AudioHandle GetAudioHandle(Transform attachedTo, int hash);
        
        void ReleaseAudioHandle(int id);

        bool IsValidAudioHandle(int id);
    }
    
}