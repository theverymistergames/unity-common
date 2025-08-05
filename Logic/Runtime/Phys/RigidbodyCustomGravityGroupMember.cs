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
        [SerializeField] private bool _kinematicOnAwake = true;

        public LabelValue Group { get => _group; set => _group = value; }
        public RigidbodyCustomGravityGroup.Options Options { get => _options; set => _options = value; }
        public bool KinematicOnAwake { get => _kinematicOnAwake; set => _kinematicOnAwake = value; }
        
        private void Awake() {
            _rigidbody.isKinematic = _kinematicOnAwake;
        }

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