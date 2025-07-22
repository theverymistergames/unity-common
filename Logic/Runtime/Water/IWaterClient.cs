using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterClient {
        
        bool IgnoreWaterZone { get; }
        
        Rigidbody Rigidbody { get; }
        int FloatingPointCount { get; }
        Vector3 GetFloatingPoint(int index);
        
        float SurfaceOffset { get; }
        float MaxSpeed { get; }
        float Buoyancy { get; }
    }
    
}