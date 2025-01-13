using System.Collections.Generic;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class RigidbodySlowZone : MonoBehaviour, IUpdate {

        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] [Min(0f)] private float _innerRadius = 0.5f;
        [SerializeField] [Min(0f)] private float _outerRadius = 1f;
        [SerializeField] [Range(0f, 1f)] private float _innerSlowFactor = 0.75f;
        [SerializeField] [Range(0f, 1f)] private float _outerSlowFactor = 0.5f;
        [SerializeField] private bool _disableGravity;
        
        private readonly Dictionary<Rigidbody, RigidbodyData> _rigidbodies = new();
        private Transform _transform;

        private readonly struct RigidbodyData {
            public readonly bool useGravity;
            
            public RigidbodyData(bool useGravity) {
                this.useGravity = useGravity;
            }
        }
        
        private void Awake() {
            _transform = transform;
        }

        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;

            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void OnDestroy() {
            _rigidbodies.Clear();
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            _rigidbodies.Add(rigidbody, new RigidbodyData(rigidbody.useGravity));
            rigidbody.useGravity = !_disableGravity && rigidbody.useGravity;
        }
        
        private void TriggerExit(Rigidbody rigidbody) {
            _rigidbodies.Remove(rigidbody, out var data);
            if (rigidbody != null) rigidbody.useGravity = data.useGravity;
        }

        void IUpdate.OnUpdate(float dt) {
            var center = _transform.position;
            float scale = _transform.localScale.x;
            
            foreach (var rb in _rigidbodies.Keys) {
                if (rb == null || !rb.gameObject.activeSelf || rb.isKinematic) {
                    continue;
                }

                float slowFactor = GetSlowFactor(center, rb.position, scale);
                var force = dt > 0f ? rb.linearVelocity / dt : Vector3.zero;

                rb.AddForce(-force * slowFactor, ForceMode.Acceleration);
            }
        }

        private float GetSlowFactor(Vector3 center, Vector3 point, float scale) {
            float sqrMagnitude = (center - point).sqrMagnitude;
            float t = sqrMagnitude < _innerRadius * _innerRadius * scale * scale ? 0f
                : _innerRadius.IsNearlyEqual(_outerRadius) || sqrMagnitude > _outerRadius * _outerRadius * scale * scale ? 1f
                : ((center - point).magnitude - _innerRadius * scale) / (_outerRadius * scale - _innerRadius * scale);
            
            return Mathf.Lerp(_innerSlowFactor, _outerSlowFactor, t);
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;
            
            var pos = transform.position;
            var scale = transform.localScale;
            DebugExt.DrawSphere(pos, _innerRadius * scale.x, Color.yellow, gizmo: true);
            DebugExt.DrawSphere(pos, _outerRadius * scale.x, Color.green, gizmo: true);
        }

        private void OnValidate() {
            if (_innerRadius > _outerRadius) _innerRadius = _outerRadius;
        }
#endif
    }
    
}