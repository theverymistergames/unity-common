using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Input.Icons;
using MisterGames.SettingsLib.Descs;
using MisterGames.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.SettingsLib.Base {
    
    [Serializable]
    public sealed class SettingBinderKeyBinding : ISettingBinder {

        public InputIconsTable inputIcons;
        public UiButton button;
        public Image icon;
        public TMP_Text textFallback;
        
        private ISettingsService _service;
        private KeyBindingSetting _desc;
        private string _id;
        
        public void Bind(ISettingsService service, ISettingDesc desc, string id) {
            if (!IsValidSettingDesc(desc, out var keyBindingSetting)) return;
            
            _service = service;
            _desc = keyBindingSetting;
            _id = id;
            
            button.OnClicked += OnClicked;
        }

        public void Unbind() {
            _service = null;
            _desc = null;
            _id = null;
            
            button.OnClicked -= OnClicked;
        }

        public void SetupView(ISettingDesc desc) {
            
        }

        public void SetupValue(ISettingDesc desc) {
            if (!IsValidSettingDesc(desc, out var keyBindingSetting)) return;
            
            var binding = keyBindingSetting.GetBinding();
            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            var sprite = inputIcons.GetIcon(binding, gamepadType);
            
            SetIcon(sprite, binding.effectivePath);
        }

        private void OnClicked() {
            
        }

        private void SetIcon(Sprite sprite, string path) {
            if (sprite != null) {
                icon.sprite = sprite;
                textFallback.SetText((string) null);
            }
            else {
                icon.sprite = null;
                textFallback.SetText(path);
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