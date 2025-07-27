using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneProxy {

        void BindZone(IWaterZone waterZone);
        void UnbindZone(IWaterZone waterZone);
        
        void SampleSurface(Vector3 position, out Vector3 surfacePoint, out Vector3 normal, out Vector3 force);
    }
    
}