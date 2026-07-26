using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Inputs;
using MisterGames.Input.Core;
using MisterGames.Input.Icons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Bindings {
    
    public static class InputBindingExtensions {
    
        public static InputControl GetControl(this KeyBinding key) {
            return InputServices.BindingHelper?.GetControl(key);
        }
        
        public static InputControl GetControl(this AxisBinding axis) {
            return InputServices.BindingHelper?.GetControl(axis);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPressed(this KeyBinding key) {
            return InputServices.BindingHelper?.IsKeyPressed(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasPressedThisFrame(this KeyBinding key) {
            return InputServices.BindingHelper?.WasKeyPressedThisFrame(key) ?? false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasReleasedThisFrame(this KeyBinding key) {
            return InputServices.BindingHelper?.WasKeyReleasedThisFrame(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetVector(this AxisBinding axis) {
            return InputServices.BindingHelper?.GetAxisValue(axis) ?? default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ArePressed(this ShortcutModifiers key) {
            return InputServices.BindingHelper?.AreModifiersPressed(key) ?? false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPressCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.AddKeyPressCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemovePressCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.RemoveKeyPressCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddReleaseCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.AddKeyReleaseCallback(key, callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveReleaseCallback(this KeyBinding key, Action callback) {
            InputServices.BindingHelper?.RemoveKeyReleaseCallback(key, callback);
        }
        
        public static string GetBindingPath(this KeyBinding keyBinding) {
            return keyBinding switch {
                KeyBinding.None => null,
                KeyBinding.LeftShift => "<Keyboard>/leftShift",
                KeyBinding.RightShift => "<Keyboard>/rightShift",
                KeyBinding.LeftAlt => "<Keyboard>/leftAlt",
                KeyBinding.RightAlt => "<Keyboard>/rightAlt",
                KeyBinding.LeftControl => "<Keyboard>/leftCtrl",
                KeyBinding.RightControl => "<Keyboard>/rightCtrl",
                KeyBinding.LeftCommand => "<Keyboard>/leftCommand",
                KeyBinding.RightCommand => "<Keyboard>/rightCommand",
                KeyBinding.Space => "<Keyboard>/space",
                KeyBinding.Enter => "<Keyboard>/enter",
                KeyBinding.Tab => "<Keyboard>/tab",
                KeyBinding.Backquote => "<Keyboard>/backquote",
                KeyBinding.Quote => "<Keyboard>/quote",
                KeyBinding.Semicolon => "<Keyboard>/semicolon",
                KeyBinding.Comma => "<Keyboard>/comma",
                KeyBinding.Period => "<Keyboard>/period",
                KeyBinding.Slash => "<Keyboard>/slash",
                KeyBinding.Backslash => "<Keyboard>/backslash",
                KeyBinding.LeftBracket => "<Keyboard>/leftBracket",
                KeyBinding.RightBracket => "<Keyboard>/rightBracket",
                KeyBinding.Minus => "<Keyboard>/minus",
                KeyBinding.Equals => "<Keyboard>/equals",
                KeyBinding.Escape => "<Keyboard>/escape",
                KeyBinding.Backspace => "<Keyboard>/backspace",
                KeyBinding.CapsLock => "<Keyboard>/capsLock",
                KeyBinding.NumLock => "<Keyboard>/numLock",
                KeyBinding.ScrollLock => "<Keyboard>/scrollLock",
                KeyBinding.PageUp => "<Keyboard>/pageUp",
                KeyBinding.PageDown => "<Keyboard>/pageDown",
                KeyBinding.Home => "<Keyboard>/home",
                KeyBinding.End => "<Keyboard>/end",
                KeyBinding.Insert => "<Keyboard>/insert",
                KeyBinding.Delete => "<Keyboard>/delete",
                KeyBinding.PrintScreen => "<Keyboard>/printScreen",
                KeyBinding.Pause => "<Keyboard>/pause",
                KeyBinding.NumEnter => "<Keyboard>/numpadEnter",
                KeyBinding.NumPlus => "<Keyboard>/numpadPlus",
                KeyBinding.NumEquals => "<Keyboard>/numpadEquals",
                KeyBinding.NumMinus => "<Keyboard>/numpadMinus",
                KeyBinding.NumDivide => "<Keyboard>/numpadDivide",
                KeyBinding.NumMultiply => "<Keyboard>/numpadMultiply",
                KeyBinding.NumPeriod => "<Keyboard>/numpadPeriod",
                KeyBinding.ArrowLeft => "<Keyboard>/leftArrow",
                KeyBinding.ArrowRight => "<Keyboard>/rightArrow",
                KeyBinding.ArrowUp => "<Keyboard>/upArrow",
                KeyBinding.ArrowDown => "<Keyboard>/downArrow",
                KeyBinding.A => "<Keyboard>/a",
                KeyBinding.B => "<Keyboard>/b",
                KeyBinding.C => "<Keyboard>/c",
                KeyBinding.D => "<Keyboard>/d",
                KeyBinding.E => "<Keyboard>/e",
                KeyBinding.F => "<Keyboard>/f",
                KeyBinding.G => "<Keyboard>/g",
                KeyBinding.H => "<Keyboard>/h",
                KeyBinding.I => "<Keyboard>/i",
                KeyBinding.J => "<Keyboard>/j",
                KeyBinding.K => "<Keyboard>/k",
                KeyBinding.L => "<Keyboard>/l",
                KeyBinding.M => "<Keyboard>/m",
                KeyBinding.N => "<Keyboard>/n",
                KeyBinding.O => "<Keyboard>/o",
                KeyBinding.P => "<Keyboard>/p",
                KeyBinding.Q => "<Keyboard>/q",
                KeyBinding.R => "<Keyboard>/r",
                KeyBinding.S => "<Keyboard>/s",
                KeyBinding.T => "<Keyboard>/t",
                KeyBinding.U => "<Keyboard>/u",
                KeyBinding.V => "<Keyboard>/v",
                KeyBinding.W => "<Keyboard>/w",
                KeyBinding.X => "<Keyboard>/x",
                KeyBinding.Y => "<Keyboard>/y",
                KeyBinding.Z => "<Keyboard>/z",
                KeyBinding.Digit0 => "<Keyboard>/0",
                KeyBinding.Digit1 => "<Keyboard>/1",
                KeyBinding.Digit2 => "<Keyboard>/2",
                KeyBinding.Digit3 => "<Keyboard>/3",
                KeyBinding.Digit4 => "<Keyboard>/4",
                KeyBinding.Digit5 => "<Keyboard>/5",
                KeyBinding.Digit6 => "<Keyboard>/6",
                KeyBinding.Digit7 => "<Keyboard>/7",
                KeyBinding.Digit8 => "<Keyboard>/8",
                KeyBinding.Digit9 => "<Keyboard>/9",
                KeyBinding.Num0 => "<Keyboard>/numpad0",
                KeyBinding.Num1 => "<Keyboard>/numpad1",
                KeyBinding.Num2 => "<Keyboard>/numpad2",
                KeyBinding.Num3 => "<Keyboard>/numpad3",
                KeyBinding.Num4 => "<Keyboard>/numpad4",
                KeyBinding.Num5 => "<Keyboard>/numpad5",
                KeyBinding.Num6 => "<Keyboard>/numpad6",
                KeyBinding.Num7 => "<Keyboard>/numpad7",
                KeyBinding.Num8 => "<Keyboard>/numpad8",
                KeyBinding.Num9 => "<Keyboard>/numpad9",
                KeyBinding.F1 => "<Keyboard>/f1",
                KeyBinding.F2 => "<Keyboard>/f2",
                KeyBinding.F3 => "<Keyboard>/f3",
                KeyBinding.F4 => "<Keyboard>/f4",
                KeyBinding.F5 => "<Keyboard>/f5",
                KeyBinding.F6 => "<Keyboard>/f6",
                KeyBinding.F7 => "<Keyboard>/f7",
                KeyBinding.F8 => "<Keyboard>/f8",
                KeyBinding.F9 => "<Keyboard>/f9",
                KeyBinding.F10 => "<Keyboard>/f10",
                KeyBinding.F11 => "<Keyboard>/f11",
                KeyBinding.F12 => "<Keyboard>/f12",
                KeyBinding.MouseLeft => "<Mouse>/leftButton",
                KeyBinding.MouseRight => "<Mouse>/rightButton",
                KeyBinding.MouseMiddle => "<Mouse>/middleButton",
                KeyBinding.MouseForward => "<Mouse>/forwardButton",
                KeyBinding.MouseBack => "<Mouse>/backButton",
                KeyBinding.GamepadSouth => "<Gamepad>/buttonSouth",
                KeyBinding.GamepadEast => "<Gamepad>/buttonEast",
                KeyBinding.GamepadWest => "<Gamepad>/buttonWest",
                KeyBinding.GamepadNorth => "<Gamepad>/buttonNorth",
                KeyBinding.GamepadLeft => "<Gamepad>/dpad/left",
                KeyBinding.GamepadRight => "<Gamepad>/dpad/right",
                KeyBinding.GamepadUp => "<Gamepad>/dpad/up",
                KeyBinding.GamepadDown => "<Gamepad>/dpad/down",
                KeyBinding.GamepadBumperLeft => "<Gamepad>/leftShoulder",
                KeyBinding.GamepadBumperRight => "<Gamepad>/rightShoulder",
                KeyBinding.GamepadTriggerLeft => "<Gamepad>/leftTrigger",
                KeyBinding.GamepadTriggerRight => "<Gamepad>/rightTrigger",
                KeyBinding.GamepadStickButtonLeft => "<Gamepad>/leftStickPress",
                KeyBinding.GamepadStickButtonRight => "<Gamepad>/rightStickPress",
                KeyBinding.GamepadSelect => "<Gamepad>/select",
                KeyBinding.GamepadStart => "<Gamepad>/start",
                _ => throw new ArgumentOutOfRangeException(nameof(keyBinding), keyBinding, null)
            };
        }

        public static string GetBindingPath(this AxisBinding axisBinding, AxisBingingDirection dir = AxisBingingDirection.Default) {
            string path = axisBinding switch {
                AxisBinding.MousePosition => "<Mouse>/position",
                AxisBinding.MouseDelta => "<Mouse>/delta",
                AxisBinding.MouseScroll => "<Mouse>/scroll",
                AxisBinding.GamepadStickLeft => "<Gamepad>/leftStick",
                AxisBinding.GamepadStickRight => "<Gamepad>/rightStick",
                AxisBinding.GamepadDpad => "<Gamepad>/dpad",
                _ => throw new ArgumentOutOfRangeException(nameof(axisBinding), axisBinding, null)
            };

            return dir switch {
                AxisBingingDirection.Default => path,
                AxisBingingDirection.Up => $"{path}/up",
                AxisBingingDirection.Down => $"{path}/down",
                AxisBingingDirection.Left => $"{path}/left",
                AxisBingingDirection.Right => $"{path}/right",
                AxisBingingDirection.UpDown => $"{path}/y",
                AxisBingingDirection.LeftRight => $"{path}/x",
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }
    }

}