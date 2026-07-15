using Unity.Mathematics;

namespace MisterGames.Common.Volumes {
    
    public readonly struct WeightData {
            
        public readonly float weight;
        public readonly int volumeId;
        public readonly float3 closestPoint;
            
        public WeightData(float weight, int volumeId, float3 closestPoint) {
            this.weight = weight;
            this.volumeId = volumeId;
            this.closestPoint = closestPoint;
        }
    }
    
}