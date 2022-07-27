using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursor : MonoBehaviour {

        [SerializeField] private InteractiveUser _interactiveUser;
        [SerializeField] private CursorIcon _initialCursorIcon;

        private Interactive _interactive;

        private void OnEnable() {
            _interactiveUser.OnInteractiveDetected += OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost += OnInteractiveLost;
        }

        private void OnDisable() {
            _interactiveUser.OnInteractiveDetected -= OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost -= OnInteractiveLost;

            if (_interactive != null) {
                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
                _interactive = null;
            }
        }

        private void OnInteractiveDetected(Interactive interactive) {
            if (_interactive != null && _interactive.IsInteracting) return;

            _interactive = interactive;

            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStartInteract += OnStartInteract;

            _interactive.OnStopInteract -= OnStopInteract;
            _interactive.OnStopInteract += OnStopInteract;

            SetCursorIconFromInteractive(_interactive);
        }

        private void OnInteractiveLost() {
            if (_interactive != null) {
                if (_interactive.IsInteracting) return;

                _interactive.OnStartInteract -= OnStartInteract;
                _interactive.OnStopInteract -= OnStopInteract;
                _interactive = null;
            }

            SetCursorIcon(_initialCursorIcon);
        }

        private void OnStartInteract(InteractiveUser user) {
            _interactive.OnStopInteract -= OnStopInteract;
            _interactive.OnStopInteract += OnStopInteract;

            SetCursorIconFromInteractive(_interactive);
        }

        private void OnStopInteract() {
            _interactive.OnStopInteract -= OnStopInteract;

            SetCursorIconFromInteractive(_interactive);
        }

        private void SetCursorIconFromInteractive(Interactive interactive) {
            var cursorIcon = interactive == null
                ? _initialCursorIcon
                : interactive.IsInteracting
                    ? interactive.Strategy.cursorIconInteract
                    : interactive.Strategy.cursorIconHover;

            SetCursorIcon(cursorIcon);
        }

        private void SetCursorIcon(CursorIcon icon) {

        }
    }

}
