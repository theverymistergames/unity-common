using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyCustomGravityGroupMember : MonoBehaviour {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LabelValue[] _groups;

        private void OnEnable() {
            for (int i = 0; i < _groups.Length; i++) {
                Services.Get<RigidbodyCustomGravityGroup>(_groups[i].GetValue())?.Register(_rigidbody);
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _groups.Length; i++) {
                Services.Get<RigidbodyCustomGravityGroup>(_groups[i].GetValue())?.Unregister(_rigidbody);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }  
#endif
    }
    
}