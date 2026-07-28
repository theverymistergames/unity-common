using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Input.Icons;
using MisterGames.SettingsLib.Descs;
using MisterGames.UI.Components;
using MisterGames.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderKeyBinding : ISettingBinder, IUiNavigationCallback {

        public InputIconsTable inputIcons;
        public UiButton button;
        public Image icon;
        public TMP_Text textFallback;
        [Min(0f)] public float delayUnblockUiAfterRebind = 0.01f;
        
        private ISettingsService _service;
        private KeyBindingSetting _desc;
        private string _id;
        
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        private bool _wasActionEnabled;
        
        public void Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var keyBindingSetting)) return;
            
            _service = service;
            _desc = keyBindingSetting;
            _id = id;
            
            button.OnClicked += OnClicked;
            _desc.AddBindingListener(NotifyBindingApplied);
        }

        public void Unbind() {
            _desc?.RemoveBindingListener(NotifyBindingApplied);
            
            _service = null;
            _desc = null;
            _id = null;
            
            button.OnClicked -= OnClicked;

            if (_rebindingOperation != null) {
                _rebindingOperation?.Cancel();
                StopRebindingDialogue();   
            }
        }

        private void NotifyBindingApplied(string id, InputAction action, int bindingIndex, string controlPath) {
            if (_desc != null) SetupValue(_desc);
        }

        private void OnClicked() {
            StartRebindingDialogue();
        }

        private void StartRebindingDialogue() {
            if (_rebindingOperation != null || 
                !_desc.PrepareBinding(out var action, out bool actionEnabled, out int bindingIndex)) 
            {
                return;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.AddTopLayerNavigationCallback(this);
                navigationService.BlockUiInputModule(this);
            }
            
            SetIcon(inputIcons.GetFallbackSprite(), "???");

            _wasActionEnabled = actionEnabled;

            _desc.TryGetBinding(out action, out var binding, out bindingIndex);
            binding.ToDisplayString(out string deviceLayoutName, out string _);
            
            _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsHavingToMatchPath($"<{deviceLayoutName}>")
                .WithActionEventNotificationsBeingSuppressed()
                .WithMatchingEventsBeingSuppressed()
                .OnCancel(OnRebindingFinish)
                .OnComplete(OnRebindingFinish)
                .OnApplyBinding(OnRebindingApply);

            _rebindingOperation.Start();
        }

        private void StopRebindingDialogue() {
            _rebindingOperation?.Dispose();
            _rebindingOperation = null;
            
            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.RemoveTopLayerNavigationCallback(this);
                navigationService.UnblockUiInputModule(this, delay: delayUnblockUiAfterRebind);
            }
            
            if (_desc != null) SetupValue(_desc);
        }

        private void OnRebindingApply(InputActionRebindingExtensions.RebindingOperation rebindingOperation, string controlPath) {
            if (_service == null || _id ==  null) return;
            
            _desc?.SetBinding(_service, _id, controlPath, _wasActionEnabled);
        }

        private void OnRebindingFinish(InputActionRebindingExtensions.RebindingOperation rebindingOperation) {
            StopRebindingDialogue();
        }

        bool IUiNavigationCallback.CanNavigateBack() {
            _rebindingOperation?.Cancel();
            StopRebindingDialogue();
            return false;
        }

        void IUiNavigationCallback.OnNavigateBack() { }

        public void SetupView(ISettingDesc desc) { }

        public void SetupValue(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var keyBindingSetting)) return;
            
            var binding = keyBindingSetting.GetBinding();
            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            var sprite = inputIcons.GetIcon(binding, gamepadType);
            
            binding.ToDisplayString(out string _, out string controlPath);

            if (string.IsNullOrWhiteSpace(controlPath)) {
                sprite = inputIcons.GetFallbackSprite();
            }
            
            SetIcon(sprite, controlPath);
        }

        private void SetIcon(Sprite sprite, string path) {
            if (sprite != null) {
                icon.sprite = sprite;
                textFallback.SetText((string) null);
                icon.enabled = true;
                textFallback.enabled = false;
            }
            else {
                icon.sprite = null;
                textFallback.SetText(path);
                icon.enabled = false;
                textFallback.enabled = true;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(icon);
                EditorUtility.SetDirty(textFallback);
            }
#endif
        }
        
        private bool IsValidSettingDesc(ISettingDesc desc, out KeyBindingSetting keyBindingSetting) {
            if (desc is not KeyBindingSetting d) {
                Debug.LogError($"Setting binder {GetType().Name} requires a setting desc of type {nameof(KeyBindingSetting)}. " +
                               $"Provided invalid desc of type {desc?.GetType().Name}.");
                keyBindingSetting = null;
                return false;
            }
            
            keyBindingSetting = d;
            return true;
        }
    }
    
}