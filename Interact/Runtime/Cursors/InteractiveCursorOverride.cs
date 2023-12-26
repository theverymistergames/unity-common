using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursorOverride : MonoBehaviour {

        [SerializeField] private Interactive _interactive;
        [SerializeField] private InteractiveCursorStrategy _strategy;

        private void Awake() {
            _interactive.OnDetectedBy -= OnDetectedByUser;
            _interactive.OnDetectedBy += OnDetectedByUser;

            _interactive.OnLostBy -= OnLostByUser;
            _interactive.OnLostBy += OnLostByUser;

            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStartInteract += OnStartInteract;

            _interactive.OnStopInteract -= OnStopInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        private void OnDestroy() {
            _interactive.OnDetectedBy -= OnDetectedByUser;
            _interactive.OnLostBy -= OnLostByUser;
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
        }

        private void OnDetectedByUser(IInteractiveUser user) {
            TryApplyCursorIcon(user);
        }

        private void OnLostByUser(IInteractiveUser user) {
            TryApplyCursorIcon(user);
        }

        private void OnStartInteract(IInteractiveUser user) {
            TryApplyCursorIcon(user);
        }

        private void OnStopInteract(IInteractiveUser user) {
            TryApplyCursorIcon(user);
        }

        private void TryApplyCursorIcon(IInteractiveUser user) {
            var host = user.Transform.GetComponent<ICursorHost>();
            if (host == null) return;

            if (_strategy.TryGetCursorIcon(user, _interactive, out var icon)) {
                host.ApplyCursorIconOverride(this, icon);
                return;
            }

            host.ResetCursorIconOverride(this);
        }
    }

}
