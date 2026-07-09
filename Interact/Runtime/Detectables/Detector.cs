using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public sealed class Detector : MonoBehaviour, IDetector, IUpdate {

        [SerializeField] private CollisionDetectorBase _directViewDetector;
        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private CollisionFilter _collisionFilter;
        [SerializeField] private GameObject _root;

        public event Action<IDetectable> OnDetected = delegate {  };
        public event Action<IDetectable> OnLost = delegate {  };

        public IReadOnlyCollection<IDetectable> Targets => _detectedTargetsSet;
        public Transform Transform { get; private set; }
        public GameObject Root => _root;

        private readonly List<IDetectable> _detectedTargets = new();
        private readonly HashSet<IDetectable> _detectedTargetsSet = new();

        private readonly List<IDetectable> _detectedCandidates = new();
        private readonly HashSet<int> _detectedCandidatesHashesSet = new();

        private readonly HashSet<int> _detectedHashesSet = new();
        private readonly HashSet<int> _detectedHashesBuffer = new();

        private CollisionInfo _directViewHit;
        
        private void Awake() {
            Transform = transform;
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);

            ForceLoseAll();

            _detectedCandidates.Clear();
            _detectedCandidatesHashesSet.Clear();

            _detectedHashesSet.Clear();
            _detectedHashesBuffer.Clear();
        }

        public bool IsInDirectView(IDetectable detectable, out float distance) {
            distance = _directViewHit.hasContact ? _directViewHit.distance : 0f;
            
            return _directViewHit.hasContact &&
                   GetColliderRootHash(_directViewHit.collider) == detectable.GameObject.GetHashCode();
        }

        public bool IsDetected(IDetectable detectable) {
            return _detectedTargetsSet.Contains(detectable);
        }

        public void ForceDetect(IDetectable detectable) {
            if (IsDetected(detectable)) return;

            _detectedTargetsSet.Add(detectable);
            _detectedTargets.Add(detectable);

            OnDetected.Invoke(detectable);
            detectable.NotifyDetectedBy(this);
        }

        public void ForceLose(IDetectable detectable) {
            if (!IsDetected(detectable)) return;

            _detectedTargetsSet.Remove(detectable);
            _detectedTargets.Remove(detectable);

            detectable.NotifyLostBy(this);
            OnLost.Invoke(detectable);
        }

        public void ForceLoseAll() {
            _detectedTargetsSet.Clear();

            for (int i = 0; i < _detectedTargets.Count; i++) {
                var detectable = _detectedTargets[i];

                detectable.NotifyLostBy(this);
                OnLost.Invoke(detectable);
            }

            _detectedTargets.Clear();
        }

        void IUpdate.OnUpdate(float dt) {
            var hits = _collisionDetector.FilterLastResults(_collisionFilter);
            
            _detectedHashesBuffer.Clear();
            FillDetectedHashesInto(hits, _detectedHashesBuffer);

            RemoveNotDetectedCandidates(_detectedHashesBuffer);
            AddNewDetectedCandidates(hits, _detectedHashesSet);
            UpdateDirectViewHits();
            
            NotifyNewDetectedOrAllowedTargets(_detectedHashesBuffer);
            NotifyLostOrNotAllowedTargets(_detectedHashesBuffer);

            _detectedHashesSet.Clear();
            FillDetectedHashesInto(hits, _detectedHashesSet);
        }

        private static void FillDetectedHashesInto(ReadOnlySpan<CollisionInfo> hits, HashSet<int> dest) {
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (hit is not { hasContact: true, isValid: true } || hit.collider is not { } c) continue;

                dest.Add(GetColliderRootHash(c));
            }
        }

        private void RemoveNotDetectedCandidates(HashSet<int> detectedHashes) {
            for (int i = _detectedCandidates.Count - 1; i >= 0; i--) {
                var detectable = _detectedCandidates[i];
                int hash = detectable.GameObject.GetHashCode();
                
                if (detectedHashes.Contains(hash)) continue;

                _detectedCandidatesHashesSet.Remove(hash);
                _detectedCandidates.RemoveAt(i);
            }
        }

        private void AddNewDetectedCandidates(ReadOnlySpan<CollisionInfo> hits, HashSet<int> lastDetectedHashes) {
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (!hit.hasContact || hit.collider == null) continue;

                int hash = GetColliderRootHash(hit.collider);
                if (lastDetectedHashes.Contains(hash)) continue;

                if (hit.collider.GetComponentFromCollider<IDetectable>() is not { } detectable) {
                    continue;
                }

                _detectedCandidatesHashesSet.Add(hash);
                _detectedCandidates.Add(detectable);
            }
        }

        private void UpdateDirectViewHits() {
            var hits = _directViewDetector.FilterLastResults(_collisionFilter);
            float minDistance = -1f;
            
            for (int i = 0; i < hits.Length; i++) {
                var info = hits[i];
                
                if (!info.hasContact ||
                    info.collider == null ||
                    !_detectedCandidatesHashesSet.Contains(GetColliderRootHash(info.collider)) ||
                    minDistance >= 0f && info.distance > minDistance) 
                {
                    continue;
                }
                
                _directViewHit = info;
                minDistance = info.distance;
            }
        }

        private void NotifyNewDetectedOrAllowedTargets(HashSet<int> detectedHashes) {
            for (int i = 0; i < _detectedCandidates.Count; i++) {
                var detectable = _detectedCandidates[i];
                if (_detectedTargetsSet.Contains(detectable)) continue;

                if (!detectedHashes.Contains(detectable.GameObject.GetHashCode()) ||
                    !detectable.IsAllowedToStartDetectBy(this)) 
                {
                    continue;
                }

                ForceDetect(detectable);
            }
        }

        private void NotifyLostOrNotAllowedTargets(HashSet<int> detectedHashes) {
            for (int i = _detectedTargets.Count - 1; i >= 0; i--) {
                var detectable = _detectedTargets[i];

                if (detectedHashes.Contains(detectable.GameObject.GetHashCode()) &&
                    detectable.IsAllowedToContinueDetectBy(this)) 
                {
                    continue;
                }
                
                ForceLose(detectable);
            }
        }
        
        public override string ToString() {
            return $"{nameof(Detector)}({name}, detected targets/candidates count = {_detectedTargets.Count}/{_detectedCandidates.Count})";
        }

        private static int GetColliderRootHash(Collider c) {
            return c.attachedRigidbody != null 
                ? c.attachedRigidbody.gameObject.GetHashCode() 
                : c.gameObject.GetHashCode();
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawDetectables;

        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_debugDrawDetectables) return;

            DebugExt.DrawSphere(transform.position, 0.2f, Color.blue, gizmo: true);

            for (int i = 0; i < _detectedCandidates.Count; i++) {
                var detectable = _detectedCandidates[i];
                var color = IsDetected(detectable) ? Color.green : Color.gray;
                DebugExt.DrawLine(transform.position, detectable.Transform.position, color, gizmo: true);
            }
        }
#endif
    }
}
