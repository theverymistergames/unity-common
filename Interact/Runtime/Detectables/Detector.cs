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

        private readonly struct CandidateData {

            public readonly IDetectable detectable;
            public readonly Collider collider;
            public readonly int detectableHash;

            public CandidateData(IDetectable detectable, Collider collider, int detectableHash) {
                this.detectable = detectable;
                this.collider = collider;
                this.detectableHash = detectableHash;
            }
        }

        private readonly List<IDetectable> _detectedTargets = new();
        private readonly HashSet<IDetectable> _detectedTargetsSet = new();

        private readonly List<CandidateData> _detectedCandidates = new();
        private readonly List<IDetectable> _detectablesCache = new();
        private readonly List<IDetectable> _forceLoseCache = new();

        private readonly List<CollisionInfo> _hitsBuffer = new();
        private readonly List<CollisionInfo> _hitsBufferResult = new();

        private CollisionInfo _directViewHit;
        private IDetectable _directViewDetectable;

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
            _detectablesCache.Clear();
            _forceLoseCache.Clear();

            _hitsBuffer.Clear();
            _hitsBufferResult.Clear();

            ResetDirectViewHit();
        }

        public bool IsInDirectView(IDetectable detectable, out float distance) {
            distance = _directViewHit.hasContact ? _directViewHit.distance : 0f;
            return _directViewHit.hasContact &&
                   detectable != null &&
                   ReferenceEquals(detectable, _directViewDetectable);
        }

        public bool IsDetected(IDetectable detectable) {
            return _detectedTargetsSet.Contains(detectable);
        }

        public void ForceDetect(IDetectable detectable) {
            if (detectable?.Transform == null || IsDetected(detectable)) return;

            _detectedTargetsSet.Add(detectable);
            _detectedTargets.Add(detectable);

            OnDetected.Invoke(detectable);
            detectable.NotifyDetectedBy(this);
        }

        public void ForceLose(IDetectable detectable) {
            if (detectable == null || !IsDetected(detectable)) return;

            _detectedTargetsSet.Remove(detectable);
            _detectedTargets.Remove(detectable);

            if (detectable.Transform != null) detectable.NotifyLostBy(this);
            OnLost.Invoke(detectable);
        }

        public void ForceLoseAll() {
            // Lost callbacks can modify targets, so a snapshot is iterated.
            _forceLoseCache.Clear();
            _forceLoseCache.AddRange(_detectedTargets);

            _detectedTargetsSet.Clear();
            _detectedTargets.Clear();

            for (int i = 0; i < _forceLoseCache.Count; i++) {
                var detectable = _forceLoseCache[i];

                if (detectable?.Transform != null) detectable.NotifyLostBy(this);
                if (detectable != null) OnLost.Invoke(detectable);
            }

            _forceLoseCache.Clear();

            // Nothing must be left even if a callback has added something back.
            _detectedTargetsSet.Clear();
            _detectedTargets.Clear();
        }

        void IUpdate.OnUpdate(float dt) {
            var hits = GetHits();

            UpdateCandidates(hits);
            UpdateDirectViewHit();

            NotifyNewDetectedOrAllowedTargets();
            NotifyLostOrNotAllowedTargets();
        }

        private IReadOnlyList<CollisionInfo> GetHits() {
            _hitsBuffer.Clear();
            _hitsBufferResult.Clear();

            var hits = _collisionDetector.FilterLastResults(_collisionFilter);
            for (int i = 0; i < hits.Length; i++) {
                _hitsBuffer.Add(hits[i]);
            }

            _hitsBuffer.SortByDistance(hits.Length, ascending: true);

            for (int i = 0; i < _hitsBuffer.Count; i++) {
                var hit = _hitsBuffer[i];
                if (!IsActiveCollider(hit.collider)) continue;

                _hitsBufferResult.Add(hit);
                if (!hit.collider.isTrigger) break;
            }

            return _hitsBufferResult;
        }

        private void UpdateCandidates(IReadOnlyList<CollisionInfo> hits) {
            for (int i = _detectedCandidates.Count - 1; i >= 0; i--) {
                var candidate = _detectedCandidates[i];

                if (candidate.detectable?.Transform != null &&
                    IsActiveCollider(candidate.collider) &&
                    ContainsCollider(hits, candidate.collider))
                {
                    continue;
                }

                _detectedCandidates.RemoveAt(i);
            }

            for (int i = 0; i < hits.Count; i++) {
                var hit = hits[i];
                if (!hit.hasContact || !IsActiveCollider(hit.collider)) continue;

                var c = hit.collider;
                if (ContainsCandidateWithCollider(c)) continue;

                var detectable = c.GetComponent<IDetectable>() ?? c.GetComponentFromCollider<IDetectable>();
                if (detectable?.Transform == null) continue;

                _detectedCandidates.Add(new CandidateData(detectable, c, detectable.GameObject.GetHashCode()));
            }
        }

        private void UpdateDirectViewHit() {
            ResetDirectViewHit();

            if (_detectedCandidates.Count == 0) return;

            var hits = _directViewDetector.FilterLastResults(_collisionFilter);
            float minDistance = -1f;

            for (int i = 0; i < hits.Length; i++) {
                var info = hits[i];

                if (!info.hasContact ||
                    !IsActiveCollider(info.collider) ||
                    minDistance >= 0f && info.distance > minDistance ||
                    !TryGetCandidateDetectable(info.collider, out var detectable))
                {
                    continue;
                }

                _directViewHit = info;
                _directViewDetectable = detectable;
                minDistance = info.distance;
            }
        }

        private void NotifyNewDetectedOrAllowedTargets() {
            // Detect callbacks can modify candidates and targets, so a snapshot is iterated.
            _detectablesCache.Clear();

            for (int i = 0; i < _detectedCandidates.Count; i++) {
                _detectablesCache.Add(_detectedCandidates[i].detectable);
            }

            for (int i = 0; i < _detectablesCache.Count; i++) {
                var detectable = _detectablesCache[i];

                if (detectable?.Transform == null || _detectedTargetsSet.Contains(detectable) ||
                    !detectable.IsAllowedToStartDetectBy(this))
                {
                    continue;
                }

                ForceDetect(detectable);
            }

            _detectablesCache.Clear();
        }

        private void NotifyLostOrNotAllowedTargets() {
            // Lost callbacks can modify targets, so a snapshot is iterated.
            _detectablesCache.Clear();
            _detectablesCache.AddRange(_detectedTargets);

            for (int i = 0; i < _detectablesCache.Count; i++) {
                var detectable = _detectablesCache[i];

                if (detectable?.Transform != null &&
                    IsCandidate(detectable) &&
                    detectable.IsAllowedToContinueDetectBy(this))
                {
                    continue;
                }

                ForceLose(detectable);
            }

            _detectablesCache.Clear();
        }

        private void ResetDirectViewHit() {
            _directViewHit = CollisionInfo.Empty;
            _directViewDetectable = null;
        }

        private bool IsCandidate(IDetectable detectable) {
            for (int i = 0; i < _detectedCandidates.Count; i++) {
                if (ReferenceEquals(_detectedCandidates[i].detectable, detectable)) return true;
            }

            return false;
        }

        private bool ContainsCandidateWithCollider(Collider collider) {
            for (int i = 0; i < _detectedCandidates.Count; i++) {
                if (_detectedCandidates[i].collider == collider) return true;
            }

            return false;
        }

        private bool TryGetCandidateDetectable(Collider collider, out IDetectable detectable) {
            int hash = GetColliderHash(collider);
            int rootHash = GetColliderRootHash(collider);

            for (int i = 0; i < _detectedCandidates.Count; i++) {
                var candidate = _detectedCandidates[i];

                if (candidate.collider != collider &&
                    candidate.detectableHash != hash &&
                    candidate.detectableHash != rootHash)
                {
                    continue;
                }

                detectable = candidate.detectable;
                return true;
            }

            detectable = null;
            return false;
        }

        private static bool ContainsCollider(IReadOnlyList<CollisionInfo> hits, Collider collider) {
            for (int i = 0; i < hits.Count; i++) {
                if (hits[i].collider == collider) return true;
            }

            return false;
        }

        public override string ToString() {
            return $"{nameof(Detector)}({name}, detected targets/candidates count = {_detectedTargets.Count}/{_detectedCandidates.Count})";
        }

        private static bool IsActiveCollider(Collider c) {
            return c != null && c.enabled && c.gameObject.activeInHierarchy;
        }

        private static int GetColliderHash(Collider c) {
            return c.gameObject.GetHashCode();
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
                var detectable = _detectedCandidates[i].detectable;
                if (detectable?.Transform == null) continue;

                var color = IsDetected(detectable) ? Color.green : Color.gray;
                DebugExt.DrawLine(transform.position, detectable.Transform.position, color, gizmo: true);
            }
        }
#endif
    }
}
