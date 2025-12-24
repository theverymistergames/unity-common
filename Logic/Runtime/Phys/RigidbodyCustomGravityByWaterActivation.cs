using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using MisterGames.Logic.Water;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class RigidbodyCustomGravityByWaterActivation : MonoBehaviour {

        [SerializeField] private LabelValue _groupId;
        [SerializeField] private LabelValue _waterZoneId;

        private void OnEnable() {
            if (Services.Get<IWaterZone>(_waterZoneId.GetValue()) is {} waterZone) waterZone.OnRigidbodyEnter += OnRigidbodyEnter;
        }

        private void OnDisable() {
            if (Services.Get<IWaterZone>(_waterZoneId.GetValue()) is {} waterZone) waterZone.OnRigidbodyEnter -= OnRigidbodyEnter;
        }

        private void OnRigidbodyEnter(Rigidbody rigidbody, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal) {
            Services.Get<RigidbodyCustomGravityGroup>(_groupId.GetValue())?.ForceActivate(rigidbody);
        }
    }
    
}