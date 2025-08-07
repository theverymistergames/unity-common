using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZone {

        public delegate void TriggerCollider(Collider collider, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal);
        public delegate void TriggerRigidbody(Rigidbody rigidbody, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal);
        
        event TriggerCollider OnColliderEnter;
        event TriggerCollider OnColliderExit;
        
        event TriggerRigidbody OnRigidbodyEnter;
        event TriggerRigidbody OnRigidbodyExit;
        
        void AddVolume(IWaterZoneVolume volume);
        void RemoveVolume(IWaterZoneVolume volume);
        
        void AddVolumeCluster(IWaterZoneVolumeCluster cluster);
        void RemoveVolumeCluster(IWaterZoneVolumeCluster cluster);
        
        int GetVolumeId(IWaterZoneVolume volume);
        
        void TriggerEnter(Collider collider, IWaterZoneVolume volume);
        void TriggerExit(Collider collider, IWaterZoneVolume volume);
    }
    
}