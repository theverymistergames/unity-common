using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public interface IWaterZone {

        void AddProxyCluster(IWaterZoneProxyCluster cluster);
        void RemoveProxyCluster(IWaterZoneProxyCluster cluster);
        
        void AddProxy(IWaterZoneProxy proxy);
        void RemoveProxy(IWaterZoneProxy proxy);
        
        int GetProxyClusterId(IWaterZoneProxy proxy);
        
        void TriggerEnter(Collider collider, IWaterZoneProxy proxy);
        void TriggerExit(Collider collider, IWaterZoneProxy proxy);
    }
    
}