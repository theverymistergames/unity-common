using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursor : MonoBehaviour {

        [SerializeField] private InteractiveUser _interactiveUser;
        [SerializeField] private CursorIcon _initialCursorIcon;

        private Interactive _detectedInteractive;

        private void OnEnable() {
            _interactiveUser.OnInteractiveDetected += OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost += OnInteractiveLost;
        }

        private void OnDisable() {
            _interactiveUser.OnInteractiveDetected -= OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost -= OnInteractiveLost;
        }

        private void OnInteractiveDetected(Interactive interactive) {
            _detectedInteractive = interactive;

            var cursorIcon = _detectedInteractive.IsInteracting
                ? _detectedInteractive.Strategy.cursorIconInteract
                : _detectedInteractive.Strategy.cursorIconHover;

            SetCursorIcon(cursorIcon);
        }

        private void OnInteractiveLost() {
            SetCursorIcon(_initialCursorIcon);
        }

        private void SetCursorIcon(CursorIcon icon) {

        }
    }

}
