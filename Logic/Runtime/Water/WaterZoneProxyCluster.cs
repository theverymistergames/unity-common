using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZoneProxyCluster : MonoBehaviour, IWaterZoneProxyCluster {
        
        [SerializeField] private WaterZoneProxy[] _proxies;

        public int ClusterId => GetInstanceID();
        public int ProxyCount => _proxies.Length;
        
        public int GetProxyId(int index) {
            return _proxies[index].ProxyId;
        }
    }
    
}