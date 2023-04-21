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

        public IReadOnlyCollection<IInteractiveUser> Users => _userStartInteractionTimeMap.Keys;
        public Transform Transform { get; private set; }

        private readonly Dictionary<IInteractiveUser, int> _userStartInteractionTimeMap = new Dictionary<IInteractiveUser, int>();
        private readonly List<IInteractiveUser> _users = new List<IInteractiveUser>();

        private void Awake() {
            Transform = transform;
        }

        private void OnDisable() {
            ForceStopInteractWithAllUsers();
        }

        public bool IsInteractingWith(IInteractiveUser user) {
            return _userStartInteractionTimeMap.ContainsKey(user);
        }

        public bool TryGetInteractionStartTime(IInteractiveUser user, out int time) {
            return _userStartInteractionTimeMap.TryGetValue(user, out time);
        }

        public bool IsAllowedToStartInteractWith(IInteractiveUser user) {
            return enabled && _strategy.IsAllowedToStartInteract(user, this);
        }

        public bool IsAllowedToContinueInteractWith(IInteractiveUser user) {
            return enabled && _strategy.IsAllowedToContinueInteract(user, this);
        }

        public void NotifyDetectedBy(IInteractiveUser user) {
            OnDetectedBy.Invoke(user);
        }

        public void NotifyLostBy(IInteractiveUser user) {
            OnLostBy.Invoke(user);
        }

        public void NotifyStartedInteractWith(IInteractiveUser user) {
            if (IsInteractingWith(user)) return;

            _userStartInteractionTimeMap.Add(user, TimeSources.FrameCount);
            _users.Add(user);

            OnStartInteract.Invoke(user);
        }

        public void NotifyStoppedInteractWith(IInteractiveUser user) {
            if (!IsInteractingWith(user)) return;

            _userStartInteractionTimeMap.Remove(user);
            _users.Remove(user);

            OnStopInteract.Invoke(user);
        }

        public void ForceStopInteractWithAllUsers() {
            for (int i = _users.Count - 1; i >= 0; i--) {
                _users[i].TryStopInteract(this);
            }

            _userStartInteractionTimeMap.Clear();
        }

        public override string ToString() {
            return $"{nameof(Interactive)}({name}, users count = {_users.Count})";
        }
    }

}
