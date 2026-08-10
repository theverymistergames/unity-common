using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
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

        public event Action<IInteractive> OnStartInteract = delegate {  };
        public event Action<IInteractive> OnStopInteract = delegate {  };

        public IReadOnlyCollection<IInteractive> Interactives => _interactiveTargetsSet;
        public IDetector Detector => _interactivesDetector;
        public Transform Transform { get; private set; }
        public Transform ViewOrigin => _viewOrigin;
        public GameObject Root => _root;

        private readonly HashSet<IInteractive> _interactiveTargetsSet = new();
        private readonly HashSet<IInteractive> _interactiveCandidatesSet = new();

        // Detectable is the key: it allows removing a candidate even if its interactive
        // or game object is already destroyed and cannot be resolved by GetComponent.
        private readonly Dictionary<IDetectable, IInteractive> _detectableToCandidateMap = new();

        private readonly List<IInteractive> _interactiveCache = new();
        private readonly List<IInteractive> _forceStopCache = new();
        private readonly List<IDetectable> _detectableCache = new();
        private readonly List<IDetectable> _forceLoseCache = new();
        private bool _enabled;

        private void Awake() {
            Transform = transform;
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

        public bool IsInteractingWith(IInteractive interactive) {
            return _interactiveTargetsSet.Contains(interactive);
        }

        public bool TryStartInteract(IInteractive interactive) {
            if (!_enabled || interactive?.Transform == null || !_interactiveTargetsSet.Add(interactive)) return false;

            interactive.NotifyStartedInteractWith(this);
            OnStartInteract.Invoke(interactive);

            PlayerLoopStage.Update.Subscribe(this);

            return true;
        }

        public bool TryStopInteract(IInteractive interactive) {
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
                TryStopInteract(_forceStopCache[i]);
            }

            _forceStopCache.Clear();
            _interactiveTargetsSet.Clear();

            UnsubscribeIfNoInteractives();
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

                if (interactive?.Transform == null ||
                    _interactiveTargetsSet.Contains(interactive) ||
                    !interactive.IsReadyToStartInteractWith(this) ||
                    !interactive.IsAllowedToStartInteractWith(this))
                {
                    continue;
                }

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
