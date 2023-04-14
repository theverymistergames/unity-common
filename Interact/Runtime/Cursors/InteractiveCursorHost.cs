using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Interact.Core;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Interact.Cursors {

    public class InteractiveCursorHost : MonoBehaviour, IInteractiveCursorHost, IUpdate {

        [Header("General")]
        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Cursor Settings")]
        [SerializeField] private Image _cursorImage;
        [SerializeField] private CursorIcon _initialCursorIcon;

        [Header("Transparency Settings")]
        [SerializeField] private bool _isAlphaControlledByDistance = true;
        [SerializeField] private AnimationCurve _alphaByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private CollisionFilter _collisionFilter = new CollisionFilter { maxDistance = 3f };
        [SerializeField] private CollisionDetectorBase _transparencyRaycaster;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private Interactive _interactive;
        private CollisionInfo _lastCollisionInfo;

        private readonly List<CursorIconQueueItem> _iconQueue = new List<CursorIconQueueItem>(1);

        private readonly struct CursorIconQueueItem {
            public readonly object source;
            public readonly CursorIcon cursorIcon;

            public CursorIconQueueItem(object source, CursorIcon cursorIcon) {
                this.source = source;
                this.cursorIcon = cursorIcon;
            }
        }

        private void Awake() {
            StartOverrideCursorIcon(this, _initialCursorIcon);
        }

        private void OnDestroy() {
            StopOverrideCursorIcon(this, _initialCursorIcon);
        }

        private void OnEnable() {
            _timeSource.Subscribe(this);

            Application.focusChanged -= OnApplicationFocusChanged;
            Application.focusChanged += OnApplicationFocusChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;

            _timeSource.Unsubscribe(this);
        }

        private void Start() {
            OnApplicationFocusChanged(true);
        }

        public void StartOverrideCursorIcon(object source, CursorIcon icon) {
            _iconQueue.Add(new CursorIconQueueItem(source, icon));

            RefreshCursorIcon();
        }

        public void StopOverrideCursorIcon(object source, CursorIcon icon) {
            for (int i = _iconQueue.Count - 1; i >= 0; i--) {
                var item = _iconQueue[i];
                if (item.source == source && item.cursorIcon == icon) _iconQueue.RemoveAt(i);
            }

            RefreshCursorIcon();
        }

        public void OnUpdate(float deltaTime) {
            if (!_isAlphaControlledByDistance) return;

            _transparencyRaycaster.FetchResults();
            _transparencyRaycaster.FilterLastResults(_collisionFilter, out _lastCollisionInfo);

            float alpha = _lastCollisionInfo.hasContact.AsInt();
            float t = _collisionFilter.maxDistance > 0f
                ? _lastCollisionInfo.lastDistance / _collisionFilter.maxDistance
                : 0f;

            alpha *= _alphaByDistance.Evaluate(t);

            SetImageAlpha(alpha);
        }

        private void SetImageAlpha(float value) {
            if (_cursorImage == null) return;

            var color = _cursorImage.color;
            color.a = value;
            _cursorImage.color = color;
        }

        private void RefreshCursorIcon() {
            if (_iconQueue.Count == 0) {
                SetCursorIcon(null);
                return;
            }

            SetCursorIcon(_iconQueue[^1].cursorIcon);
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

        private static void OnApplicationFocusChanged(bool isFocused) {
            Cursor.visible = !isFocused;
            Cursor.lockState = isFocused ? CursorLockMode.Locked : CursorLockMode.None;
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
