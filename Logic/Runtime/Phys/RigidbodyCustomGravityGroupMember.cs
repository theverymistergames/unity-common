using System.Collections;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyCustomGravityGroupMember : MonoBehaviour {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LabelValue _group;
        [SerializeField] private RigidbodyCustomGravityGroup.Options _options;

        private void OnEnable() {
            Services.Get<RigidbodyCustomGravityGroup>(_group.GetValue())?.Register(_rigidbody, _options);
        }

        private void OnDisable() {
            Services.Get<RigidbodyCustomGravityGroup>(_group.GetValue())?.Unregister(_rigidbody);
        }
        
#if UNITY_EDITOR
        private void Reset() {
            StartCoroutine(ResetNextFrame());
        }

        private IEnumerator ResetNextFrame() {
            yield return null;
            _rigidbody = GetComponent<Rigidbody>();
        }
#endif
    }
    
}