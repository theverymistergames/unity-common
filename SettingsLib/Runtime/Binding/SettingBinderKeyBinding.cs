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
            button.OnClicked -= OnClicked;
            _desc?.RemoveBindingListener(NotifyBindingApplied);
            
            _service = null;
            _desc = null;
            _id = null;

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
                !_desc.PrepareRebinding(out var action, out bool actionEnabled, out int bindingIndex))
            {
                return;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.AddTopLayerNavigationCallback(this);
                navigationService.BlockUiInputModule(this);
            }
            
            SetIcon(keyBindingConfig.inputIcons.GetFallbackSprite(), "???");

            _wasActionEnabled = actionEnabled;

            var operation = new InputActionRebindingExtensions.RebindingOperation()
                .WithAction(action)
                .WithTargetBinding(bindingIndex)
                .OnMatchWaitForAnother(0.05f)

                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("<Pointer>/delta")
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Touchscreen>/touch*/position")
                .WithControlsExcluding("<Touchscreen>/touch*/delta")
                .WithControlsExcluding("<Mouse>/clickCount")
                .WithActionEventNotificationsBeingSuppressed()
                .WithMatchingEventsBeingSuppressed()

                .OnCancel(OnRebindingFinish)
                .OnComplete(OnRebindingFinish)
                .OnApplyBinding(OnRebindingApply);

            switch (_desc.GetDeviceType()) {
                case InputDeviceType.KeyboardMouse:
                    operation.WithControlsHavingToMatchPath("<Keyboard>");
                    operation.WithControlsHavingToMatchPath("<Mouse>");
                    break;
                
                case InputDeviceType.Gamepad:
                    operation.WithControlsHavingToMatchPath("<Gamepad>");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _rebindingOperation = operation;
            _rebindingOperation.Start();
        }

        private void StopRebindingDialogue() {
            _rebindingOperation?.Dispose();
            _rebindingOperation = null;

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.RemoveTopLayerNavigationCallback(this);
                navigationService.UnblockUiInputModule(this, delay: keyBindingConfig.delayUnblockUiAfterRebind);
            }

            if (_desc == null) return;
            
            _desc.FinishRebinding(_wasActionEnabled);
            SetupValue(_desc);
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
                
                var parentCanvas = Services.Get<CanvasRegistry>().FindClosestParentCanvas(button.transform);
                Services.Get<IUiModalDialogService>().CreateModalDialogDefault(parentCanvas)
                    .SetTitle(keyBindingConfig.rebindingDialogTitle)
                    .SetContent(keyBindingConfig.rebindingDialogContent, (LocalizationKey key, Locale _, ref string value) => {
                        var sb = new StringBuilder();
                        for (int i = 0; i < inputActionsWithSamePath.Length; i++) {
                            sb.AppendLine(string.Format(keyBindingConfig.rebindingDialogUsedAction.GetValue(), inputActionsWithSamePath[i].GetValue()));
                        }

                        value = string.Format(key.GetValue(), GetEmbeddedIconTag(controlPath), sb);
                    })
                    .AddButton(keyBindingConfig.rebindingDialogOk, () => {
                        _isRebindingDialogActive = false;
                        _desc?.ApplyRebinding(_service, _id, controlPath);
                        _desc?.FinishRebinding(_wasActionEnabled);
                    })
                    .AddButton(keyBindingConfig.rebindingDialogCancel, () => {
                        _isRebindingDialogActive = false;
                        _desc?.FinishRebinding(_wasActionEnabled);
                        SetupValue(_desc);
                    })
                    .SetBackNavigation(canCloseOnNavigateBack: true, callButton: 1)
                    .Show();
                return;
            }
            
            if (samePathBindings != null) ListPool<KeyBindingSetting>.Release(samePathBindings);
            
            _isRebindingDialogActive = false;
            
            _desc?.ApplyRebinding(_service, _id, controlPath);
            _desc?.FinishRebinding(_wasActionEnabled);
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
            var sprite = keyBindingConfig.inputIcons.GetSprite(binding, gamepadType);
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