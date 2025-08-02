using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZone {

        void AddVolume(IWaterZoneVolume volume);
        void RemoveVolume(IWaterZoneVolume volume);
        
        void AddVolumeCluster(IWaterZoneVolumeCluster cluster);
        void RemoveVolumeCluster(IWaterZoneVolumeCluster cluster);
        
        int GetVolumeId(IWaterZoneVolume volume);
        
        void TriggerEnter(Collider collider, IWaterZoneVolume volume);
        void TriggerExit(Collider collider, IWaterZoneVolume volume);
    }
    
}