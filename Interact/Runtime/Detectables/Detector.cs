using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
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

        private readonly HashSet<int> _detectedTransformHashesSet = new();
        private readonly HashSet<int> _detectedTransformHashesBuffer = new();

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

            _detectedTransformHashesSet.Clear();
            _detectedTransformHashesBuffer.Clear();
        }

        public bool IsInDirectView(IDetectable detectable, out float distance) {
            distance = _directViewHit.hasContact ? _directViewHit.distance : 0f;
            
            return _directViewHit.hasContact &&
                   _directViewHit.collider.gameObject.GetHashCode() == detectable.GameObject.GetHashCode();
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
            
            FillDetectedTransformHashesInto(hits, _detectedTransformHashesBuffer);

            RemoveNotDetectedCandidates(_detectedTransformHashesBuffer);
            AddNewDetectedCandidates(_detectedTransformHashesSet, hits);
            UpdateDirectViewHits();
            
            NotifyNewDetectedOrAllowedTargets(_detectedTransformHashesBuffer);
            NotifyLostOrNotAllowedTargets(_detectedTransformHashesBuffer);

            FillDetectedTransformHashesInto(hits, _detectedTransformHashesSet);
        }

        private static void FillDetectedTransformHashesInto(ReadOnlySpan<CollisionInfo> hits, ISet<int> dest) {
            dest.Clear();

            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (hit is { hasContact: true, isValid: true } && hit.collider != null) {
                    dest.Add(hit.collider.gameObject.GetHashCode());
                }
            }
        }

        private void UpdateDirectViewHits() {
            var hits = _directViewDetector.FilterLastResults(_collisionFilter);
            float minDistance = -1f;
            
            for (int i = 0; i < hits.Length; i++) {
                var info = hits[i];
                
                if (!info.hasContact ||
                    !_detectedCandidatesHashesSet.Contains(info.collider.gameObject.GetHashCode()) ||
                    minDistance >= 0f && info.distance > minDistance) 
                {
                    continue;
                }
                
                _directViewHit = info;
                minDistance = info.distance;
            }
        }

        private void RemoveNotDetectedCandidates(ICollection<int> detectedTransformHashes) {
            for (int i = _detectedCandidates.Count - 1; i >= 0; i--) {
                var detectable = _detectedCandidates[i];
                int hash = detectable.GameObject.GetHashCode();
                
                if (detectedTransformHashes.Contains(hash)) continue;

                _detectedCandidatesHashesSet.Remove(hash);
                _detectedCandidates.RemoveAt(i);
            }
        }

        private void AddNewDetectedCandidates(ICollection<int> lastDetectedTransformHashes, ReadOnlySpan<CollisionInfo> hits) {
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (!hit.hasContact || hit.collider == null) continue;

                int hash = hit.collider.gameObject.GetHashCode();
                if (lastDetectedTransformHashes.Contains(hash)) continue;

                if (hit.collider.GetComponent<IDetectable>() is not {} detectable) continue;

                _detectedCandidatesHashesSet.Add(detectable.GameObject.GetHashCode());
                _detectedCandidates.Add(detectable);
            }
        }

        private void NotifyNewDetectedOrAllowedTargets(ICollection<int> detectedTransformHashes) {
            for (int i = 0; i < _detectedCandidates.Count; i++) {
                var detectable = _detectedCandidates[i];
                if (_detectedTargetsSet.Contains(detectable)) continue;

                if (!detectedTransformHashes.Contains(detectable.GameObject.GetHashCode()) ||
                    !detectable.IsAllowedToStartDetectBy(this)) 
                {
                    continue;
                }

                ForceDetect(detectable);
            }
        }

        private void NotifyLostOrNotAllowedTargets(ICollection<int> detectedTransformHashes) {
            for (int i = _detectedTargets.Count - 1; i >= 0; i--) {
                var detectable = _detectedTargets[i];

                if (detectedTransformHashes.Contains(detectable.GameObject.GetHashCode()) &&
                    detectable.IsAllowedToContinueDetectBy(this)) 
                {
                    continue;
                }
                
                ForceLose(detectable);
            }
        }

        public override string ToString() {
            return $"{nameof(Detector)}(" +
                   $"{name}, " +
                   $"detected targets/candidates count = {_detectedTargets.Count}/{_detectedCandidates.Count}" +
                   $")";
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawDetectables;

#if UNITY_EDITOR
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
