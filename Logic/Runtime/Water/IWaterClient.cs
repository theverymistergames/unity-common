using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterClient {
        
        bool IgnoreWaterZone { get; set; }
        
        Rigidbody Rigidbody { get; }
        
        int FloatingPointCount { get; }
        Vector3 GetFloatingPoint(int index);
        
        float Buoyancy { get; set; }
        float MaxSpeed { get; set; }
    }
    
}