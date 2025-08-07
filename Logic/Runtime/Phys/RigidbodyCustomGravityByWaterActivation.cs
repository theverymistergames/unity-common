using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using MisterGames.Logic.Water;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(RigidbodyCustomGravityGroup))]
    public sealed class RigidbodyCustomGravityByWaterActivation : MonoBehaviour {

        [SerializeField] private RigidbodyCustomGravityGroup _customGravityGroup;
        [SerializeField] private LabelValue _waterZoneId;

        private void OnEnable() {
            if (Services.Get<IWaterZone>(_waterZoneId.GetValue()) is {} waterZone) waterZone.OnRigidbodyEnter += OnRigidbodyEnter;
        }

        private void OnDisable() {
            if (Services.Get<IWaterZone>(_waterZoneId.GetValue()) is {} waterZone) waterZone.OnRigidbodyEnter -= OnRigidbodyEnter;
        }

        private void OnRigidbodyEnter(Rigidbody rigidbody, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal) {
            _customGravityGroup.ForceActivate(rigidbody);
        }
    }
    
}