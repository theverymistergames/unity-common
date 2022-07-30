using MisterGames.Common.Collisions;
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

        [Header("Transparency Settings")]
        [SerializeField] private bool _isAlphaControlledByDistance = true;
        [SerializeField] private AnimationCurve _alphaByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Transparency Raycast Settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0.01f)] private float _maxDistance = 6f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private Interactive _interactive;
        private Transform _transform;
        private RaycastHit[] _hits;

        private void Awake() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _transform = transform;
            _hits = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _interactiveUser.OnInteractiveDetected += OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost += OnInteractiveLost;

            if (_isAlphaControlledByDistance) {
                _timeDomain.SubscribeUpdate(this);
            }
        }

        private void OnDisable() {
            if (_isAlphaControlledByDistance) {
                _timeDomain.UnsubscribeUpdate(this);
            }

            _interactiveUser.OnInteractiveDetected -= OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost -= OnInteractiveLost;

            if (_interactive != null) ResetInteractive();
        }

        private void Start() {
            SetCursorIcon(_initialCursorIcon);
        }

        void IUpdate.OnUpdate(float deltaTime) {
            bool hasHit = PerformRaycast(out var hit);
            float alpha = hasHit.ToInt() * _alphaByDistance.Evaluate(hit.distance / _maxDistance);
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

            if (icon == null) {
                _cursorImage.sprite = null;
                return;
            }

            _cursorImage.sprite = icon.sprite;

            var rectTransform = _cursorImage.rectTransform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.size.y);
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

            return CollisionUtils.TryGetMinimumDistanceHit(hitCount, _hits, out hit);
        }
    }

}
