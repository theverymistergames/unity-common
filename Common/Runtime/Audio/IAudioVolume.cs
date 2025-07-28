using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public interface IAudioVolume {

        int Priority { get; }
        float GetWeight(Vector3 position);

        void ModifyPitch(ref float pitch);
        void ModifyOcclusionWeightForSound(ref float occlusionWeight);
        void ModifyOcclusionWeightForListener(ref float occlusionWeight);
        void ModifyLowHighPassFilters(ref float lpCutoffFreq, ref float hpCutoffFreq);
    }
    
}