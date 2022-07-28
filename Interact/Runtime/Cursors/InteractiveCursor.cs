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
        [SerializeField] private Image _cursorImage;
        [SerializeField] private CursorIcon _initialCursorIcon;

        [Header("Raycast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0.01f)] private float _maxDistance = 6f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private Interactive _interactive;
        private Transform _transform;
        private RaycastHit[] _hits;

        private void Awake() {
            _transform = transform;
            _hits = new RaycastHit[_maxHits];
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
            bool hasHit = PerformRaycast(out var hit);
            float alpha = hasHit.ToInt() * (1f - hit.distance / _maxDistance);
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

        private bool PerformRaycast(out RaycastHit hit) {
            int hitCount = Physics.RaycastNonAlloc(
                _transform.position,
                _transform.forward,
                _hits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            if (hitCount <= 0) {
                hit = default;
                return false;
            }

            hit = _hits[0];
            float distance = hit.distance;

            for (int i = 1; i < hitCount; i++) {
                var nextHit = _hits[i];
                if (nextHit.distance < distance) hit = nextHit;
            }

            return true;
        }
    }

}
