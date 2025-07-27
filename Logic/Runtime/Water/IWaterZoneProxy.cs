using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneProxy {

        float SurfaceOffset { get; }
        
        void BindZone(IWaterZone waterZone);
        void UnbindZone(IWaterZone waterZone);
        
        void GetBox(out Vector3 position, out Quaternion rotation, out Vector3 size);
    }
    
}