using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public interface IAudioVolume {

        int Priority { get; }
        float ListenerPresence { get; }
        
        float GetWeight(Vector3 position, out int volumeId);

        bool ModifyOcclusionWeightForListener(ref float occlusionWeight);
        
        bool ModifyPitch(ref float pitch);
        bool ModifyAttenuationDistance(ref float attenuationDistance);
        bool ModifyOcclusionWeightForSound(ref float occlusionWeight);
        bool ModifyLowPassFilter(ref float lpCutoffFreq);
        bool ModifyHighPassFilter(ref float hpCutoffFreq);
    }
    
}