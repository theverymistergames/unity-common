using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Common.Save;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MisterGames.Interact.Cursors {

    public sealed class GameplayCursorHost : MonoBehaviour, ICursorHost, IUpdate {

        [SerializeField] private CursorHostSettings _settings;
        [SerializeField] private Image _cursorImage;
        [SerializeField] private Image _helperImage;
        [SerializeField] private CollisionFilter _collisionFilter = new() { maxDistance = 3f };
        [SerializeField] private CollisionDetectorBase _transparencyRaycaster;

        private readonly struct CursorIconQueueItem {

            public readonly int creationFrame;
            public readonly CursorIcon cursorIcon;

            public CursorIconQueueItem(int creationFrame, CursorIcon cursorIcon = null) {
                this.creationFrame = creationFrame;
                this.cursorIcon = cursorIcon;
            }
        }

        [Serializable]
        private struct LearnDataItem {
            public SerializedGuid guid;
            public int bindingIndex;
            public int bindingPathHash;
            public int learnCount;
        }
        
        [Serializable]
        private sealed class LearnData {
            public List<LearnDataItem> items;
        }
        
        private IDeviceService _deviceService;
        private IGameplaySaveService _saveService;
        
        private readonly Dictionary<object, CursorIconQueueItem> _iconOverridesMap = new();
        private readonly List<object> _destroyedSourcesCache = new();
        private LearnData _learnData;
        private string _learnDataKey;
        private float _lastLearnTime;

        private void Awake() {
            _deviceService = Services.Get<IDeviceService>();
            _saveService = Services.Get<IGameplaySaveService>();
            _learnDataKey = _settings.learnPromptSetting.GetFullLabel();
        }

        private void OnEnable() {
            _iconOverridesMap[this] = new CursorIconQueueItem(TimeSources.frameCount, _settings.initialCursorIcon);

            FetchLearnData();
            
            _saveService.OnProfileUpdated += OnSaveProfileUpdated;
            
            if (_settings.enableCursorOverride) RefreshCursorIcon();
            else SetCursorIcon(_settings.initialCursorIcon);

            PlayerLoopStage.Update.Subscribe(this);
            
            _deviceService.OnDeviceChanged += OnDeviceChanged;
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);

            _saveService.OnProfileUpdated -= OnSaveProfileUpdated;
            
            _deviceService.OnDeviceChanged -= OnDeviceChanged;
            
            _iconOverridesMap.Remove(this);
            SetCursorIcon(null);
        }

        private void OnSaveProfileUpdated(string profileKey) {
            FetchLearnData();

            if (_settings.enableCursorOverride) RefreshCursorIcon();
            else SetCursorIcon(_settings.initialCursorIcon);
        }

        private void FetchLearnData() {
            if (string.IsNullOrWhiteSpace(_learnDataKey)) return;
            
            if (!_saveService.TryGet(_learnDataKey, 0, out _learnData)) {
                _learnData = new LearnData { items = new List<LearnDataItem>() };
            }

            if (_learnData.items == null) {
                _learnData.items = new List<LearnDataItem>();
            }
        }
        
        private void OnDeviceChanged(InputDeviceType obj) {
            RefreshCursorIcon();
        }

        public void ApplyCursorIconOverride(object source, CursorIcon icon) {
            _iconOverridesMap[source] = new CursorIconQueueItem(TimeSources.frameCount, icon);

            RefreshCursorIcon(notifyLearn: true);
        }

        public void ResetCursorIconOverride(object source) {
            if (!_iconOverridesMap.Remove(source)) return;

            RefreshCursorIcon(notifyLearn: true);
        }

        void IUpdate.OnUpdate(float deltaTime) {
            if (RemoveDestroyedIconOverrides()) RefreshCursorIcon();

            if (!_settings.isAlphaControlledByDistance) return;

            var hits = _transparencyRaycaster.FilterLastResults(_collisionFilter);
            bool hasHit = hits.TryGetMinimumDistanceHit(hits.Length, out var hit);

            float alpha = (hasHit && hit.hasContact).AsInt();

            float t = _collisionFilter.maxDistance > 0f
                ? hit.distance / _collisionFilter.maxDistance
                : 0f;

            alpha *= _settings.alphaByDistance.Evaluate(t);

            SetImageAlpha(alpha);
        }

        private void SetImageAlpha(float value) {
            if (_cursorImage == null) return;

            var color = _cursorImage.color;
            color.a = value;
            _cursorImage.color = color;
        }

        private bool RemoveDestroyedIconOverrides() {
            _destroyedSourcesCache.Clear();

            foreach (object source in _iconOverridesMap.Keys) {
                if (source is UnityEngine.Object o && o == null) _destroyedSourcesCache.Add(source);
            }

            for (int i = 0; i < _destroyedSourcesCache.Count; i++) {
                _iconOverridesMap.Remove(_destroyedSourcesCache[i]);
            }

            bool removed = _destroyedSourcesCache.Count > 0;
            _destroyedSourcesCache.Clear();

            return removed;
        }

        /// <summary>
        /// Learn count is increased only when cursor is refreshed by an external icon override change,
        /// so that internal refreshes (save profile update, device change, destroyed sources cleanup)
        /// do not affect learn progress.
        /// </summary>
        private void RefreshCursorIcon(bool notifyLearn = false) {
            if (!_settings.enableCursorOverride) return;

            RemoveDestroyedIconOverrides();

            int lastCreationFrame = -1;
            CursorIconQueueItem lastCreatedItem = default;

            foreach (var item in _iconOverridesMap.Values) {
                if (lastCreationFrame >= 0 && item.creationFrame <= lastCreationFrame) continue;

                lastCreationFrame = item.creationFrame;
                lastCreatedItem = item;
            }

            SetCursorIcon(lastCreatedItem.cursorIcon, notifyLearn);
        }
        
        private void SetCursorIcon(CursorIcon icon, bool notifyLearn = false) {
            if (!enabled || icon == null) {
                if (_cursorImage != null) _cursorImage.enabled = false;
                if (_helperImage != null) _helperImage.enabled = false;
                return;
            }

            bool hasInput = TryGetInputBinding(icon, out var action, out int bindingIndex);
            int bindingPathHash = hasInput ? GetBindingPathHash(action, bindingIndex) : 0;
            int learntCount = hasInput ? GetFinishedLearnTimes(action, bindingIndex, bindingPathHash) : 0;
            int targetLearnCount = GetTargetLearnTimes();
            bool isLearning = hasInput && targetLearnCount >= 0 && learntCount < targetLearnCount;
            bool canShowPrompt = hasInput && (!icon.disablePromptAfterLearn || targetLearnCount < 0 || isLearning);
            bool canLearn = notifyLearn && isLearning;
            
            if (_cursorImage != null) {
                _cursorImage.color = icon.tint;
                _cursorImage.enabled = true;

                if (canShowPrompt && icon.showInteractionPrompt == CursorIcon.PromptMode.ReplaceCursor) {
                    _cursorImage.sprite = icon.iconsTable.GetSprite(action.bindings[bindingIndex]);
                    var rect = _cursorImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.promptSize.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.promptSize.y);
                    if (canLearn) NotifyLearn(action, bindingIndex, bindingPathHash);
                }
                else {
                    _cursorImage.sprite = icon.sprite;
                    var rect = _cursorImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.size.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.size.y);
                }
            }

            if (_helperImage != null) {
                if (canShowPrompt && icon.showInteractionPrompt == CursorIcon.PromptMode.ShowAdditive) {
                    _helperImage.sprite = icon.iconsTable.GetSprite(action.bindings[bindingIndex]);
                    _helperImage.color = icon.tint;
                    _helperImage.enabled = true;
                    var rect = _helperImage.rectTransform;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, icon.promptSize.x);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, icon.promptSize.y);
                    if (canLearn) NotifyLearn(action, bindingIndex, bindingPathHash);
                }
                else {
                    _helperImage.enabled = false;
                }
            }
        }

        private bool TryGetInputBinding(CursorIcon icon, out InputAction action, out int index) {
            action = null;
            index = -1;
            
            if (icon.showInteractionPrompt == CursorIcon.PromptMode.Disable || icon.iconsTable == null) return false;
            
            action = icon.interactionAction.Get();
            if (action == null) return false;
            
            index = _deviceService.CurrentDevice switch {
                InputDeviceType.KeyboardMouse => icon.interactionBindingMouse,
                InputDeviceType.Gamepad => icon.interactionBindingGamepad,
                _ => throw new ArgumentOutOfRangeException()
            };
            return index >= 0 && index < action.bindings.Count;
        }

        private int GetTargetLearnTimes() {
            return _deviceService.CurrentDevice switch {
                InputDeviceType.KeyboardMouse => _settings.learnCountToDisablePromptKeyboardMouse,
                InputDeviceType.Gamepad => _settings.learnCountToDisablePromptGamepad,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static int GetBindingPathHash(InputAction action, int bindingIndex) {
            string path = action.bindings[bindingIndex].effectivePath;
            return string.IsNullOrEmpty(path) ? 0 : Animator.StringToHash(path);
        }
        
        private int GetFinishedLearnTimes(InputAction action, int bindingIndex, int bindingPathHash) {
            if (_learnData == null) return 0;

            var guid = new SerializedGuid(action.id);
            int i = _learnData.items.TryFindIndex((guid, bindingIndex), (item, data) => item.bindingIndex == data.bindingIndex && item.guid == data.guid);
            if (i < 0) return 0;

            var item = _learnData.items[i];
            
            // Binding was rebound after it had been learnt, so it has to be learnt again.
            return item.bindingPathHash == bindingPathHash ? item.learnCount : 0;
        }

        private void NotifyLearn(InputAction action, int bindingIndex, int bindingPathHash) {
            if (_learnData == null || TimeSources.scaledTime < _lastLearnTime + _settings.learnCooldown) return;

            _lastLearnTime = TimeSources.scaledTime;
            
            var guid = new SerializedGuid(action.id);
            int i = _learnData.items.TryFindIndex((guid, bindingIndex), (item, data) => item.bindingIndex == data.bindingIndex && item.guid == data.guid);
            
            if (i < 0) {
                _learnData.items.Add(new LearnDataItem {
                    guid = guid,
                    bindingIndex = bindingIndex,
                    bindingPathHash = bindingPathHash,
                    learnCount = 1,
                });
            }
            else {
                var item = _learnData.items[i];
                
                // Binding was rebound after it had been learnt, so learn count starts over.
                item.learnCount = item.bindingPathHash == bindingPathHash ? item.learnCount + 1 : 1;
                item.bindingPathHash = bindingPathHash;
                
                _learnData.items[i] = item;
            }

            _saveService.Set(_learnDataKey, 0, _learnData);
        }
    }

}
