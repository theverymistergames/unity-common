using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Input.Actions;
using MisterGames.Input.Icons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Input.Components {

    public sealed class InputKeyIcon : MonoBehaviour {

        [SerializeField] private InputActionRef _inputAction;
        [SerializeField] private InputIconsTable _iconsTable;
        [SerializeField] private Image _image;
        [SerializeField] [Min(-1)] private int _bindingIndexKeyboardMouse = -1;
        [SerializeField] [Min(-1)] private int _bindingIndexGamepad = -1;

        private const string KeyboardLayout = "Keyboard";
        private const string PointerLayout = "Pointer";
        private const string GamepadLayout = "Gamepad";

        private void OnEnable() {
            InputSystem.onActionChange += OnActionChange;

            if (!Services.TryGet(out IDeviceService deviceService)) return;

            deviceService.OnDeviceChanged += OnDeviceChanged;
            OnDeviceChanged(deviceService.CurrentDevice);
        }

        private void OnDisable() {
            InputSystem.onActionChange -= OnActionChange;

            if (!Services.TryGet(out IDeviceService deviceService)) return;

            deviceService.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnActionChange(object obj, InputActionChange change) {
            if (change != InputActionChange.BoundControlsChanged ||
                !Services.TryGet(out IDeviceService deviceService))
            {
                return;
            }

            var action = _inputAction.Get();
            if (action == null ||
                obj != action && obj != action.actionMap && obj != action.actionMap?.asset)
            {
                return;
            }

            OnDeviceChanged(deviceService.CurrentDevice);
        }

        private void OnDeviceChanged(InputDeviceType deviceType) {
            if (!TryGetBinding(deviceType, out var binding)) return;

            var sprite = GetSprite(binding);
            if (_image.sprite == sprite) return;

            SetSprite(sprite);
        }

        private bool TryGetBinding(InputDeviceType deviceType, out InputBinding binding) {
            var action = _inputAction.Get();
            int bindingIndex = action == null ? -1 : GetBindingIndex(action, deviceType);

            binding = bindingIndex >= 0 ? action!.bindings[bindingIndex] : default;
            return bindingIndex >= 0;
        }

        private Sprite GetSprite(InputBinding binding) {
            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            return _iconsTable.GetSprite(binding, gamepadType);
        }

        private void SetSprite(Sprite sprite) {
            _image.sprite = sprite;

#if UNITY_EDITOR
            EditorUtility.SetDirty(_image);
#endif
        }

        private int GetBindingIndex(InputAction action, InputDeviceType deviceType) {
            int bindingIndex = deviceType switch {
                InputDeviceType.KeyboardMouse => _bindingIndexKeyboardMouse,
                InputDeviceType.Gamepad => _bindingIndexGamepad,
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };

            if (bindingIndex >= 0 && bindingIndex < action.bindings.Count) return bindingIndex;

            var bindings = action.bindings;

            for (int i = 0; i < bindings.Count; i++) {
                if (IsBindingMatchesDevice(bindings[i], deviceType)) return i;
            }

            return -1;
        }

        private static bool IsBindingMatchesDevice(InputBinding binding, InputDeviceType deviceType) {
            string layout = binding.isComposite ? null : InputControlPath.TryGetDeviceLayout(binding.effectivePath);

            return !string.IsNullOrEmpty(layout) && deviceType switch {
                InputDeviceType.KeyboardMouse => InputSystem.IsFirstLayoutBasedOnSecond(layout, KeyboardLayout) ||
                                                 InputSystem.IsFirstLayoutBasedOnSecond(layout, PointerLayout),
                InputDeviceType.Gamepad => InputSystem.IsFirstLayoutBasedOnSecond(layout, GamepadLayout),
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private InputDeviceType _debugDevice;
        [SerializeField] [HideInInspector] private string _lastBindingPath;

        private void OnValidate() {
            var device = Services.TryGet(out IDeviceService deviceService)
                ? deviceService.CurrentDevice
                : _debugDevice;
                
            if (_image == null ||
                _iconsTable == null ||
                !TryGetBinding(device, out var binding) ||
                binding.effectivePath == _lastBindingPath)
            {
                return;
            }

            _lastBindingPath = binding.effectivePath;
            SetSprite(GetSprite(binding));
        }

        [Common.Attributes.Button]
        private void ForceRefreshSprite() {
            var device = Services.TryGet(out IDeviceService deviceService)
                ? deviceService.CurrentDevice
                : _debugDevice;
            
            if (_image == null ||
                _iconsTable == null ||
                !TryGetBinding(device, out var binding))
            {
                return;
            }

            _lastBindingPath = binding.effectivePath;
            SetSprite(GetSprite(binding));
        }
#endif
    }

}
