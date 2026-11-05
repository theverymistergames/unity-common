using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public sealed class InteractiveUserBlocker : MonoBehaviour {

        [SerializeField] private Interactive _interactive;

        private readonly HashSet<IInteractiveUser> _users = new();

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;

            foreach (var user in _interactive.Users) {
                BlockUser(user);
            }
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;

            foreach (var user in _users) {
                user.Block(this, false);
            }

            _users.Clear();
        }

        private void OnStartInteract(IInteractiveUser user) {
            BlockUser(user);
        }

        private void OnStopInteract(IInteractiveUser user) {
            if (!_users.Remove(user)) return;

            user.Block(this, false);
        }

        private void BlockUser(IInteractiveUser user) {
            if (user == null || !_users.Add(user)) return;

            user.Block(this, true);
        }
    }

}
