using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyGroupMember : MonoBehaviour {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LabelValue _group;
        [SerializeField] private Mode _mode;

        private enum Mode {
            AwakeDestroy,
            EnableDisable,
        }

        private void Awake() {
            
        }

        private void OnDestroy() {
            
        }

        private void OnEnable() {
            
        }

        private void OnDisable() {
            
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }  
#endif
    }
    
}