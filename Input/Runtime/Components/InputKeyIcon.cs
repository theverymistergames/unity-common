using System;
using MisterGames.Common.Data;
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

        [Header("Debug")]
        [SerializeField] private Optional<InputDeviceType> _debugDevice;

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
            var action = _inputAction.Get();
            if (action == null) return;

            int bindingIndex = GetBindingIndex(action, deviceType);
            if (bindingIndex < 0) return;

            var gamepadType = Services.TryGet(out IDeviceService deviceService) ? deviceService.GamepadType : GamepadType.Default;
            var sprite = _iconsTable.GetSprite(action.bindings[bindingIndex], gamepadType);
            if (_image.sprite == sprite) return;

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
        private void OnValidate() {
            EditorApplication.delayCall -= UpdateIconInEditor;
            EditorApplication.delayCall += UpdateIconInEditor;
        }

        private void UpdateIconInEditor() {
            if (this == null || _image == null || _iconsTable == null) return;

            var deviceType = Services.TryGet(out IDeviceService deviceService)
                ? deviceService.CurrentDevice
                : InputDeviceType.KeyboardMouse;

            OnDeviceChanged(_debugDevice.GetOrDefault(deviceType));
        }
#endif
    }

}
