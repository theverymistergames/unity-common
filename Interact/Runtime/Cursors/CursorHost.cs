using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Inputs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Interact.Cursors {

    public sealed class CursorHost : MonoBehaviour, ICursorHost, IUpdate {

        [Header("Cursor Settings")]
        [SerializeField] private bool _enableCursorOverride = true;
        [SerializeField] private Image _cursorImage;
        [SerializeField] private Image _helperImage;
        [SerializeField] private CursorIcon _initialCursorIcon;

        [Header("Transparency Settings")]
        [SerializeField] private bool _isAlphaControlledByDistance = true;
        [SerializeField] private AnimationCurve _alphaByDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] private CollisionFilter _collisionFilter = new() { maxDistance = 3f };
        [SerializeField] private CollisionDetectorBase _transparencyRaycaster;

        private IDeviceService _deviceService;
        
        private readonly Dictionary<object, CursorIconQueueItem> _iconOverridesMap = new();

        private readonly struct CursorIconQueueItem {

            public readonly int creationFrame;
            public readonly CursorIcon cursorIcon;

            public CursorIconQueueItem(int creationFrame, CursorIcon cursorIcon = null) {
                this.creationFrame = creationFrame;
                this.cursorIcon = cursorIcon;
            }
        }

        private void Awake() {
            _deviceService = Services.Get<IDeviceService>();
        }

        private void OnEnable() {
            ApplyCursorIconOverride(this, _initialCursorIcon);

            if (!_enableCursorOverride) SetCursorIcon(_initialCursorIcon);

            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);

            ResetCursorIconOverride(this);
            SetCursorIcon(null);
        }
        
        public void ApplyCursorIconOverride(object source, CursorIcon icon) {
            _iconOverridesMap[source] = new CursorIconQueueItem(TimeSources.frameCount, icon);

            RefreshCursorIcon();
        }

        public void ResetCursorIconOverride(object source) {
            if (!_iconOverridesMap.Remove(source)) return;

            RefreshCursorIcon();
        }

        void IUpdate.OnUpdate(float deltaTime) {
            if (!_isAlphaControlledByDistance) return;

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
            if (!enabled || icon == null) {
                if (_cursorImage != null) _cursorImage.enabled = false;
                if (_helperImage != null) _helperImage.enabled = false;
                return;
            }

            if (_cursorImage != null) {
                _cursorImage.color = icon.tint;
                _cursorImage.enabled = true;
                
                if (icon.showInteractionPrompt == CursorIcon.PromptMode.ReplaceCursor && icon.interactionAction.Get() is { } action) {
                    int index = _deviceService.CurrentDevice switch {
                        InputDeviceType.KeyboardMouse => icon.interactionBindingMouse,
                        InputDeviceType.Gamepad => icon.interactionBindingGamepad,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    _cursorImage.sprite = icon.iconsTable.GetSprite(action.bindings[index]);
                    var rect = _cursorImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.promptSize.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.promptSize.y);
                }
                else {
                    _cursorImage.sprite = icon.sprite;
                    var rect = _cursorImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.size.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.size.y);
                }
            }

            if (_helperImage != null) {
                if (icon.showInteractionPrompt == CursorIcon.PromptMode.ShowAdditive && icon.interactionAction.Get() is { } action) {
                    int index = _deviceService.CurrentDevice switch {
                        InputDeviceType.KeyboardMouse => icon.interactionBindingMouse,
                        InputDeviceType.Gamepad => icon.interactionBindingGamepad,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    _helperImage.sprite = icon.iconsTable.GetSprite(action.bindings[index]);
                    _helperImage.color = icon.tint;
                    _helperImage.enabled = true;
                    var rect = _helperImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.promptSize.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.promptSize.y);
                }
                else {
                    _helperImage.enabled = false;
                }
            }
        }
    }

}
