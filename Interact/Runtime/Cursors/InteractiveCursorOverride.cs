using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursorOverride : MonoBehaviour {

        [SerializeField] private Interactive _interactive;
        [SerializeField] private CursorIcon _cursorIconOnHover;
        [SerializeField] private CursorIcon _cursorIconWhileInteracting;

        private void Awake() {
            _interactive.OnDetectedByUser -= OnDetectedByUser;
            _interactive.OnDetectedByUser += OnDetectedByUser;

            _interactive.OnLostByUser -= OnLostByUser;
            _interactive.OnLostByUser += OnLostByUser;

            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStartInteract += OnStartInteract;

            _interactive.OnStopInteract -= OnStopInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        private void OnDestroy() {
            _interactive.OnDetectedByUser -= OnDetectedByUser;
            _interactive.OnLostByUser -= OnLostByUser;
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
        }

        private void OnDetectedByUser(IInteractiveUser user) {
            if (_cursorIconOnHover == null) return;

            var host = user.GameObject.GetComponent<IInteractiveCursorHost>();
            host?.StartOverrideCursorIcon(this, _cursorIconOnHover);
        }

        private void OnLostByUser(IInteractiveUser user) {
            if (_cursorIconOnHover == null) return;

            var host = user.GameObject.GetComponent<IInteractiveCursorHost>();
            host?.StopOverrideCursorIcon(this, _cursorIconOnHover);
        }

        private void OnStartInteract(IInteractiveUser user, Vector3 hitPoint) {
            if (_cursorIconWhileInteracting == null) return;

            var host = user.GameObject.GetComponent<IInteractiveCursorHost>();
            host?.StartOverrideCursorIcon(this, _cursorIconWhileInteracting);
        }

        private void OnStopInteract(IInteractiveUser user) {
            if (_cursorIconWhileInteracting == null) return;

            var host = user.GameObject.GetComponent<IInteractiveCursorHost>();
            host?.StopOverrideCursorIcon(this, _cursorIconWhileInteracting);
        }
    }

}
