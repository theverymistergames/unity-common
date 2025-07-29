using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZoneProxy {

        int ProxyId { get; }
        float SurfaceOffset { get; }
        
        void BindZone(IWaterZone waterZone);
        void UnbindZone(IWaterZone waterZone);

        Vector3 GetClosestPoint(Vector3 position);
        void SampleSurface(Vector3 position, out Vector3 surfacePoint, out Vector3 normal);
        void GetBox(out Vector3 position, out Quaternion rotation, out Vector3 size);
    }
    
}