using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZone {
    
        void TriggerEnter(Collider collider, IWaterZoneProxy proxy);
        void TriggerExit(Collider collider, IWaterZoneProxy proxy);
    }
    
}