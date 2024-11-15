using System.Threading;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public interface IAudioPool {

        public void Play(
            AudioClip clip,
            Vector3 position,
            float volume = 1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            bool loop = false,
            CancellationToken cancellationToken = default
        );
        
        public void Play(
            AudioClip clip,
            Transform attachTo,
            Vector3 localPosition = default,
            float volume = 1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            bool loop = false,
            CancellationToken cancellationToken = default
        );
    }
    
}