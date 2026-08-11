using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Attributes;
using MisterGames.Interact.Cursors;
using MisterGames.Interact.Detectables;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [RequireComponent(typeof(Detectable))]
    public sealed class Interactive : MonoBehaviour, IInteractive, IActorComponent {
        
        [EmbeddedInspector]
        [SerializeField] private InteractionStrategy _strategy;
        
        [EmbeddedInspector]
        [SerializeField] private InteractiveCursorStrategy _cursorStrategy;

        [SerializeField] private bool _syncEnableStateWithDetectable = true;
        
        public event Action<IInteractiveUser> OnDetectedBy = delegate {  };
        public event Action<IInteractiveUser> OnLostBy = delegate {  };

        public event Action<IInteractiveUser> OnStartInteract = delegate {  };
        public event Action<IInteractiveUser> OnStopInteract = delegate {  };

        public IReadOnlyCollection<IInteractiveUser> Users => _users;
        public Transform Transform { get; private set; }
        public bool IsInteracting => _userInteractionMap.Count > 0;
        private readonly List<IInteractiveUser> _users = new();
        private readonly List<IInteractiveUser> _detectedByUsers = new();
        private readonly Dictionary<IInteractiveUser, InteractionData> _userInteractionMap = new();
        private Detectable _detectable;
        private float _startTime;
        private bool _enabled;

        private readonly struct InteractionData {

            public readonly int startFrame;

            public InteractionData(int startFrame) {
                this.startFrame = startFrame;
            }
        }

        private void Awake() {
            Transform = transform;
            _detectable = GetComponent<Detectable>();
        }

        private void OnEnable() {
            _enabled = true;
            _startTime = Time.time;
            if (_syncEnableStateWithDetectable) _detectable.enabled = true;

            for (int i = 0; i < _detectedByUsers.Count; i++) {
                TryApplyCursorIcon(_detectedByUsers[i]);
            }
        }

        private void OnDisable() {
            _enabled = false;
            
            ForceStopInteractWithAllUsers();
            ResetCursorIconForDetectedByUsers();

            if (_syncEnableStateWithDetectable) _detectable.enabled = false;
        }

        public bool IsInteractingWith(IInteractiveUser user) {
            return _userInteractionMap.ContainsKey(user);
        }

        public bool TryGetInteractionStartTime(IInteractiveUser user, out int startTime) {
            if (_userInteractionMap.TryGetValue(user, out var data)) {
                startTime = data.startFrame;
                return true;
            }

            startTime = 0;
            return false;
        }

        public bool IsReadyToStartInteractWith(IInteractiveUser user) {
            return _enabled && _strategy.IsReadyToStartInteraction(user, this, _startTime);
        }

        public bool IsAllowedToStartInteractWith(IInteractiveUser user) {
            return _enabled && _strategy.IsAllowedToStartInteraction(user, this, _startTime);
        }

        public bool IsAllowedToContinueInteractWith(IInteractiveUser user) {
            return _enabled && _strategy.IsAllowedToContinueInteraction(user, this, _startTime);
        }

        public void NotifyDetectedBy(IInteractiveUser user) {
            if (user != null && !_detectedByUsers.Contains(user)) _detectedByUsers.Add(user);

            OnDetectedBy.Invoke(user);
            TryApplyCursorIcon(user);
        }

        public void NotifyLostBy(IInteractiveUser user) {
            _detectedByUsers.Remove(user);

            OnLostBy.Invoke(user);
            TryApplyCursorIcon(user);
        }

        public void NotifyStartedInteractWith(IInteractiveUser user) {
            if (IsInteractingWith(user)) return;

            _userInteractionMap.Add(user, new InteractionData(TimeSources.frameCount));
            _users.Add(user);

            OnStartInteract.Invoke(user);
            TryApplyCursorIcon(user);
        }

        public void NotifyStoppedInteractWith(IInteractiveUser user) {
            if (!IsInteractingWith(user)) return;

            _userInteractionMap.Remove(user);
            _users.Remove(user);

            OnStopInteract.Invoke(user);
            TryApplyCursorIcon(user);
        }

        public void ForceStopInteractWithAllUsers() {
            for (int i = _users.Count - 1; i >= 0; i--) {
                _users[i].TryStopInteract(this);
            }

            _users.Clear();
            _userInteractionMap.Clear();
        }
        
        private void TryApplyCursorIcon(IInteractiveUser user) {
            if (!TryGetCursorHost(user, out var host)) return;

            if (_enabled && _cursorStrategy.TryGetCursorIcon(user, this, _startTime, out var icon)) {
                host.ApplyCursorIconOverride(this, icon);
                return;
            }

            host.ResetCursorIconOverride(this);
        }

        private void ResetCursorIconForDetectedByUsers() {
            for (int i = 0; i < _detectedByUsers.Count; i++) {
                if (TryGetCursorHost(_detectedByUsers[i], out var host)) host.ResetCursorIconOverride(this);
            }
        }

        private bool TryGetCursorHost(IInteractiveUser user, out ICursorHost host) {
            if (_strategy == null || _cursorStrategy == null || user?.Transform == null) {
                host = null;
                return false;
            }

            return user.Transform.TryGetComponent(out host);
        }

        public override string ToString() {
            return $"{nameof(Interactive)}({name}, users count = {_users.Count})";
        }
    }

}
