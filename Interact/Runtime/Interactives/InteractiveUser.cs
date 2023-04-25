using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Interact.Detectables;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public sealed class InteractiveUser : MonoBehaviour, IInteractiveUser, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private Detector _interactivesDetector;
        [SerializeField] private CollisionDetectorBase _directViewDetector;

        public event Action<IInteractive> OnDetected = delegate {  };
        public event Action<IInteractive> OnLost = delegate {  };

        public event Action<IInteractive> OnStartInteract = delegate {  };
        public event Action<IInteractive> OnStopInteract = delegate {  };

        public IReadOnlyCollection<IInteractive> Interactives => _interactiveTargetsSet;
        public Transform Transform { get; private set; }

        private readonly List<IInteractive> _interactiveTargets = new List<IInteractive>();
        private readonly HashSet<IInteractive> _interactiveTargetsSet = new HashSet<IInteractive>();

        private readonly List<IInteractive> _interactiveCandidates = new List<IInteractive>();
        private readonly HashSet<IInteractive> _interactiveCandidatesSet = new HashSet<IInteractive>();

        private void Awake() {
            Transform = transform;
        }

        private void OnEnable() {
            _interactivesDetector.OnDetected -= HandleDetected;
            _interactivesDetector.OnDetected += HandleDetected;

            _interactivesDetector.OnLost -= HandleLost;
            _interactivesDetector.OnLost += HandleLost;

            if (_interactiveCandidatesSet.Count > 0 || _interactiveTargetsSet.Count > 0) {
                TimeSources.Get(_timeSourceStage).Subscribe(this);
            }
        }

        private void OnDisable() {
            TimeSources.Get(_timeSourceStage).Unsubscribe(this);

            _interactivesDetector.OnDetected -= HandleDetected;
            _interactivesDetector.OnLost -= HandleLost;

            _interactiveCandidates.Clear();
            _interactiveCandidatesSet.Clear();

            ForceStopInteractAll();
        }

        public bool IsInDirectView(IInteractive interactive, out float distance) {
            _directViewDetector.FetchResults();
            var info = _directViewDetector.CollisionInfo;

            distance = info.hasContact ? info.distance : 0f;

            return info.hasContact &&
                   info.transform.GetHashCode() == interactive.Transform.GetHashCode();
        }

        public bool IsDetected(IInteractive interactive) {
            return _interactiveCandidatesSet.Contains(interactive);
        }

        public bool IsInteractingWith(IInteractive interactive) {
            return _interactiveTargetsSet.Contains(interactive);
        }

        public bool TryStartInteract(IInteractive interactive) {
            if (!enabled || interactive == null || _interactiveTargetsSet.Contains(interactive)) return false;

            _interactiveTargetsSet.Add(interactive);
            _interactiveTargets.Add(interactive);

            interactive.NotifyStartedInteractWith(this);
            OnStartInteract.Invoke(interactive);

            TimeSources.Get(_timeSourceStage).Subscribe(this);

            return true;
        }

        public bool TryStopInteract(IInteractive interactive) {
            if (interactive == null || !_interactiveTargetsSet.Contains(interactive)) return false;

            _interactiveTargetsSet.Remove(interactive);
            _interactiveTargets.Remove(interactive);

            OnStopInteract.Invoke(interactive);
            interactive.NotifyStoppedInteractWith(this);

            if (_interactiveTargetsSet.Count == 0 && _interactiveCandidatesSet.Count == 0) {
                TimeSources.Get(_timeSourceStage).Unsubscribe(this);
            }

            return true;
        }

        public void ForceStopInteractAll() {
            _interactiveTargetsSet.Clear();

            for (int i = 0; i < _interactiveTargets.Count; i++) {
                var interactive = _interactiveTargets[i];

                OnStopInteract.Invoke(interactive);
                interactive.NotifyStoppedInteractWith(this);
            }

            _interactiveTargets.Clear();

            if (_interactiveTargetsSet.Count == 0 && _interactiveCandidatesSet.Count == 0) {
                TimeSources.Get(_timeSourceStage).Unsubscribe(this);
            }
        }

        private void HandleDetected(IDetectable detectable) {
            if (detectable.Transform.GetComponent<IInteractive>() is not {} interactive ||
                _interactiveCandidatesSet.Contains(interactive)) return;

            _interactiveCandidatesSet.Add(interactive);
            _interactiveCandidates.Add(interactive);

            OnDetected.Invoke(interactive);
            interactive.NotifyDetectedBy(this);

            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        private void HandleLost(IDetectable detectable) {
            if (detectable.Transform == null ||
                detectable.Transform.GetComponent<IInteractive>() is not {} interactive ||
                !_interactiveCandidatesSet.Contains(interactive)) return;

            _interactiveCandidatesSet.Remove(interactive);
            _interactiveCandidates.Remove(interactive);

            interactive.NotifyLostBy(this);
            OnLost.Invoke(interactive);

            if (_interactiveTargetsSet.Count == 0 && _interactiveCandidatesSet.Count == 0) {
                TimeSources.Get(_timeSourceStage).Unsubscribe(this);
            }
        }

        public void OnUpdate(float dt) {
            for (int i = 0; i < _interactiveCandidates.Count; i++) {
                var interactive = _interactiveCandidates[i];
                if (_interactiveTargetsSet.Contains(interactive)) continue;

                if (!interactive.IsReadyToStartInteractWith(this) ||
                    !interactive.IsAllowedToStartInteractWith(this)) continue;

                TryStartInteract(interactive);
            }

            for (int i = _interactiveTargets.Count - 1; i >= 0; i--) {
                var interactive = _interactiveTargets[i];

                if (interactive.IsAllowedToContinueInteractWith(this)) continue;

                TryStopInteract(interactive);
            }
        }

        public override string ToString() {
            return $"{nameof(InteractiveUser)}({name}, interactives count = {_interactiveTargetsSet.Count})";
        }
    }

}
