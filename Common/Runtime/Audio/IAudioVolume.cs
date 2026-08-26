using MisterGames.Common.Volumes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Audio {

    public interface IAudioVolume {

        EntityId Id { get; }
        int Priority { get; }
        float ListenerPresence { get; }
        float Weight { get; set; }

        WeightData GetWeight(Vector3 position);
        JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default);

        bool ModifyOcclusionWeightForListener(ref float occlusionWeight);
        bool ModifyOcclusionWeightForSound(ref float occlusionWeight);
        bool ModifyPitch(ref float pitch);
        bool ModifyAttenuationDistance(ref float attenuationDistance);
        bool ModifyLowPassFilter(ref float lpCutoffFreq);
        bool ModifyHighPassFilter(ref float hpCutoffFreq);
    }

}
