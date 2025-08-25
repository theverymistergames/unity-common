using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MisterGames.Input.Bindings {
    
    public sealed class InputBindingHelper : IInputBindingHelper, IDisposable, IUpdate {
        
        private readonly Dictionary<KeyBinding, ButtonControl> _keyBindingMap = new();
        private readonly Dictionary<AxisBinding, Vector2Control> _axisBindingMap = new();
    
        private readonly MultiValueDictionary<KeyBinding, Action> _keyPressCallbackMap = new();
        private readonly MultiValueDictionary<KeyBinding, Action> _keyReleaseCallbackMap = new();
        
        public void Initialize() {
            FetchKeyboardBindings();
            FetchMouseBindings();
            FetchGamepadBindings();
        }
        
        public void Dispose() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            _keyPressCallbackMap.Clear();
            _keyReleaseCallbackMap.Clear();
            
            ClearAllBindings();
        }
        
        public bool IsKeyPressed(KeyBinding key) {
            return _keyBindingMap.TryGetValue(key, out var control) && control.isPressed;
        }
        
        public bool WasKeyPressedThisFrame(KeyBinding key) {
            return _keyBindingMap.TryGetValue(key, out var control) && control.wasPressedThisFrame;
        }

        public bool WasKeyReleasedThisFrame(KeyBinding key) {
            return _keyBindingMap.TryGetValue(key, out var control) && control.wasReleasedThisFrame;
        }
        
        public Vector2 GetAxisValue(AxisBinding axis) {
            return _axisBindingMap.TryGetValue(axis, out var control) ? control.ReadValue() : default;
        }

        public bool AreModifiersPressed(ShortcutModifiers key) {
            if (key == ShortcutModifiers.None) return true;

            return ((key & ShortcutModifiers.Alt) != ShortcutModifiers.Alt || 
                    IsKeyPressed(KeyBinding.LeftAlt) || IsKeyPressed(KeyBinding.RightAlt)) &&
                   
                   ((key & ShortcutModifiers.Action) != ShortcutModifiers.Action || 
                    IsKeyPressed(KeyBinding.LeftControl) || IsKeyPressed(KeyBinding.RightControl)) && 
                   
                   ((key & ShortcutModifiers.Shift) != ShortcutModifiers.Shift || 
                    IsKeyPressed(KeyBinding.LeftShift) || IsKeyPressed(KeyBinding.RightShift)) &&
                   
                   ((key & ShortcutModifiers.Control) != ShortcutModifiers.Control || 
                    IsKeyPressed(KeyBinding.LeftControl) || IsKeyPressed(KeyBinding.RightControl));
        }

        public void AddKeyPressCallback(KeyBinding keyBinding, Action callback) {
            if (callback == null) return;
            
            _keyPressCallbackMap.AddValue(keyBinding, callback);
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }
        
        public void RemoveKeyPressCallback(KeyBinding keyBinding, Action callback) {
            _keyPressCallbackMap.RemoveValue(keyBinding, callback);
            
            if (!HasSubscribedCallbacks()) PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public void AddKeyReleaseCallback(KeyBinding keyBinding, Action callback) {
            if (callback == null) return;
            
            _keyReleaseCallbackMap.AddValue(keyBinding, callback);
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }
        
        public void RemoveKeyReleaseCallback(KeyBinding keyBinding, Action callback) {
            _keyReleaseCallbackMap.RemoveValue(keyBinding, callback);
            
            if (!HasSubscribedCallbacks()) PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }
        
        void IUpdate.OnUpdate(float dt) {
            ProcessPressCallbacks();
            ProcessReleaseCallbacks();
        }

        private bool HasSubscribedCallbacks() {
            return _keyPressCallbackMap.Count > 0 || _keyReleaseCallbackMap.Count > 0;
        }
        
        private void ProcessPressCallbacks() {
            var keysBuffer = new NativeArray<KeyBinding>(_keyPressCallbackMap.Count, Allocator.Temp);
            int keyPressedCount = 0;
            
            foreach (var key in _keyPressCallbackMap.Keys) {
                if (WasKeyPressedThisFrame(key)) keysBuffer[keyPressedCount++] = key;
            }

            for (int i = 0; i < keyPressedCount; i++) {
                var key = keysBuffer[i];
                int count = _keyPressCallbackMap.GetCount(key);

                for (int j = count - 1; j >= 0; j--) {
                    _keyPressCallbackMap.GetValueAt(key, j)?.Invoke();
                }
            }

            keysBuffer.Dispose();
        }
        
        private void ProcessReleaseCallbacks() {
            var keysBuffer = new NativeArray<KeyBinding>(_keyReleaseCallbackMap.Count, Allocator.Temp);
            int keyReleasedCount = 0;
            
            foreach (var key in _keyReleaseCallbackMap.Keys) {
                if (WasKeyReleasedThisFrame(key)) keysBuffer[keyReleasedCount++] = key;
            }

            for (int i = 0; i < keyReleasedCount; i++) {
                var key = keysBuffer[i];
                int count = _keyReleaseCallbackMap.GetCount(key);

                for (int j = count - 1; j >= 0; j--) {
                    _keyReleaseCallbackMap.GetValueAt(key, j)?.Invoke();
                }
            }

            keysBuffer.Dispose();
        }
        
        private void FetchKeyboardBindings() {
            if (Keyboard.current is not { } keyboard) return;
            
            _keyBindingMap[KeyBinding.LeftShift] = keyboard.leftShiftKey;
            _keyBindingMap[KeyBinding.RightShift] = keyboard.rightShiftKey;
                
            _keyBindingMap[KeyBinding.LeftAlt] = keyboard.leftAltKey;
            _keyBindingMap[KeyBinding.RightAlt] = keyboard.rightAltKey;
                
            _keyBindingMap[KeyBinding.LeftControl] = keyboard.leftCtrlKey;
            _keyBindingMap[KeyBinding.RightControl] = keyboard.rightCtrlKey;
                
            _keyBindingMap[KeyBinding.LeftCommand] = keyboard.leftCommandKey;
            _keyBindingMap[KeyBinding.RightCommand] = keyboard.rightCommandKey;

            _keyBindingMap[KeyBinding.Space] = keyboard.spaceKey;
            _keyBindingMap[KeyBinding.Enter] = keyboard.enterKey;
            _keyBindingMap[KeyBinding.Tab] = keyboard.tabKey;
            _keyBindingMap[KeyBinding.Backquote] = keyboard.backquoteKey;
            _keyBindingMap[KeyBinding.Quote] = keyboard.quoteKey;
            _keyBindingMap[KeyBinding.Semicolon] = keyboard.semicolonKey;
            _keyBindingMap[KeyBinding.Comma] = keyboard.commaKey;
            _keyBindingMap[KeyBinding.Period] = keyboard.periodKey;
            _keyBindingMap[KeyBinding.Slash] = keyboard.slashKey;
            _keyBindingMap[KeyBinding.Backslash] = keyboard.backslashKey;
            _keyBindingMap[KeyBinding.LeftBracket] = keyboard.leftBracketKey;
            _keyBindingMap[KeyBinding.RightBracket] = keyboard.rightBracketKey;
            _keyBindingMap[KeyBinding.Minus] = keyboard.minusKey;
            _keyBindingMap[KeyBinding.Equals] = keyboard.equalsKey;

            _keyBindingMap[KeyBinding.Escape] = keyboard.escapeKey;
            _keyBindingMap[KeyBinding.Backspace] = keyboard.backspaceKey;
            _keyBindingMap[KeyBinding.CapsLock] = keyboard.capsLockKey;
            _keyBindingMap[KeyBinding.NumLock] = keyboard.numLockKey;
            _keyBindingMap[KeyBinding.ScrollLock] = keyboard.scrollLockKey;
            _keyBindingMap[KeyBinding.PageUp] = keyboard.pageUpKey;
            _keyBindingMap[KeyBinding.PageDown] = keyboard.pageDownKey;
            _keyBindingMap[KeyBinding.Home] = keyboard.homeKey;
            _keyBindingMap[KeyBinding.End] = keyboard.endKey;
            _keyBindingMap[KeyBinding.Insert] = keyboard.insertKey;
            _keyBindingMap[KeyBinding.Delete] = keyboard.deleteKey;
            _keyBindingMap[KeyBinding.PrintScreen] = keyboard.printScreenKey;
            _keyBindingMap[KeyBinding.Pause] = keyboard.pauseKey;
                
            _keyBindingMap[KeyBinding.NumEnter] = keyboard.numpadEnterKey;
            _keyBindingMap[KeyBinding.NumPlus] = keyboard.numpadPlusKey;
            _keyBindingMap[KeyBinding.NumEquals] = keyboard.numpadEqualsKey;
            _keyBindingMap[KeyBinding.NumMinus] = keyboard.numpadMinusKey;
            _keyBindingMap[KeyBinding.NumDivide] = keyboard.numpadDivideKey;
            _keyBindingMap[KeyBinding.NumMultiply] = keyboard.numpadMultiplyKey;
            _keyBindingMap[KeyBinding.NumPeriod] = keyboard.numpadPeriodKey;
                
            _keyBindingMap[KeyBinding.ArrowDown] = keyboard.downArrowKey;
            _keyBindingMap[KeyBinding.ArrowUp] = keyboard.upArrowKey;
            _keyBindingMap[KeyBinding.ArrowLeft] = keyboard.leftArrowKey;
            _keyBindingMap[KeyBinding.ArrowRight] = keyboard.rightArrowKey;
                
            _keyBindingMap[KeyBinding.A] = keyboard.aKey;
            _keyBindingMap[KeyBinding.B] = keyboard.bKey;
            _keyBindingMap[KeyBinding.C] = keyboard.cKey;
            _keyBindingMap[KeyBinding.D] = keyboard.dKey;
            _keyBindingMap[KeyBinding.E] = keyboard.eKey;
            _keyBindingMap[KeyBinding.F] = keyboard.fKey;
            _keyBindingMap[KeyBinding.G] = keyboard.gKey;
            _keyBindingMap[KeyBinding.H] = keyboard.hKey;
            _keyBindingMap[KeyBinding.I] = keyboard.iKey;
            _keyBindingMap[KeyBinding.J] = keyboard.jKey;
            _keyBindingMap[KeyBinding.K] = keyboard.kKey;
            _keyBindingMap[KeyBinding.L] = keyboard.lKey;
            _keyBindingMap[KeyBinding.M] = keyboard.mKey;
            _keyBindingMap[KeyBinding.N] = keyboard.nKey;
            _keyBindingMap[KeyBinding.O] = keyboard.oKey;
            _keyBindingMap[KeyBinding.P] = keyboard.pKey;
            _keyBindingMap[KeyBinding.Q] = keyboard.qKey;
            _keyBindingMap[KeyBinding.R] = keyboard.rKey;
            _keyBindingMap[KeyBinding.S] = keyboard.sKey;
            _keyBindingMap[KeyBinding.T] = keyboard.tKey;
            _keyBindingMap[KeyBinding.U] = keyboard.uKey;
            _keyBindingMap[KeyBinding.V] = keyboard.vKey;
            _keyBindingMap[KeyBinding.W] = keyboard.wKey;
            _keyBindingMap[KeyBinding.X] = keyboard.xKey;
            _keyBindingMap[KeyBinding.Y] = keyboard.yKey;
            _keyBindingMap[KeyBinding.Z] = keyboard.zKey;    
                
            _keyBindingMap[KeyBinding.Digit0] = keyboard.digit0Key;
            _keyBindingMap[KeyBinding.Digit1] = keyboard.digit1Key;
            _keyBindingMap[KeyBinding.Digit2] = keyboard.digit2Key;
            _keyBindingMap[KeyBinding.Digit3] = keyboard.digit3Key;
            _keyBindingMap[KeyBinding.Digit4] = keyboard.digit4Key;
            _keyBindingMap[KeyBinding.Digit5] = keyboard.digit5Key;
            _keyBindingMap[KeyBinding.Digit6] = keyboard.digit6Key;
            _keyBindingMap[KeyBinding.Digit7] = keyboard.digit7Key;
            _keyBindingMap[KeyBinding.Digit8] = keyboard.digit8Key;
            _keyBindingMap[KeyBinding.Digit9] = keyboard.digit9Key;
                
            _keyBindingMap[KeyBinding.Num0] = keyboard.numpad0Key;
            _keyBindingMap[KeyBinding.Num1] = keyboard.numpad1Key;
            _keyBindingMap[KeyBinding.Num2] = keyboard.numpad2Key;
            _keyBindingMap[KeyBinding.Num3] = keyboard.numpad3Key;
            _keyBindingMap[KeyBinding.Num4] = keyboard.numpad4Key;
            _keyBindingMap[KeyBinding.Num5] = keyboard.numpad5Key;
            _keyBindingMap[KeyBinding.Num6] = keyboard.numpad6Key;
            _keyBindingMap[KeyBinding.Num7] = keyboard.numpad7Key;
            _keyBindingMap[KeyBinding.Num8] = keyboard.numpad8Key;
            _keyBindingMap[KeyBinding.Num9] = keyboard.numpad9Key;
                
            _keyBindingMap[KeyBinding.F1] = keyboard.f1Key;
            _keyBindingMap[KeyBinding.F2] = keyboard.f2Key;
            _keyBindingMap[KeyBinding.F3] = keyboard.f3Key;
            _keyBindingMap[KeyBinding.F4] = keyboard.f4Key;
            _keyBindingMap[KeyBinding.F5] = keyboard.f5Key;
            _keyBindingMap[KeyBinding.F6] = keyboard.f6Key;
            _keyBindingMap[KeyBinding.F7] = keyboard.f7Key;
            _keyBindingMap[KeyBinding.F8] = keyboard.f8Key;
            _keyBindingMap[KeyBinding.F9] = keyboard.f9Key;
            _keyBindingMap[KeyBinding.F10] = keyboard.f10Key;
            _keyBindingMap[KeyBinding.F11] = keyboard.f11Key;
            _keyBindingMap[KeyBinding.F12] = keyboard.f12Key;
        }

        private void FetchMouseBindings() {
            if (Mouse.current is not { } mouse) return;

            _keyBindingMap[KeyBinding.MouseLeft] = mouse.leftButton;
            _keyBindingMap[KeyBinding.MouseMiddle] = mouse.middleButton;
            _keyBindingMap[KeyBinding.MouseRight] = mouse.rightButton;
            _keyBindingMap[KeyBinding.MouseForward] = mouse.forwardButton;
            _keyBindingMap[KeyBinding.MouseBack] = mouse.backButton;
            
            _axisBindingMap[AxisBinding.MousePosition] = mouse.position;
            _axisBindingMap[AxisBinding.MouseDelta] = mouse.delta;
            _axisBindingMap[AxisBinding.MouseScroll] = mouse.scroll;
        }

        private void FetchGamepadBindings() {
            if (Gamepad.current is not { } gamepad) return;

            _keyBindingMap[KeyBinding.GamepadSouth] = gamepad.aButton;
            _keyBindingMap[KeyBinding.GamepadEast] = gamepad.bButton;
            _keyBindingMap[KeyBinding.GamepadWest] = gamepad.xButton;
            _keyBindingMap[KeyBinding.GamepadNorth] = gamepad.yButton;
            
            _keyBindingMap[KeyBinding.GamepadUp] = gamepad.dpad.up;
            _keyBindingMap[KeyBinding.GamepadDown] = gamepad.dpad.down;
            _keyBindingMap[KeyBinding.GamepadLeft] = gamepad.dpad.left;
            _keyBindingMap[KeyBinding.GamepadRight] = gamepad.dpad.right;
            
            _keyBindingMap[KeyBinding.GamepadBumperLeft] = gamepad.leftShoulder;
            _keyBindingMap[KeyBinding.GamepadBumperRight] = gamepad.rightShoulder;
            _keyBindingMap[KeyBinding.GamepadTriggerLeft] = gamepad.leftTrigger;
            _keyBindingMap[KeyBinding.GamepadTriggerRight] = gamepad.rightTrigger;
            _keyBindingMap[KeyBinding.GamepadStickButtonLeft] = gamepad.leftStickButton;
            _keyBindingMap[KeyBinding.GamepadStickButtonRight] = gamepad.rightStickButton;
            
            _keyBindingMap[KeyBinding.GamepadStart] = gamepad.startButton;
            _keyBindingMap[KeyBinding.GamepadSelect] = gamepad.selectButton;
            
            _axisBindingMap[AxisBinding.GamepadStickLeft] = gamepad.leftStick;
            _axisBindingMap[AxisBinding.GamepadStickRight] = gamepad.rightStick;
        }

        private void ClearAllBindings() {
            _keyBindingMap.Clear();
            _axisBindingMap.Clear();
        }
    }
    
}