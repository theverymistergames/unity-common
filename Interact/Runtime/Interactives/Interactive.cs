using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public sealed class Interactive : MonoBehaviour, IInteractive {

        [SerializeField] private InteractionStrategy _strategy;

        public event Action<IInteractiveUser> OnDetectedBy = delegate {  };
        public event Action<IInteractiveUser> OnLostBy = delegate {  };

        public event Action<IInteractiveUser> OnStartInteract = delegate {  };
        public event Action<IInteractiveUser> OnStopInteract = delegate {  };

        public IReadOnlyCollection<IInteractiveUser> Users => _users;
        public Transform Transform { get; private set; }

        private readonly List<IInteractiveUser> _users = new List<IInteractiveUser>();
        private readonly Dictionary<IInteractiveUser, InteractionData> _userInteractionMap = new Dictionary<IInteractiveUser, InteractionData>();

        private readonly struct InteractionData {

            public readonly int startTime;

            public InteractionData(int startTime) {
                this.startTime = startTime;
            }
        }

        private void Awake() {
            Transform = transform;
        }

        private void OnDisable() {
            ForceStopInteractWithAllUsers();
        }

        public bool IsInteractingWith(IInteractiveUser user) {
            return _userInteractionMap.ContainsKey(user);
        }

        public bool TryGetInteractionStartTime(IInteractiveUser user, out int startTime) {
            if (_userInteractionMap.TryGetValue(user, out var data)) {
                startTime = data.startTime;
                return true;
            }

            startTime = 0;
            return false;
        }

        public bool IsReadyToStartInteractWith(IInteractiveUser user) {
            return enabled && _strategy.IsReadyToStartInteraction(user, this);
        }

        public bool IsAllowedToStartInteractWith(IInteractiveUser user) {
            return enabled && _strategy.IsAllowedToStartInteraction(user, this);
        }

        public bool IsAllowedToContinueInteractWith(IInteractiveUser user) {
            return enabled && _strategy.IsAllowedToContinueInteraction(user, this);
        }

        public void NotifyDetectedBy(IInteractiveUser user) {
            OnDetectedBy.Invoke(user);
        }

        public void NotifyLostBy(IInteractiveUser user) {
            OnLostBy.Invoke(user);
        }

        public void NotifyStartedInteractWith(IInteractiveUser user) {
            if (IsInteractingWith(user)) return;

            _userInteractionMap.Add(user, new InteractionData(TimeSources.frameCount));
            _users.Add(user);

            OnStartInteract.Invoke(user);
        }

        public void NotifyStoppedInteractWith(IInteractiveUser user) {
            if (!IsInteractingWith(user)) return;

            _userInteractionMap.Remove(user);
            _users.Remove(user);

            OnStopInteract.Invoke(user);
        }

        public void ForceStopInteractWithAllUsers() {
            for (int i = _users.Count - 1; i >= 0; i--) {
                _users[i].TryStopInteract(this);
            }

            _users.Clear();
            _userInteractionMap.Clear();
        }

        public override string ToString() {
            return $"{nameof(Interactive)}({name}, users count = {_users.Count})";
        }
    }

}
