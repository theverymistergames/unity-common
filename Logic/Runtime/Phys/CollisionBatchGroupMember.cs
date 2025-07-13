using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CollisionBatchGroupMember : MonoBehaviour {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider[] _colliders;
        [SerializeField] private LabelValue _surfaceMaterial;
        [SerializeField] private LabelValue _group;

        private void OnEnable() {
            ProvideContacts(true);
            Services.Get<CollisionBatchGroup>(_group.GetValue())?.Register(_rigidbody, _surfaceMaterial.GetValue());
        }

        private void OnDisable() {
            ProvideContacts(false);
            Services.Get<CollisionBatchGroup>(_group.GetValue())?.Unregister(_rigidbody);
        }

        private void ProvideContacts(bool provide) {
            for (int i = 0; i < _colliders.Length; i++) {
                _colliders[i].providesContacts = provide;
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
        }
#endif
    }
    
}