using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZoneProxy))]
    public sealed class WaterZoneProxyRegisterer : MonoBehaviour {

        [SerializeField] private WaterZoneProxy _waterZoneProxy;
        [SerializeField] private LabelValue _waterZoneId;

        private void OnEnable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.AddProxy(_waterZoneProxy);
        }

        private void OnDisable() {
            Services.Get<IWaterZone>(_waterZoneId.GetValue())?.RemoveProxy(_waterZoneProxy);
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _waterZoneProxy = GetComponent<WaterZoneProxy>();
        }
#endif
    }
    
}