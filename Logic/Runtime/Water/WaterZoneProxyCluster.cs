using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZoneProxyCluster : MonoBehaviour, IWaterZoneProxyCluster {
        
        [SerializeField] private WaterZoneProxy[] _proxies;

        public int VolumeId => GetInstanceID();
        public int ProxyCount => _proxies.Length;
        
        public int GetVolumeId(int index) {
            return _proxies[index].VolumeId;
        }
    }
    
}