using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    public interface IAudioPool {

        void RegisterListener(AudioListener listener, Transform up, int priority);
        void UnregisterListener(AudioListener listener);
        
        void RegisterVolume(IAudioVolume volume);
        void UnregisterVolume(IAudioVolume volume);
        
        AudioHandle Play(
            AudioClip clip,
            Vector3 position,
            float volume = 1f,
            float fadeIn = 0f,
            float fadeOut = -1f,
            float pitch = 1f,
            float spatialBlend = 1f,
            float normalizedTime = 0f,
            AudioMixerGroup mixerGroup = null,
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
            AudioMixerGroup mixerGroup = null,
            AudioOptions options = default,
            CancellationToken cancellationToken = default
        );
        
        AudioClip ShuffleClips(IReadOnlyList<AudioClip> clips);
        AudioHandle GetAudioHandle(Transform attachedTo, int hash);
        
        void SetGlobalOcclusionWeightNextFrame(float weight);
        
        internal void ReleaseAudioHandle(int handleId);
        internal void SetAudioHandlePitch(int handleId, float pitch);
        internal bool TryGetAudioElement(int handleId, out IAudioElement audioElement);
    }
    
}