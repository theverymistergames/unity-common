using UnityEngine;

namespace MisterGames.Common.Audio {
    
    internal interface IAudioElement {
    
        Transform Transform { get; }
        AudioSource Source { get; }
        AudioLowPassFilter LowPass { get; }
        AudioHighPassFilter HighPass { get; }
        
        int Id { get; set; }
        int MixerGroupId { get; set; }
        float Pitch { get; set; }
        float AttenuationDistance { get; set; }
        AudioOptions AudioOptions { get; set; }
        int OcclusionFlag { get; set; }
    }
    
}