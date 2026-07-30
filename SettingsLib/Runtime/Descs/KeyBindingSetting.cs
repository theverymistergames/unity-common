using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Inputs;
using MisterGames.Common.Localization;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.SettingsLib.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class KeyBindingSetting : ISettingDesc {

        public LocalizationKey name;
        public InputActionRef inputActionRef;
        [Min(0f)] public int bindingIndex;
        public InputDeviceType deviceType;
        [SerializeReference] [SubclassSelector] public IBindingValidator validator;
        
        public delegate void Listener(string id, InputAction action, int bindingIndex, string path);
        
        private readonly HashSet<Listener> _listeners = new();
        private readonly HashSet<KeyBindingSettingGroup> _groups = new();

        public void ApplySetting(ISettingsService service, string id) {
            if (!PrepareRebinding(out _, out bool actionEnabled, out int index)) return;
            
            string controlPath = service.TryGet(id, 0, out string p) ? p : null; 
            ApplyBinding(id, index, controlPath);
            FinishRebinding(actionEnabled);
        }

        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<string>(id, 0);
        }

        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out string value)) {
                service.Set(id, 0, value);
            }
        }

        public void AddBindingListener(Listener listener) {
            _listeners.Add(listener);
        }

        public void RemoveBindingListener(Listener listener) {
            _listeners.Remove(listener);
        }

        public void AddToGroup(KeyBindingSettingGroup group) {
            _groups.Add(group);
        }

        public void RemoveFromGroup(KeyBindingSettingGroup group) {
            _groups.Remove(group);
        }

        public HashSet<KeyBindingSettingGroup> GetKeyBindingGroups() {
            return _groups;
        }

        public InputDeviceType GetDeviceType() {
            return deviceType;
        }
        
        public LocalizationKey GetName() {
            return name;
        }

        public InputBinding GetBinding() {
            return inputActionRef.Get().bindings[bindingIndex];
        }

        public bool TryGetBinding(out InputAction action, out InputBinding binding, out int bindingIndex) {
            action = inputActionRef.Get();
            binding = default;
            bindingIndex = this.bindingIndex;
            
            if (action == null) {
                Debug.LogError($"KeyBindingSetting.ApplyBinding: f {Time.frameCount}, input action is null in {nameof(KeyBindingSetting)} with id [{name}]");
                return false;
            }

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) {
                Debug.LogError($"KeyBindingSetting.ApplyBinding: f {Time.frameCount}, trying to set incorrect binding index [{bindingIndex}] for " +
                               $"input action [{action.actionMap.name}/{action.name}] in {nameof(KeyBindingSetting)} with id [{name}]");
                return false;
            }

            binding = action.bindings[bindingIndex];
            
            if (binding.isComposite) {
                Debug.LogError($"KeyBindingSetting.ApplyBinding: f {Time.frameCount}, trying to set composite binding with index [{bindingIndex}] for " + 
                               $"input action [{action.actionMap.name}/{action.name}] in {nameof(KeyBindingSetting)} with id [{name}]. " + 
                               "Setting composite binding is not allowed, select valid physical binding index.");
                return false;
            }

            return true;
        }

        public bool PrepareRebinding(out InputAction action, out bool actionEnabled, out int bindingIndex) {
            if (!TryGetBinding(out action, out _, out bindingIndex)) {
                actionEnabled = false;
                return false;
            }
            
            actionEnabled = action.enabled;
            action.Disable();
            return true;
        }

        public bool ApplyRebinding(ISettingsService service, string id, string path) {
            if (!IsValidBinding(path)) return false;
            
            service.Set(id, 0, path);
            ApplyBinding(id, bindingIndex, path);
            
            return true;
        }

        public void FinishRebinding(bool enableAction) {
            if (enableAction) inputActionRef.Get().Enable();
        }

        public void ClearBinding(ISettingsService service, string id) {
            if (!PrepareRebinding(out var action, out bool actionEnabled, out int index)) return;
            
            const string path = "";
            
            service.Set(id, 0, path);
            ApplyBinding(id, index, path);
            
            if (actionEnabled) action.Enable();
        }

        private void ApplyBinding(string id, int index, string controlPath) {
            var action = inputActionRef.Get();
            
            if (controlPath != null) action.ApplyBindingOverride(index, controlPath);
            else action.RemoveBindingOverride(index);
            
            foreach (var bindingListener in _listeners) {
                bindingListener.Invoke(id, action, index, controlPath);
            }
        }

        private bool IsValidBinding(string controlPath) {
            return !string.IsNullOrWhiteSpace(controlPath) && 
                   (validator?.IsMatch(controlPath) ?? true);
        }
    }
    
}