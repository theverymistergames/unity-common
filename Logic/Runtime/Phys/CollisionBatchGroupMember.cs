using System.Collections;
using MisterGames.Collisions.Core;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CollisionBatchGroupMember : MonoBehaviour {
        
        [SerializeField] private LabelValue _group;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private SurfaceMaterial _surfaceMaterial;
        [SerializeField] private Collider[] _colliders;

        public LabelValue Group { get => _group; set => _group = value; }
        
        private void OnEnable() {
            ProvideContacts(true);
            Services.Get<CollisionBatchGroup>(_group.GetValue())
                ?.Register(_rigidbody, _surfaceMaterial != null ? _surfaceMaterial.MaterialId : 0);
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
            StartCoroutine(ResetNextFrame());
        }

        private IEnumerator ResetNextFrame() {
            yield return null;
            
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            _surfaceMaterial = GetComponent<SurfaceMaterial>();
        }
#endif
    }
    
}