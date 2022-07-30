using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using MisterGames.Interact.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursor : MonoBehaviour, IUpdate {

        [Header("General")]
        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private InteractiveUser _interactiveUser;

        [Header("Cursor Settings")]
        [SerializeField] [Min(0.01f)] private float _maxCursorVisibilityDistance = 7f;
        [SerializeField] private Image _cursorImage;
        [SerializeField] private CursorIcon _initialCursorIcon;

        private Interactive _interactive;

        private void Awake() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable() {
            _interactiveUser.OnInteractiveDetected += OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost += OnInteractiveLost;

            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);

            _interactiveUser.OnInteractiveDetected -= OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost -= OnInteractiveLost;

            if (_interactive != null) ResetInteractive();
        }

        private void Start() {
            SetCursorIcon(_initialCursorIcon);
        }

        void IUpdate.OnUpdate(float deltaTime) {
            bool showCursor = _interactiveUser.HasPossibleInteractive;
            float distance = _interactiveUser.LastDetectionDistance;
            float alpha = showCursor.ToInt() * (1f - distance / _maxCursorVisibilityDistance);

            SetImageAlpha(alpha);
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
                ResetInteractive();
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

        private void ResetInteractive() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
            _interactive = null;
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
            if (_cursorImage == null) return;

            _cursorImage.sprite = icon != null ? icon.sprite : null;
        }

        private void SetImageAlpha(float value) {
            if (_cursorImage == null) return;

            var color = _cursorImage.color;
            color.a = value;
            _cursorImage.color = color;
        }
    }

}
