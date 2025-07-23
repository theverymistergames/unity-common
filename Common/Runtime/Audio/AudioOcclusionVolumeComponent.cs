using UnityEngine.Rendering;

namespace MisterGames.Common.Audio {
    
    [VolumeComponentMenu("Audio/Audio Occlusion")]
    public sealed class AudioOcclusionVolumeComponent : VolumeComponent {
        
        public MinFloatParameter weight = new(1f, 0f);

        public override void Override(VolumeComponent state, float interpFactor) {
            base.Override(state, interpFactor);
            
            float w = ((AudioOcclusionVolumeComponent) state).weight.value;
            AudioPool.Main?.SetGlobalOcclusionWeightNextFrame(w);
        }
    }
    
}