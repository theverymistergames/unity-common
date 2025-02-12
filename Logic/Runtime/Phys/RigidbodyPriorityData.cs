using MisterGames.Actors;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyPriorityData : MonoBehaviour, IActorComponent {
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private RigidbodyCustomGravity _rigidbodyCustomGravity;
        [SerializeField] private bool _useGravityDefault = true;
        
        public Rigidbody Rigidbody => _rigidbody;
        public bool UseGravity => _useGravityMap.GetResultOrDefault(_useGravityDefault);
        
        private readonly PriorityMap<object, bool> _useGravityMap = new();
        
        private void Awake() { 
            _rigidbody = GetComponent<Rigidbody>();
            UpdateGravityUsage();
        }

        public void SetUseGravity(object source, bool useGravity, int priority) {
            _useGravityMap.Set(source, useGravity, priority);
            UpdateGravityUsage();
        }

        public void RemoveUseGravity(object source) {
            _useGravityMap.Remove(source);
            UpdateGravityUsage();
        }

        private void UpdateGravityUsage() {
            if (_rigidbodyCustomGravity != null) {
                _rigidbodyCustomGravity.UseGravity = UseGravity;
                return;
            }
            
            _rigidbody.useGravity = UseGravity;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbodyCustomGravity = GetComponent<RigidbodyCustomGravity>();
        }  
#endif
    }
    
}