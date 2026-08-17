using System.Collections.Generic;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Labels;
using MisterGames.Common.Tick;
using MisterGames.Common.Volumes;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class RigidbodySlowZone : MonoBehaviour, IUpdate {

        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private PositionWeightProvider _positionWeightProvider;
        [SerializeField] [Range(0f, 1f)] private float _innerSlowFactor = 0.75f;
        [SerializeField] [Range(0f, 1f)] private float _outerSlowFactor = 0.5f;
        [SerializeField] private bool _disableGravity;
        [SerializeField] private LabelValue _gravityPriority;
        
        private readonly Dictionary<Rigidbody, RigidbodyData> _rigidbodies = new();

        private readonly struct RigidbodyData {
            public readonly bool useGravity;
            
            public RigidbodyData(bool useGravity) {
                this.useGravity = useGravity;
            }
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
            
            bool useGravity = !_disableGravity && rigidbody.useGravity;
            
            if (rigidbody.TryGetComponent(out RigidbodyPriorityData priorityData)) {
                priorityData.SetUseGravity(this, useGravity, _gravityPriority.GetValue());
            }
            else {
                rigidbody.useGravity = !_disableGravity && rigidbody.useGravity;    
            }
        }
        
        private void TriggerExit(Rigidbody rigidbody) {
            _rigidbodies.Remove(rigidbody, out var data);
            if (rigidbody == null) return;
            
            if (rigidbody.TryGetComponent(out RigidbodyPriorityData priorityData)) {
                priorityData.RemoveUseGravity(this);
            }
            else {
                rigidbody.useGravity = data.useGravity;    
            }
        }

        void IUpdate.OnUpdate(float dt) {
            foreach (var rb in _rigidbodies.Keys) {
                if (rb == null || !rb.gameObject.activeSelf || rb.isKinematic) {
                    continue;
                }

                float slowFactor = GetSlowFactor(rb.position);
                if (slowFactor <= 0f) continue;
                
                var force = dt > 0f ? rb.linearVelocity / dt : Vector3.zero;
                rb.AddForce(-force * slowFactor, ForceMode.Acceleration);
            }
        }

        private float GetSlowFactor(Vector3 point) {
            return Mathf.Lerp(_outerSlowFactor, _innerSlowFactor, _positionWeightProvider.GetWeight(point).weight);
        }
    }
    
}