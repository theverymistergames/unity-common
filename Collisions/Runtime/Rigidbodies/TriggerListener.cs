using System.Collections.Generic;
using MisterGames.Common.Layers;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListener : TriggerEmitter, IUpdate {
        
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _clearCollidersOnDisable = true;

        private readonly struct ColliderData {
            public readonly Collider collider;
            public readonly int frame;
            
            public ColliderData(Collider collider, int frame) {
                this.collider = collider;
                this.frame = frame;
            }
            
            public ColliderData WithFrame(int frame) => new(collider, frame);
        }
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => _indexMap.Keys;
        
        private readonly List<ColliderData> _colliderDataList = new();
        private readonly Dictionary<Collider, int> _indexMap = new();
        private int _frameCount;

        private void OnEnable() {
            if (_colliderDataList.Count > 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            if (!_clearCollidersOnDisable) return;
            
            for (int i = 0; i < _colliderDataList.Count; i++) {
                TriggerExit.Invoke(_colliderDataList[i].collider);
            }

            _colliderDataList.Clear();
            _indexMap.Clear();
        }

        void IUpdate.OnUpdate(float dt) {
            int count = _colliderDataList.Count;
            int validCount = count;

            for (int i = count - 1; i >= 0; i--) {
                var data = _colliderDataList[i];
                if (data.frame >= _frameCount) continue;
                
                if (data.frame >= 0) TriggerExit.Invoke(data.collider);

                _indexMap.Remove(data.collider);
                
                if (_colliderDataList[--validCount] is var swap && swap.frame >= _frameCount &&
                    swap.collider != null) 
                {
                    _colliderDataList[i] = swap;
                    _indexMap[swap.collider] = i;
                }
            }
            
            _colliderDataList.RemoveRange(validCount, count - validCount);
            _frameCount++;
            
            if (validCount <= 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }
        
        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;

            if (_indexMap.TryAdd(collider, _colliderDataList.Count)) {
                _colliderDataList.Add(new ColliderData(collider, _frameCount));
            }
            
            TriggerEnter.Invoke(collider);
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnTriggerStay(Collider collider) {
            if (!CanCollide(collider)) return;
            
            if (_indexMap.TryGetValue(collider, out int index)) {
                _colliderDataList[index] = _colliderDataList[index].WithFrame(_frameCount);
            }
            
            TriggerStay.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider) {
            if (!CanCollide(collider)) return;

            if (_indexMap.Remove(collider, out int index)) {
                _colliderDataList[index] = _colliderDataList[index].WithFrame(-1);
            }
            
            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return enabled && _layerMask.Contains(collider.gameObject.layer);
        }
    }
    
}