using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZone {
    
        void AddProxy(IWaterZoneProxy proxy);
        void RemoveProxy(IWaterZoneProxy proxy);
        
        void TriggerEnter(Collider collider, IWaterZoneProxy proxy);
        void TriggerExit(Collider collider, IWaterZoneProxy proxy);
    }
    
}