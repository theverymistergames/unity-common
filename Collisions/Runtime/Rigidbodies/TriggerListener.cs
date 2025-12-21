using System.Collections.Generic;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerListener : TriggerEmitter, IUpdate {
        
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers = true;
        [SerializeField] private bool _clearCollidersOnDisable = true;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => _colliderSet;
        
        private readonly HashSet<Collider> _colliderSet = new();
        private readonly Dictionary<int, Collider> _hashToColliderMap = new();
        private NativeHashMap<int, int> _colliderHashToStayFrameMap;
        private int _frameCount;

        private void Awake() {
            _colliderHashToStayFrameMap = new NativeHashMap<int, int>(2, Allocator.Persistent);
        }

        private void OnDestroy() {
            _colliderHashToStayFrameMap.Dispose();
        }

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
            _colliderHashToStayFrameMap.Clear();
        }
        
        void IUpdate.OnUpdate(float dt) {
            int count = _colliderSet.Count;
            
            var job = new CheckExitedCollidersJob {
                frameCount = _frameCount.IncrementUncheckedRef(),
                colliderHashToStayFrameMap = _colliderHashToStayFrameMap,
                exitArray = new NativeArray<int>(count, Allocator.TempJob),
                exitArrayCount = new NativeArray<int>(2, Allocator.TempJob),
            };
            
            job.Schedule().Complete();

            int exitCount = job.exitArrayCount[0];
            
            for (int i = 0; i < exitCount; i++) {
                if (!_hashToColliderMap.Remove(job.exitArray[i], out var collider)) continue;
                
                _colliderSet.Remove(collider);
                TriggerExit.Invoke(collider);
            }
            
            job.exitArray.Dispose();
            job.exitArrayCount.Dispose();

            if (_colliderSet.Count <= 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }
        
        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;

            int hash = collider.GetHashCode();
            int count = _colliderHashToStayFrameMap.Count;
            
            _colliderHashToStayFrameMap[hash] = _frameCount;
            _hashToColliderMap[hash] = collider;
            _colliderSet.Add(collider);

            TriggerEnter.Invoke(collider);
            
            if (count <= 0) PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnTriggerStay(Collider collider) {
            int hash = collider.GetHashCode();
            if (!_colliderHashToStayFrameMap.ContainsKey(hash)) return;
            
            _colliderHashToStayFrameMap[hash] = _frameCount;
            
            TriggerStay.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider) {
            int hash = collider.GetHashCode();
            if (!_colliderHashToStayFrameMap.Remove(hash)) return;

            _hashToColliderMap.Remove(hash);
            _colliderSet.Remove(collider);
            
            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return enabled && 
                   _layerMask.Contains(collider.gameObject.layer) && 
                   (_collideWithTriggers || !collider.isTrigger);
        }
        
        [BurstCompile]
        private struct CheckExitedCollidersJob : IJob {
            
            [ReadOnly] public int frameCount;
            public NativeHashMap<int, int> colliderHashToStayFrameMap;
            public NativeArray<int> exitArray;
            public NativeArray<int> exitArrayCount;
            
            public void Execute() {
                int exitCount = 0;
                
                foreach (var kvp in colliderHashToStayFrameMap) {
                    if (kvp.Value < frameCount) exitArray[exitCount++] = kvp.Key;
                }

                for (int i = 0; i < exitCount; i++) {
                    colliderHashToStayFrameMap.Remove(exitArray[i]);
                }

                exitArrayCount[0] = exitCount;
            }
        }
    }
    
}