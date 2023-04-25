using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public sealed class Detector : MonoBehaviour, IDetector, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private CollisionDetectorBase _directViewDetector;
        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private CollisionFilter _collisionFilter;

        public event Action<IDetectable> OnDetected = delegate {  };
        public event Action<IDetectable> OnLost = delegate {  };

        public IReadOnlyCollection<IDetectable> Targets => _detectedTargetsSet;
        public Transform Transform { get; private set; }

        private readonly List<IDetectable> _detectedTargets = new List<IDetectable>();
        private readonly HashSet<IDetectable> _detectedTargetsSet = new HashSet<IDetectable>();

        private readonly List<IDetectable> _detectedCandidates = new List<IDetectable>();
        private readonly HashSet<IDetectable> _detectedCandidatesSet = new HashSet<IDetectable>();

        private readonly HashSet<int> _detectedTransformHashesSet = new HashSet<int>();
        private readonly HashSet<int> _detectedTransformHashesBuffer = new HashSet<int>();

        private void Awake() {
            Transform = transform;
        }

        private void OnEnable() {
            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        private void OnDisable() {
            TimeSources.Get(_timeSourceStage).Unsubscribe(this);

            ForceLoseAll();

            _detectedCandidates.Clear();
            _detectedCandidatesSet.Clear();

            _detectedTransformHashesSet.Clear();
            _detectedTransformHashesBuffer.Clear();
        }

        public bool IsInDirectView(IDetectable detectable, out float distance) {
            _directViewDetector.FetchResults();
            var info = _directViewDetector.CollisionInfo;

            distance = info.hasContact ? info.distance : 0f;

            return info.hasContact &&
                   info.transform.GetHashCode() == detectable.Transform.GetHashCode();
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

        public void OnUpdate(float dt) {
            _collisionDetector.FetchResults();
            var hits = _collisionDetector.FilterLastResults(_collisionFilter);

            FillDetectedTransformHashesInto(hits, _detectedTransformHashesBuffer);

            RemoveNotDetectedCandidates(_detectedTransformHashesBuffer);
            AddNewDetectedCandidates(_detectedTransformHashesSet, hits);

            NotifyNewDetectedOrAllowedTargets(_detectedTransformHashesBuffer);
            NotifyLostOrNotAllowedTargets(_detectedTransformHashesBuffer);

            FillDetectedTransformHashesInto(hits, _detectedTransformHashesSet);
        }

        private void RemoveNotDetectedCandidates(ICollection<int> detectedTransformHashes) {
            for (int i = _detectedCandidates.Count - 1; i >= 0; i--) {
                var detectable = _detectedCandidates[i];
                if (detectedTransformHashes.Contains(detectable.Transform.GetHashCode())) continue;

                _detectedCandidatesSet.Remove(detectable);
                _detectedCandidates.RemoveAt(i);
            }
        }

        private void AddNewDetectedCandidates(ICollection<int> lastDetectedTransformHashes, ReadOnlySpan<CollisionInfo> hits) {
            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (!hit.hasContact) continue;

                int hash = hit.transform.GetHashCode();
                if (lastDetectedTransformHashes.Contains(hash)) continue;

                if (hit.transform.GetComponent<IDetectable>() is not {} detectable) continue;

                _detectedCandidatesSet.Add(detectable);
                _detectedCandidates.Add(detectable);
            }
        }

        private void NotifyNewDetectedOrAllowedTargets(ICollection<int> detectedTransformHashes) {
            for (int i = 0; i < _detectedCandidates.Count; i++) {
                var detectable = _detectedCandidates[i];
                if (_detectedTargetsSet.Contains(detectable)) continue;

                if (!detectedTransformHashes.Contains(detectable.Transform.GetHashCode()) ||
                    !detectable.IsAllowedToStartDetectBy(this)) continue;

                ForceDetect(detectable);
            }
        }

        private void NotifyLostOrNotAllowedTargets(ICollection<int> detectedTransformHashes) {
            for (int i = _detectedTargets.Count - 1; i >= 0; i--) {
                var detectable = _detectedTargets[i];

                if (detectedTransformHashes.Contains(detectable.Transform.GetHashCode()) &&
                    detectable.IsAllowedToContinueDetectBy(this)) continue;

                ForceLose(detectable);
            }
        }

        private static void FillDetectedTransformHashesInto(ReadOnlySpan<CollisionInfo> hits, ISet<int> dest) {
            dest.Clear();

            for (int i = 0; i < hits.Length; i++) {
                var hit = hits[i];
                if (hit.hasContact) dest.Add(hit.transform.GetHashCode());
            }
        }

        public override string ToString() {
            return $"{nameof(Detector)}(" +
                   $"{name}, " +
                   $"detected targets/candidates count = {_detectedTargetsSet.Count}/{_detectedCandidatesSet.Count}" +
                   $")";
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawDetectables;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_debugDrawDetectables) return;

            DbgSphere.Create().Color(Color.blue).Position(transform.position).Radius(0.2f).Draw();

            foreach (var detectable in _detectedCandidatesSet) {
                var color = IsDetected(detectable) ? Color.green : Color.gray;
                DbgLine.Create().Color(color).From(transform.position).To(detectable.Transform.position).Draw();
            }
        }
#endif
    }
}
