using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Inputs;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Descs;
using MisterGames.UI.Components;
using MisterGames.UI.Navigation;
using MisterGames.UI.UiServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderKeyBinding : ISettingBinder, IUiNavigationCallback {
        
        public KeyBindingConfig keyBindingConfig;
        public UiButton button;
        public Image icon;
        public TMP_Text textFallback;
        [Min(0f)] public float delayUnblockUiAfterRebind = 0.01f;
        
        private ISettingsService _service;
        private KeyBindingSetting _desc;
        private string _id;
        
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        private bool _wasActionEnabled;
        private bool _isRebindingDialogActive;
        
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

        private void OnClicked(UiButton button) {
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
            
            SetIcon(keyBindingConfig.inputIcons.GetFallbackSprite(), "???");

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

            List<KeyBindingSetting> samePathBindings = null;
            
            var groups = _desc.GetKeyBindingGroups();
            foreach (var group in groups) {
                var groupKeyBindings = group.GetKeyBindings();
                foreach ((string id, var keyBinding) in groupKeyBindings) {
                    if (id == _id || !keyBinding.TryGetBinding(out _, out var binding, out _) || binding.effectivePath != controlPath) {
                        continue;
                    }

                    samePathBindings ??= ListPool<KeyBindingSetting>.Get();
                    samePathBindings.Add(keyBinding);
                }
            }

            if (samePathBindings is { Count: > 0 }) {
                _isRebindingDialogActive = true;
                
                var inputActionsWithSamePath = new LocalizationKey[samePathBindings.Count];
                for (int i = 0; i < samePathBindings.Count; i++) {
                    inputActionsWithSamePath[i] = samePathBindings[i].GetName();
                }
                ListPool<KeyBindingSetting>.Release(samePathBindings);
                
                var parentCanvas = Services.Get<CanvasRegistry>().GetClosestParentCanvas(button.transform);
                Services.Get<IUiModalDialogService>().CreateModalDialogDefault(parentCanvas)
                    .SetTitle(keyBindingConfig.rebindingDialogTitle)
                    .SetContent(keyBindingConfig.rebindingDialogContent, (LocalizationKey key, Locale locale, ref string value) => {
                        var sb = new StringBuilder();
                        for (int i = 0; i < inputActionsWithSamePath.Length; i++) {
                            sb.AppendLine(string.Format(keyBindingConfig.rebindingDialogUsedAction.GetValue(), inputActionsWithSamePath[i].GetValue()));
                        }

                        value = string.Format(key.GetValue(), GetEmbeddedIconTag(controlPath), sb);
                    })
                    .AddButton(keyBindingConfig.rebindingDialogOk, () => {
                        _isRebindingDialogActive = false;
                        _desc?.SetBinding(_service, _id, controlPath, _wasActionEnabled);
                    })
                    .AddButton(keyBindingConfig.rebindingDialogCancel, () => {
                        _isRebindingDialogActive = false;
                        SetupValue(_desc);
                    })
                    .SetBackNavigation(canCloseOnNavigateBack: true, callButton: 1)
                    .Show();
                return;
            }
            
            _isRebindingDialogActive = false;
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

            var sprite = GetIcon(keyBindingSetting.GetBinding(), out string controlPath);
            SetIcon(sprite, controlPath);
        }

        private string GetEmbeddedIconTag(string controlPath) {
            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            string sprite = keyBindingConfig.inputIcons.GetEmbeddedSpriteTag(controlPath, gamepadType);
            return string.IsNullOrWhiteSpace(sprite) ? controlPath : sprite;
        }
        
        private Sprite GetIcon(InputBinding binding, out string controlPath) {
            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            var sprite = keyBindingConfig.inputIcons.GetIcon(binding, gamepadType);
            binding.ToDisplayString(out string _, out controlPath);
            
            if (_isRebindingDialogActive || _rebindingOperation != null) {
                sprite = keyBindingConfig.inputIcons.GetFallbackSprite();
            }
            else if (string.IsNullOrWhiteSpace(controlPath)) {
                sprite = keyBindingConfig.inputIcons.GetNullSprite();
            }
            
            return sprite;
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