using UnityEngine;

namespace MisterGames.Common.Audio {
    
    internal interface IAudioElement {
    
        Transform Transform { get; }
        AudioSource Source { get; }
        AudioLowPassFilter LowPass { get; }
        AudioHighPassFilter HighPass { get; }
        
        int Id { get; set; }
        float Pitch { get; set; }
        AudioOptions AudioOptions { get; set; }
    }
    
}