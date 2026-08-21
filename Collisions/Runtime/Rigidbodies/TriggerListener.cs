using System.Collections.Generic;
using MisterGames.Common.Layers;
using MisterGames.Common.Tick;
using Unity.Collections;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListener : TriggerEmitter, IUpdate {
        
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers = true;
        [SerializeField] private bool _clearCollidersOnDisable = true;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => _colliderSet;
        
        private readonly HashSet<Collider> _colliderSet = new();
        private readonly Dictionary<int, Collider> _hashToColliderMap = new();

        private void OnEnable() {
            if (_colliderSet.Count > 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            if (!_clearCollidersOnDisable) return;
            
            foreach (var c in _colliderSet) {
                TriggerExit.Invoke(c);
            }

            _colliderSet.Clear();
            _hashToColliderMap.Clear();
        }
        
        void IUpdate.OnUpdate(float dt) {
            int count = _colliderSet.Count;

            var exitArray = new NativeArray<int>(count, Allocator.TempJob);
            int exitCount = 0;
            
            foreach ((int hash, var collider) in _hashToColliderMap) {
                if (collider != null && collider.gameObject is { activeSelf: true, activeInHierarchy: true }) {
                    continue;
                }
                
                exitArray[exitCount++] = hash;
            }
            
            for (int i = 0; i < exitCount; i++) {
                if (!_hashToColliderMap.Remove(exitArray[i], out var collider)) continue;
                
                _colliderSet.Remove(collider);
                TriggerExit.Invoke(collider);
            }
            
            exitArray.Dispose();

            if (_colliderSet.Count <= 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }
        
        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;

            int hash = collider.GetHashCode();
            int count = _hashToColliderMap.Count;
            
            _hashToColliderMap[hash] = collider;
            _colliderSet.Add(collider);

            TriggerEnter.Invoke(collider);
            
            if (count <= 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnTriggerExit(Collider collider) {
            int hash = collider.GetHashCode();
            if (!_hashToColliderMap.Remove(hash)) return;

            _colliderSet.Remove(collider);
            
            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return enabled && 
                   _layerMask.Contains(collider.gameObject.layer) && 
                   (_collideWithTriggers || !collider.isTrigger);
        }
    }
    
}