using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    public interface IMusicService {
        
        AudioHandle StartMusic(
            AudioClip clip,
            float volume,
            float pitch, 
            float startTime,
            bool loop, 
            bool affectedByTimescale, 
            AudioMixerGroup mixerGroup,
            float fadeIn = -1f,
            float fadeOut = -1f,
            bool waitForPreviousFadeOut = false,
            CancellationToken cancellationToken = default
        );
        
        AudioHandle GetCurrentMusic(AudioMixerGroup mixerGroup);
        
        void StopMusic(AudioMixerGroup mixerGroup, bool immediate = false);
        
        void StopAllMusic(bool immediate = false);
    }
    
}