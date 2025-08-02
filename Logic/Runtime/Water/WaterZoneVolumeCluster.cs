using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZoneVolumeCluster : MonoBehaviour, IWaterZoneVolumeCluster {
        
        [SerializeField] private WaterZoneVolume[] _volumes;

        public int ClusterId => GetHashCode();
        public int VolumeCount => _volumes.Length;
        
        public int GetVolumeId(int index) {
            return _volumes[index].VolumeId;
        }

#if UNITY_EDITOR
        private void Reset() {
            _volumes = GetComponentsInChildren<WaterZoneVolume>();
        }
#endif
    }
    
}