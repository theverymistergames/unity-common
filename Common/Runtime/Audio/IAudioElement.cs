using System.Threading;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    internal interface IAudioElement {
    
        Transform Transform { get; }
        AudioSource Source { get; }
        AudioLowPassFilter LowPass { get; }
        AudioHighPassFilter HighPass { get; }
        
        int Id { get; set; }
        IAudioPool AudioPool { get; set; }
        
        int MixerGroupId { get; set; }
        float PitchMul { get; set; }
        float AttenuationMul { get; set; }
        AudioOptions AudioOptions { get; set; }
        
        float ClipLength { get; set; }
        float ClipTime { get; set; }
        float FadeOut { get; set; }
        CancellationToken CancellationToken { get; set; }
        
        int OcclusionFlag { get; set; }
    }
    
}