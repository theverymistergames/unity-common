using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Interact.Cursors {

    public class CursorHost : MonoBehaviour, ICursorHost, IUpdate {

        [Header("General")]
        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Cursor Settings")]
        [SerializeField] private bool _enableCursorOverride = true;
        [SerializeField] private Image _cursorImage;
        [SerializeField] private CursorIcon _initialCursorIcon;

        [Header("Transparency Settings")]
        [SerializeField] private bool _isAlphaControlledByDistance = true;
        [SerializeField] private AnimationCurve _alphaByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private CollisionFilter _collisionFilter = new CollisionFilter { maxDistance = 3f };
        [SerializeField] private CollisionDetectorBase _transparencyRaycaster;

        private readonly Dictionary<object, CursorIconQueueItem> _iconOverridesMap = new Dictionary<object, CursorIconQueueItem>();

        private readonly struct CursorIconQueueItem {

            public readonly int creationFrame;
            public readonly CursorIcon cursorIcon;

            public CursorIconQueueItem(int creationFrame, CursorIcon cursorIcon = null) {
                this.creationFrame = creationFrame;
                this.cursorIcon = cursorIcon;
            }
        }

        private void OnEnable() {
            ApplyCursorIconOverride(this, _initialCursorIcon);

            if (!_enableCursorOverride) SetCursorIcon(_initialCursorIcon);

            TimeSources.Get(_timeSourceStage).Subscribe(this);

            Application.focusChanged -= OnApplicationFocusChanged;
            Application.focusChanged += OnApplicationFocusChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;

            TimeSources.Get(_timeSourceStage).Unsubscribe(this);

            ResetCursorIconOverride(this);
            SetCursorIcon(null);
        }

        private void Start() {
            OnApplicationFocusChanged(true);
        }

        public void ApplyCursorIconOverride(object source, CursorIcon icon) {
            _iconOverridesMap[source] = new CursorIconQueueItem(TimeSources.FrameCount, icon);

            RefreshCursorIcon();
        }

        public void ResetCursorIconOverride(object source) {
            if (_iconOverridesMap.ContainsKey(source)) _iconOverridesMap.Remove(source);

            RefreshCursorIcon();
        }

        public void OnUpdate(float deltaTime) {
            if (!_isAlphaControlledByDistance) return;

            _transparencyRaycaster.FetchResults();
            var hits = _transparencyRaycaster.FilterLastResults(_collisionFilter);
            bool hasHit = hits.TryGetMinimumDistanceHit(hits.Length, out var hit);

            float alpha = (hasHit && hit.hasContact).AsInt();

            float t = _collisionFilter.maxDistance > 0f
                ? hit.distance / _collisionFilter.maxDistance
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
            if (!_enableCursorOverride) return;

            int lastCreationFrame = -1;
            CursorIconQueueItem lastCreatedItem = default;

            foreach (var item in _iconOverridesMap.Values) {
                if (lastCreationFrame >= 0 && item.creationFrame <= lastCreationFrame) continue;

                lastCreationFrame = item.creationFrame;
                lastCreatedItem = item;
            }

            SetCursorIcon(lastCreatedItem.cursorIcon);
        }

        private void SetCursorIcon(CursorIcon icon) {
            if (_cursorImage == null) return;

            if (!enabled || icon == null) {
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
    }

}
