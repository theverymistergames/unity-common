using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
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
        [SerializeReference] [SubclassSelector] public IBindingValidator validator;
        
        public delegate void BindingListener(string id, InputAction action, int bindingIndex, string path);
        
        private readonly HashSet<BindingListener> _bindingListeners = new();
        private readonly HashSet<KeyBindingSettingGroup> _groups = new();
        
        public void Initialize(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out string controlPath) && 
                PrepareBinding(out _, out bool actionEnabled, out int index)) 
            {
                ApplyBinding(index, controlPath, actionEnabled);
            }
        }
        
        public void Deinitialize(ISettingsService service, string id) {
            
        }

        public void AddBindingListener(BindingListener listener) {
            _bindingListeners.Add(listener);
        }

        public void RemoveBindingListener(BindingListener listener) {
            _bindingListeners.Remove(listener);
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

        public bool PrepareBinding(out InputAction action, out bool actionEnabled, out int bindingIndex) {
            if (!TryGetBinding(out action, out _, out bindingIndex)) {
                actionEnabled = false;
                return false;
            }
            
            actionEnabled = action.enabled;
            action.Disable();
            return true;
        }

        public bool SetBinding(ISettingsService service, string id, string path, bool enableAction) {
            if (!IsValidBinding(path)) return false;
            
            service.Set(id, 0, path);
            ApplyBinding(bindingIndex, path, enableAction);
            var action = inputActionRef.Get();
            
            foreach (var bindingListener in _bindingListeners) {
                bindingListener.Invoke(id, action, bindingIndex, path);
            }
            
            return true;
        }

        public void ClearBinding(ISettingsService service, string id) {
            if (!PrepareBinding(out var action, out bool actionEnabled, out int index)) return;
            
            const string path = "";
            
            service.Set(id, 0, path);
            ApplyBinding(index, path, actionEnabled);
            
            foreach (var bindingListener in _bindingListeners) {
                bindingListener.Invoke(id, action, index, path);
            }
        }

        private void ApplyBinding(int index, string controlPath, bool enableAction) {
            var action = inputActionRef.Get();
            action.ApplyBindingOverride(index, controlPath);
            if (enableAction) action.Enable();
        }

        private bool IsValidBinding(string controlPath) {
            return validator?.IsMatch(controlPath) ?? true;
        }
    }
    
}