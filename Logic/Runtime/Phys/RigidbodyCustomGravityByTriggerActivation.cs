using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class RigidbodyCustomGravityByTriggerActivation : MonoBehaviour {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LabelValue _groupId;

        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEnter;
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEnter;
        }

        private void TriggerEnter(Collider collider) {
            if (collider.attachedRigidbody == null) return;
            
            Services.Get<RigidbodyCustomGravityGroup>(_groupId.GetValue())?.ForceActivate(collider.attachedRigidbody);
        }
    }
    
}