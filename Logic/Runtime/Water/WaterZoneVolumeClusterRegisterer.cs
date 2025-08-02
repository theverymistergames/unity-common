using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZoneVolumeCluster))]
    public sealed class WaterZoneVolumeClusterRegisterer : MonoBehaviour {

        [SerializeField] private WaterZoneVolumeCluster _cluster;
        [SerializeField] private LabelValue _waterZoneId;

        private void OnEnable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.AddVolumeCluster(_cluster);
        }

        private void OnDisable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.RemoveVolumeCluster(_cluster);
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _cluster = GetComponent<WaterZoneVolumeCluster>();
        }
#endif
    }
    
}