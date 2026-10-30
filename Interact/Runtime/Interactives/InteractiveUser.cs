using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Interact.Cursors;
using MisterGames.Interact.Detectables;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public sealed class InteractiveUser : MonoBehaviour, IInteractiveUser, IUpdate {

        [SerializeField] private Detector _interactivesDetector;
        [SerializeField] private CollisionDetectorBase _directViewDetector;
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _viewOrigin;

        public event Action<IInteractive> OnDetected = delegate {  };
        public event Action<IInteractive> OnLost = delegate {  };

        public event Action<IInteractive> OnCandidateAdded = delegate {  };
        public event Action<IInteractive> OnCandidateRemoved = delegate {  };

        public event Action<IInteractive> OnStartInteract = delegate {  };
        public event Action<IInteractive> OnStopInteract = delegate {  };

        public IReadOnlyCollection<IInteractive> Interactives => _interactiveTargetsSet;
        public IReadOnlyCollection<IInteractive> Candidates => _startCandidatesSet;
        public IDetector Detector => _interactivesDetector;
        public Transform Transform { get; private set; }
        public Transform ViewOrigin => _viewOrigin;
        public GameObject Root => _root;

        private readonly HashSet<IInteractive> _interactiveTargetsSet = new();
        private readonly HashSet<IInteractive> _interactiveCandidatesSet = new();
        private readonly HashSet<IInteractive> _startCandidatesSet = new();
        private readonly HashSet<int> _blockSources = new();

        // Detectable is the key: it allows removing a candidate even if its interactive
        // or game object is already destroyed and cannot be resolved by GetComponent.
        private readonly Dictionary<IDetectable, IInteractive> _detectableToCandidateMap = new();

        private readonly List<IInteractive> _interactiveCache = new();
        private readonly List<IInteractive> _forceStopCache = new();
        private readonly List<IDetectable> _detectableCache = new();
        private readonly List<IDetectable> _forceLoseCache = new();
        private readonly List<(object source, ICursorHost host)> _cursorHostOverrides = new();
        private ICursorHost _cursorHost;
        private bool _enabled;

        private void Awake() {
            Transform = transform;
            TryGetComponent(out _cursorHost);
        }

        private void OnEnable() {
            _enabled = true;

            _interactivesDetector.OnDetected -= HandleDetected;
            _interactivesDetector.OnDetected += HandleDetected;

            _interactivesDetector.OnLost -= HandleLost;
            _interactivesDetector.OnLost += HandleLost;

            RestoreCandidatesFromDetector();

            if (_interactiveCandidatesSet.Count > 0 || _interactiveTargetsSet.Count > 0) {
                PlayerLoopStage.Update.Subscribe(this);
            }
        }

        private void OnDisable() {
            _enabled = false;

            PlayerLoopStage.Update.Unsubscribe(this);

            _interactivesDetector.OnDetected -= HandleDetected;
            _interactivesDetector.OnLost -= HandleLost;

            ForceStopInteractAll();
            ForceLoseAll();
        }

        public bool IsInDirectView(IInteractive interactive, out float distance) {
            var info = _directViewDetector.CollisionInfo;
            distance = info.hasContact ? info.distance : 0f;

            return info.hasContact &&
                   info.transform != null &&
                   interactive?.Transform != null &&
                   info.transform.GetHashCode() == interactive.Transform.GetHashCode();
        }

        public bool IsDetected(IInteractive interactive) {
            return _interactiveCandidatesSet.Contains(interactive);
        }

        public bool IsCandidate(IInteractive interactive) {
            return _startCandidatesSet.Contains(interactive);
        }

        public bool IsInteractingWith(IInteractive interactive) {
            return _interactiveTargetsSet.Contains(interactive);
        }

        public bool TryStartInteract(IInteractive interactive) {
            if (!_enabled || interactive?.Transform == null || _blockSources.Count > 0 ||
                !_interactiveTargetsSet.Add(interactive))
            {
                return false;
            }

            SetStartCandidate(interactive, false);

            interactive.NotifyStartedInteractWith(this);
            OnStartInteract.Invoke(interactive);

            PlayerLoopStage.Update.Subscribe(this);

            return true;
        }

        public bool TryStopInteract(IInteractive interactive) {
            return _blockSources.Count == 0 && ForceStopInteract(interactive);
        }

        public bool ForceStopInteract(IInteractive interactive) {
            if (interactive == null || !_interactiveTargetsSet.Remove(interactive)) return false;

            OnStopInteract.Invoke(interactive);

            if (interactive.Transform != null) {
                interactive.NotifyStoppedInteractWith(this);
            }

            UnsubscribeIfNoInteractives();

            return true;
        }

        public void ForceStopInteractAll() {
            _forceStopCache.Clear();
            _forceStopCache.AddRange(_interactiveTargetsSet);

            for (int i = 0; i < _forceStopCache.Count; i++) {
                ForceStopInteract(_forceStopCache[i]);
            }

            _forceStopCache.Clear();
            _interactiveTargetsSet.Clear();

            UnsubscribeIfNoInteractives();
        }

        public void Block(object source, bool block) {
            if (source == null) return;

            int hash = source.GetHashCode();

            if (block) _blockSources.Add(hash);
            else _blockSources.Remove(hash);

            _interactivesDetector.Block(source, block);
        }

        public bool TryGetCursorHost(out ICursorHost host) {
            if (_cursorHost != null) {
                host = _cursorHost;
                return true;
            }

            if (_cursorHostOverrides.Count > 0) {
                host = _cursorHostOverrides[^1].host;
                return true;
            }

            host = null;
            return false;
        }

        public void SetCursorHostOverride(object source, ICursorHost host) {
            if (source == null || host == null) return;

            ResetCursorHostOverride(source);
            _cursorHostOverrides.Add((source, host));
        }

        public void ResetCursorHostOverride(object source) {
            for (int i = _cursorHostOverrides.Count - 1; i >= 0; i--) {
                if (Equals(_cursorHostOverrides[i].source, source)) _cursorHostOverrides.RemoveAt(i);
            }
        }

        public void ForceLoseAll() {
            _forceLoseCache.Clear();

            foreach (var detectable in _detectableToCandidateMap.Keys) {
                _forceLoseCache.Add(detectable);
            }

            for (int i = 0; i < _forceLoseCache.Count; i++) {
                RemoveCandidate(_forceLoseCache[i]);
            }

            _forceLoseCache.Clear();

            // Nothing must be left even if a callback has added something back.
            _detectableToCandidateMap.Clear();
            _interactiveCandidatesSet.Clear();
            _startCandidatesSet.Clear();

            UnsubscribeIfNoInteractives();
        }

        private void HandleDetected(IDetectable detectable) {
            if (detectable?.Transform == null ||
                _detectableToCandidateMap.ContainsKey(detectable) ||
                detectable.Transform.GetComponent<IInteractive>() is not { } interactive ||
                !_interactiveCandidatesSet.Add(interactive))
            {
                return;
            }

            _detectableToCandidateMap[detectable] = interactive;

            OnDetected.Invoke(interactive);
            interactive.NotifyDetectedBy(this);

            PlayerLoopStage.Update.Subscribe(this);
        }

        private void HandleLost(IDetectable detectable) {
            if (detectable == null) return;

            RemoveCandidate(detectable);
        }

        private void RemoveCandidate(IDetectable detectable) {
            if (!_detectableToCandidateMap.Remove(detectable, out var interactive)) return;

            _interactiveCandidatesSet.Remove(interactive);

            if (interactive == null) return;

            SetStartCandidate(interactive, false);

            if (interactive.Transform != null) interactive.NotifyLostBy(this);
            OnLost.Invoke(interactive);

            UnsubscribeIfNoInteractives();
        }

        /// <summary>
        /// Detector keeps its targets while this component is disabled and does not raise
        /// OnDetected for them again, so candidates have to be restored explicitly.
        /// </summary>
        private void RestoreCandidatesFromDetector() {
            _detectableCache.Clear();
            _detectableCache.AddRange(_interactivesDetector.Targets);

            for (int i = 0; i < _detectableCache.Count; i++) {
                HandleDetected(_detectableCache[i]);
            }

            _detectableCache.Clear();
        }

        /// <summary>
        /// Drops candidates that are not detected anymore or became destroyed,
        /// in case OnLost was not raised for them.
        /// </summary>
        private void RemoveInvalidCandidates() {
            _detectableCache.Clear();

            foreach (var (detectable, interactive) in _detectableToCandidateMap) {
                if (detectable?.Transform != null &&
                    interactive?.Transform != null &&
                    _interactivesDetector.IsDetected(detectable))
                {
                    continue;
                }

                _detectableCache.Add(detectable);
            }

            for (int i = 0; i < _detectableCache.Count; i++) {
                RemoveCandidate(_detectableCache[i]);
            }

            _detectableCache.Clear();
        }

        private void SetStartCandidate(IInteractive interactive, bool isCandidate) {
            if (interactive == null) return;

            if (isCandidate) {
                if (_startCandidatesSet.Add(interactive)) OnCandidateAdded.Invoke(interactive);
                return;
            }

            if (_startCandidatesSet.Remove(interactive)) OnCandidateRemoved.Invoke(interactive);
        }

        private void UnsubscribeIfNoInteractives() {
            if (_interactiveTargetsSet.Count == 0 && _interactiveCandidatesSet.Count == 0) {
                PlayerLoopStage.Update.Unsubscribe(this);
            }
        }

        void IUpdate.OnUpdate(float dt) {
            RemoveInvalidCandidates();

            _interactiveCache.Clear();
            _interactiveCache.AddRange(_interactiveCandidatesSet);

            for (int i = 0; i < _interactiveCache.Count; i++) {
                var interactive = _interactiveCache[i];

                if (interactive?.Transform == null || _interactiveTargetsSet.Contains(interactive)) {
                    SetStartCandidate(interactive, false);
                    continue;
                }

                bool isReady = interactive.IsReadyToStartInteractWith(this);
                SetStartCandidate(interactive, isReady);

                if (!isReady || !interactive.IsAllowedToStartInteractWith(this)) continue;

                TryStartInteract(interactive);
            }

            _interactiveCache.Clear();
            _interactiveCache.AddRange(_interactiveTargetsSet);

            for (int i = 0; i < _interactiveCache.Count; i++) {
                var interactive = _interactiveCache[i];

                if (interactive?.Transform != null &&
                    interactive.IsAllowedToContinueInteractWith(this))
                {
                    continue;
                }

                TryStopInteract(interactive);
            }

            _interactiveCache.Clear();
        }

        public override string ToString() {
            return $"{nameof(InteractiveUser)}({name}, interactives count = {_interactiveTargetsSet.Count})";
        }
    }

}
