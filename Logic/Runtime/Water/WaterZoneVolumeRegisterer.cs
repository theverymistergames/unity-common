using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZoneVolume))]
    public sealed class WaterZoneVolumeRegisterer : MonoBehaviour {

        [SerializeField] private WaterZoneVolume _volume;
        [SerializeField] private LabelValue _waterZoneId;

        private void OnEnable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.AddVolume(_volume);
        }

        private void OnDisable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.RemoveVolume(_volume);
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _volume = GetComponent<WaterZoneVolume>();
        }
#endif
    }
    
}