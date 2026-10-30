using System.Collections.Generic;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveDelegate : MonoBehaviour, IActorComponent {

        [SerializeField] private InteractiveUser _delegateUser;
        [SerializeField] private bool _blockUsersIfHasCandidates = true;

        private readonly HashSet<IInteractiveUser> _users = new();
        private Interactive _interactive;
        private bool _isActive;

        private void Awake() {
            _interactive = GetComponent<Interactive>();
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;

            _delegateUser.Block(this, true);

            foreach (var user in _interactive.Users) {
                AddUser(user);
            }
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;

            Deactivate();
        }

        private void OnStartInteract(IInteractiveUser user) {
            AddUser(user);
        }

        private void OnStopInteract(IInteractiveUser user) {
            if (!_users.Remove(user)) return;

            user.Block(this, false);

            if (_users.Count == 0) {
                Deactivate();
                return;
            }

            UpdateCursorHost();
        }

        private void AddUser(IInteractiveUser user) {
            if (user == null || ReferenceEquals(user, _delegateUser) || !_users.Add(user)) return;

            UpdateCursorHost();
            Activate();
            user.Block(this, IsBlocking());
        }

        private void UpdateCursorHost() {
            _delegateUser.ResetCursorHostOverride(this);

            foreach (var user in _users) {
                if (!user.TryGetCursorHost(out var host)) continue;

                _delegateUser.SetCursorHostOverride(this, host);
                return;
            }
        }

        private void Activate() {
            if (_isActive) return;

            _isActive = true;

            _delegateUser.OnCandidateAdded += OnDelegateUserChanged;
            _delegateUser.OnCandidateRemoved += OnDelegateUserChanged;
            _delegateUser.OnStartInteract += OnDelegateUserChanged;
            _delegateUser.OnStopInteract += OnDelegateUserChanged;

            _delegateUser.Block(this, false);
        }

        private void Deactivate() {
            foreach (var user in _users) {
                user.Block(this, false);
            }

            _users.Clear();

            if (!_isActive) return;

            _isActive = false;

            _delegateUser.OnCandidateAdded -= OnDelegateUserChanged;
            _delegateUser.OnCandidateRemoved -= OnDelegateUserChanged;
            _delegateUser.OnStartInteract -= OnDelegateUserChanged;
            _delegateUser.OnStopInteract -= OnDelegateUserChanged;

            _delegateUser.ForceStopInteractAll();
            _delegateUser.Detector.ForceLoseAll();
            _delegateUser.ForceLoseAll();
            _delegateUser.Block(this, true);
            _delegateUser.ResetCursorHostOverride(this);
        }

        private void OnDelegateUserChanged(IInteractive interactive) {
            bool block = IsBlocking();

            foreach (var user in _users) {
                user.Block(this, block);
            }
        }

        private bool IsBlocking() {
            return _blockUsersIfHasCandidates &&
                   (ContainsOtherThanSelf(_delegateUser.Candidates) || ContainsOtherThanSelf(_delegateUser.Interactives));
        }

        private bool ContainsOtherThanSelf(IReadOnlyCollection<IInteractive> interactives) {
            foreach (var interactive in interactives) {
                if (!ReferenceEquals(interactive, _interactive)) return true;
            }

            return false;
        }
    }

}
