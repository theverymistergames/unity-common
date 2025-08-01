using MisterGames.Common.Volumes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public interface IAudioVolume {

        int Priority { get; }
        float ListenerPresence { get; }
        
        WeightData GetWeight(Vector3 position);
        void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count);
        
        bool ModifyOcclusionWeightForListener(ref float occlusionWeight);
        bool ModifyPitch(ref float pitch);
        bool ModifyAttenuationDistance(ref float attenuationDistance);
        bool ModifyOcclusionWeightForSound(ref float occlusionWeight);
        bool ModifyLowPassFilter(ref float lpCutoffFreq);
        bool ModifyHighPassFilter(ref float hpCutoffFreq);
    }
    
}