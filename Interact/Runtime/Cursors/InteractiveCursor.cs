using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Interact.Core;
using MisterGames.Tick.Core;
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
        [SerializeField] private float _maxDistance;
        [SerializeField] private CollisionDetector _transparencyRaycaster;

        private Interactive _interactive;
        private CollisionFilter _transparencyRaycastFilter;
        private CollisionInfo _lastCollisionInfo;

        private void Awake() {
            _transparencyRaycastFilter = new CollisionFilter { maxDistance = _maxDistance };
        }

        private void OnEnable() {
            _interactiveUser.OnInteractiveDetected += OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost += OnInteractiveLost;

            _timeDomain.Source.Subscribe(this);

            Application.focusChanged -= OnApplicationFocusChanged;
            Application.focusChanged += OnApplicationFocusChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;

            _timeDomain.Source.Unsubscribe(this);

            _interactiveUser.OnInteractiveDetected -= OnInteractiveDetected;
            _interactiveUser.OnInteractiveLost -= OnInteractiveLost;

            if (_interactive != null) ResetInteractive();
        }

        private void Start() {
            OnApplicationFocusChanged(true);
            SetCursorIcon(_initialCursorIcon);
        }

        private static void OnApplicationFocusChanged(bool isFocused) {
            Cursor.visible = !isFocused;
            Cursor.lockState = isFocused ? CursorLockMode.Locked : CursorLockMode.None;
        }

        void IUpdate.OnUpdate(float deltaTime) {
            if (!_isAlphaControlledByDistance) return;

            _transparencyRaycaster.FetchResults();
            _transparencyRaycaster.FilterLastResults(_transparencyRaycastFilter, out _lastCollisionInfo);

            float alpha = _lastCollisionInfo.hasContact.ToInt();
            alpha *= _alphaByDistance.Evaluate(_lastCollisionInfo.lastDistance / _maxDistance);

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

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawRaycastHit;

        private void Update() {
            DbgDraw();
        }

        private void DbgDraw() {
            if (_debugDrawRaycastHit) {
                DbgPointer.Create().Color(Color.cyan).Position(_lastCollisionInfo.lastHitPoint).Size(0.5f).Draw();
            }
        }
#endif
    }

}
