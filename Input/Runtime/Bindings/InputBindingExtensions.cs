using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string deviceLayoutName, string controlPath) GetBindingPath(this KeyBinding keyBinding) {
            return KeyBindingToPathMap.GetValueOrDefault(keyBinding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyBinding ToKeyBinding(string controlPath) {
            return PathToKeyBindingMap.GetValueOrDefault(controlPath, KeyBinding.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (AxisBinding, AxisBingingDirection) ToAxisBinding(string controlPath) {
            return PathToAxisBindingMap.GetValueOrDefault(controlPath, (AxisBinding.None, AxisBingingDirection.Default));
        }
        
        public static (string deviceLayoutName, string controlPath) GetBindingPath(
            this AxisBinding axisBinding,
            AxisBingingDirection dir = AxisBingingDirection.Default) 
        {
            (string deviceLayoutName, string controlPath) = AxisBindingToPathMap.GetValueOrDefault(axisBinding);

            controlPath = dir switch {
                AxisBingingDirection.Default => controlPath,
                AxisBingingDirection.Up => $"{controlPath}/up",
                AxisBingingDirection.Down => $"{controlPath}/down",
                AxisBingingDirection.Left => $"{controlPath}/left",
                AxisBingingDirection.Right => $"{controlPath}/right",
                AxisBingingDirection.Y => $"{controlPath}/y",
                AxisBingingDirection.X => $"{controlPath}/x",
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            return (deviceLayoutName, controlPath);
        }
        
        public static bool SplitFullInputPath(string path, out string deviceLayoutName, out string controlPath) {
            deviceLayoutName = null;
            controlPath = null;
            
            if (string.IsNullOrWhiteSpace(path)) return false;

            int closingBracket = path.IndexOf('>');

            if (!path.StartsWith("<") ||
                closingBracket <= 1 ||
                closingBracket + 1 >= path.Length ||
                path[closingBracket + 1] != '/')
            {
                return false;
            }

            deviceLayoutName = path[1..closingBracket];
            controlPath = path[(closingBracket + 2)..];

            return !string.IsNullOrEmpty(controlPath);
        }

        private static readonly Dictionary<AxisBinding, (string, string)> AxisBindingToPathMap = new() {
            { AxisBinding.MousePosition, ("Mouse", "position") },
            { AxisBinding.MouseDelta, ("Mouse", "delta") },
            { AxisBinding.MouseScroll, ("Mouse", "scroll") },
            { AxisBinding.GamepadStickLeft, ("Gamepad", "leftStick") },
            { AxisBinding.GamepadStickRight, ("Gamepad", "rightStick") },
            { AxisBinding.GamepadDpad, ("Gamepad", "dpad") },
        };
        
        private static readonly Dictionary<string, (AxisBinding axis, AxisBingingDirection dir)> PathToAxisBindingMap = new() {
            { "<Mouse>/position", (AxisBinding.MousePosition, AxisBingingDirection.Default) },
            { "<Mouse>/position/x", (AxisBinding.MousePosition, AxisBingingDirection.X) },
            { "<Mouse>/position/y", (AxisBinding.MousePosition, AxisBingingDirection.Y) },
            { "<Mouse>/position/left", (AxisBinding.MousePosition, AxisBingingDirection.Left) },
            { "<Mouse>/position/right", (AxisBinding.MousePosition, AxisBingingDirection.Right) },
            { "<Mouse>/position/up", (AxisBinding.MousePosition, AxisBingingDirection.Up) },
            { "<Mouse>/position/down", (AxisBinding.MousePosition, AxisBingingDirection.Down) },
            { "<Mouse>/delta", (AxisBinding.MouseDelta, AxisBingingDirection.Default) },
            { "<Mouse>/delta/x", (AxisBinding.MouseDelta, AxisBingingDirection.X) },
            { "<Mouse>/delta/y", (AxisBinding.MouseDelta, AxisBingingDirection.Y) },
            { "<Mouse>/delta/left", (AxisBinding.MouseDelta, AxisBingingDirection.Left) },
            { "<Mouse>/delta/right", (AxisBinding.MouseDelta, AxisBingingDirection.Right) },
            { "<Mouse>/delta/up", (AxisBinding.MouseDelta, AxisBingingDirection.Up) },
            { "<Mouse>/delta/down", (AxisBinding.MouseDelta, AxisBingingDirection.Down) },
            { "<Mouse>/scroll", (AxisBinding.MouseScroll, AxisBingingDirection.Default) },
            { "<Mouse>/scroll/x", (AxisBinding.MouseScroll, AxisBingingDirection.X) },
            { "<Mouse>/scroll/y", (AxisBinding.MouseScroll, AxisBingingDirection.Y) },
            { "<Mouse>/scroll/left", (AxisBinding.MouseScroll, AxisBingingDirection.Left) },
            { "<Mouse>/scroll/right", (AxisBinding.MouseScroll, AxisBingingDirection.Right) },
            { "<Mouse>/scroll/up", (AxisBinding.MouseScroll, AxisBingingDirection.Up) },
            { "<Mouse>/scroll/down", (AxisBinding.MouseScroll, AxisBingingDirection.Down) },
            { "<Gamepad>/leftStick", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Default) },
            { "<Gamepad>/leftStick/x", (AxisBinding.GamepadStickLeft, AxisBingingDirection.X) },
            { "<Gamepad>/leftStick/y", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Y) },
            { "<Gamepad>/leftStick/left", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Left) },
            { "<Gamepad>/leftStick/right", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Right) },
            { "<Gamepad>/leftStick/up", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Up) },
            { "<Gamepad>/leftStick/down", (AxisBinding.GamepadStickLeft, AxisBingingDirection.Down) },
            { "<Gamepad>/rightStick", (AxisBinding.GamepadStickRight, AxisBingingDirection.Default) },
            { "<Gamepad>/rightStick/x", (AxisBinding.GamepadStickRight, AxisBingingDirection.X) },
            { "<Gamepad>/rightStick/y", (AxisBinding.GamepadStickRight, AxisBingingDirection.Y) },
            { "<Gamepad>/rightStick/left", (AxisBinding.GamepadStickRight, AxisBingingDirection.Left) },
            { "<Gamepad>/rightStick/right", (AxisBinding.GamepadStickRight, AxisBingingDirection.Right) },
            { "<Gamepad>/rightStick/up", (AxisBinding.GamepadStickRight, AxisBingingDirection.Up) },
            { "<Gamepad>/rightStick/down", (AxisBinding.GamepadStickRight, AxisBingingDirection.Down) },
            { "<Gamepad>/dpad", (AxisBinding.GamepadDpad, AxisBingingDirection.Default) },
            { "<Gamepad>/dpad/x", (AxisBinding.GamepadDpad, AxisBingingDirection.X) },
            { "<Gamepad>/dpad/y", (AxisBinding.GamepadDpad, AxisBingingDirection.Y) },
            { "<Gamepad>/dpad/left", (AxisBinding.GamepadDpad, AxisBingingDirection.Left) },
            { "<Gamepad>/dpad/right", (AxisBinding.GamepadDpad, AxisBingingDirection.Right) },
            { "<Gamepad>/dpad/up", (AxisBinding.GamepadDpad, AxisBingingDirection.Up) },
            { "<Gamepad>/dpad/down", (AxisBinding.GamepadDpad, AxisBingingDirection.Down) },
        };
        
        private static readonly Dictionary<KeyBinding, (string, string)> KeyBindingToPathMap = new() {
            { KeyBinding.LeftShift, ("Keyboard", "leftShift") },
            { KeyBinding.RightShift, ("Keyboard", "rightShift") },
            { KeyBinding.LeftAlt, ("Keyboard", "leftAlt") },
            { KeyBinding.RightAlt, ("Keyboard", "rightAlt") },
            { KeyBinding.LeftControl, ("Keyboard", "leftCtrl") },
            { KeyBinding.RightControl, ("Keyboard", "rightCtrl") },
            { KeyBinding.LeftCommand, ("Keyboard", "leftCommand") },
            { KeyBinding.RightCommand, ("Keyboard", "rightCommand") },
            { KeyBinding.Space, ("Keyboard", "space") },
            { KeyBinding.Enter, ("Keyboard", "enter") },
            { KeyBinding.Tab, ("Keyboard", "tab") },
            { KeyBinding.Backquote, ("Keyboard", "backquote") },
            { KeyBinding.Quote, ("Keyboard", "quote") },
            { KeyBinding.Semicolon, ("Keyboard", "semicolon") },
            { KeyBinding.Comma, ("Keyboard", "comma") },
            { KeyBinding.Period, ("Keyboard", "period") },
            { KeyBinding.Slash, ("Keyboard", "slash") },
            { KeyBinding.Backslash, ("Keyboard", "backslash") },
            { KeyBinding.LeftBracket, ("Keyboard", "leftBracket") },
            { KeyBinding.RightBracket, ("Keyboard", "rightBracket") },
            { KeyBinding.Minus, ("Keyboard", "minus") },
            { KeyBinding.Equals, ("Keyboard", "equals") },
            { KeyBinding.Escape, ("Keyboard", "escape") },
            { KeyBinding.Backspace, ("Keyboard", "backspace") },
            { KeyBinding.CapsLock, ("Keyboard", "capsLock") },
            { KeyBinding.NumLock, ("Keyboard", "numLock") },
            { KeyBinding.ScrollLock, ("Keyboard", "scrollLock") },
            { KeyBinding.PageUp, ("Keyboard", "pageUp") },
            { KeyBinding.PageDown, ("Keyboard", "pageDown") },
            { KeyBinding.Home, ("Keyboard", "home") },
            { KeyBinding.End, ("Keyboard", "end") },
            { KeyBinding.Insert, ("Keyboard", "insert") },
            { KeyBinding.Delete, ("Keyboard", "delete") },
            { KeyBinding.PrintScreen, ("Keyboard", "printScreen") },
            { KeyBinding.Pause, ("Keyboard", "pause") },
            { KeyBinding.NumEnter, ("Keyboard", "numpadEnter") },
            { KeyBinding.NumPlus, ("Keyboard", "numpadPlus") },
            { KeyBinding.NumEquals, ("Keyboard", "numpadEquals") },
            { KeyBinding.NumMinus, ("Keyboard", "numpadMinus") },
            { KeyBinding.NumDivide, ("Keyboard", "numpadDivide") },
            { KeyBinding.NumMultiply, ("Keyboard", "numpadMultiply") },
            { KeyBinding.NumPeriod, ("Keyboard", "numpadPeriod") },
            { KeyBinding.ArrowLeft, ("Keyboard", "leftArrow") },
            { KeyBinding.ArrowRight, ("Keyboard", "rightArrow") },
            { KeyBinding.ArrowUp, ("Keyboard", "upArrow") },
            { KeyBinding.ArrowDown, ("Keyboard", "downArrow") },
            { KeyBinding.A, ("Keyboard", "a") },
            { KeyBinding.B, ("Keyboard", "b") },
            { KeyBinding.C, ("Keyboard", "c") },
            { KeyBinding.D, ("Keyboard", "d") },
            { KeyBinding.E, ("Keyboard", "e") },
            { KeyBinding.F, ("Keyboard", "f") },
            { KeyBinding.G, ("Keyboard", "g") },
            { KeyBinding.H, ("Keyboard", "h") },
            { KeyBinding.I, ("Keyboard", "i") },
            { KeyBinding.J, ("Keyboard", "j") },
            { KeyBinding.K, ("Keyboard", "k") },
            { KeyBinding.L, ("Keyboard", "l") },
            { KeyBinding.M, ("Keyboard", "m") },
            { KeyBinding.N, ("Keyboard", "n") },
            { KeyBinding.O, ("Keyboard", "o") },
            { KeyBinding.P, ("Keyboard", "p") },
            { KeyBinding.Q, ("Keyboard", "q") },
            { KeyBinding.R, ("Keyboard", "r") },
            { KeyBinding.S, ("Keyboard", "s") },
            { KeyBinding.T, ("Keyboard", "t") },
            { KeyBinding.U, ("Keyboard", "u") },
            { KeyBinding.V, ("Keyboard", "v") },
            { KeyBinding.W, ("Keyboard", "w") },
            { KeyBinding.X, ("Keyboard", "x") },
            { KeyBinding.Y, ("Keyboard", "y") },
            { KeyBinding.Z, ("Keyboard", "z") },
            { KeyBinding.Digit0, ("Keyboard", "0") },
            { KeyBinding.Digit1, ("Keyboard", "1") },
            { KeyBinding.Digit2, ("Keyboard", "2") },
            { KeyBinding.Digit3, ("Keyboard", "3") },
            { KeyBinding.Digit4, ("Keyboard", "4") },
            { KeyBinding.Digit5, ("Keyboard", "5") },
            { KeyBinding.Digit6, ("Keyboard", "6") },
            { KeyBinding.Digit7, ("Keyboard", "7") },
            { KeyBinding.Digit8, ("Keyboard", "8") },
            { KeyBinding.Digit9, ("Keyboard", "9") },
            { KeyBinding.Num0, ("Keyboard", "numpad0") },
            { KeyBinding.Num1, ("Keyboard", "numpad1") },
            { KeyBinding.Num2, ("Keyboard", "numpad2") },
            { KeyBinding.Num3, ("Keyboard", "numpad3") },
            { KeyBinding.Num4, ("Keyboard", "numpad4") },
            { KeyBinding.Num5, ("Keyboard", "numpad5") },
            { KeyBinding.Num6, ("Keyboard", "numpad6") },
            { KeyBinding.Num7, ("Keyboard", "numpad7") },
            { KeyBinding.Num8, ("Keyboard", "numpad8") },
            { KeyBinding.Num9, ("Keyboard", "numpad9") },
            { KeyBinding.F1, ("Keyboard", "f1") },
            { KeyBinding.F2, ("Keyboard", "f2") },
            { KeyBinding.F3, ("Keyboard", "f3") },
            { KeyBinding.F4, ("Keyboard", "f4") },
            { KeyBinding.F5, ("Keyboard", "f5") },
            { KeyBinding.F6, ("Keyboard", "f6") },
            { KeyBinding.F7, ("Keyboard", "f7") },
            { KeyBinding.F8, ("Keyboard", "f8") },
            { KeyBinding.F9, ("Keyboard", "f9") },
            { KeyBinding.F10, ("Keyboard", "f10") },
            { KeyBinding.F11, ("Keyboard", "f11") },
            { KeyBinding.F12, ("Keyboard", "f12") },
            { KeyBinding.MouseLeft, ("Mouse", "leftButton") },
            { KeyBinding.MouseRight, ("Mouse", "rightButton") },
            { KeyBinding.MouseMiddle, ("Mouse", "middleButton") },
            { KeyBinding.MouseForward, ("Mouse", "forwardButton") },
            { KeyBinding.MouseBack, ("Mouse", "backButton") },
            { KeyBinding.GamepadSouth, ("Gamepad", "buttonSouth") },
            { KeyBinding.GamepadEast, ("Gamepad", "buttonEast") },
            { KeyBinding.GamepadWest, ("Gamepad", "buttonWest") },
            { KeyBinding.GamepadNorth, ("Gamepad", "buttonNorth") },
            { KeyBinding.GamepadLeft, ("Gamepad", "dpad/left") },
            { KeyBinding.GamepadRight, ("Gamepad", "dpad/right") },
            { KeyBinding.GamepadUp, ("Gamepad", "dpad/up") },
            { KeyBinding.GamepadDown, ("Gamepad", "dpad/down") },
            { KeyBinding.GamepadBumperLeft, ("Gamepad", "leftShoulder") },
            { KeyBinding.GamepadBumperRight, ("Gamepad", "rightShoulder") },
            { KeyBinding.GamepadTriggerLeft, ("Gamepad", "leftTrigger") },
            { KeyBinding.GamepadTriggerRight, ("Gamepad", "rightTrigger") },
            { KeyBinding.GamepadStickButtonLeft, ("Gamepad", "leftStickPress") },
            { KeyBinding.GamepadStickButtonRight, ("Gamepad", "rightStickPress") },
            { KeyBinding.GamepadSelect, ("Gamepad", "select") },
            { KeyBinding.GamepadStart, ("Gamepad", "start") },
        };
        
        private static readonly Dictionary<string, KeyBinding> PathToKeyBindingMap = new() {
            { "<Keyboard>/leftShift", KeyBinding.LeftShift },
            { "<Keyboard>/rightShift", KeyBinding.RightShift },
            { "<Keyboard>/leftAlt", KeyBinding.LeftAlt },
            { "<Keyboard>/rightAlt", KeyBinding.RightAlt },
            { "<Keyboard>/leftCtrl", KeyBinding.LeftControl },
            { "<Keyboard>/rightCtrl", KeyBinding.RightControl },
            { "<Keyboard>/leftCommand", KeyBinding.LeftCommand },
            { "<Keyboard>/rightCommand", KeyBinding.RightCommand },
            { "<Keyboard>/space", KeyBinding.Space },
            { "<Keyboard>/enter", KeyBinding.Enter },
            { "<Keyboard>/tab", KeyBinding.Tab },
            { "<Keyboard>/backquote", KeyBinding.Backquote },
            { "<Keyboard>/quote", KeyBinding.Quote },
            { "<Keyboard>/semicolon", KeyBinding.Semicolon },
            { "<Keyboard>/comma", KeyBinding.Comma },
            { "<Keyboard>/period", KeyBinding.Period },
            { "<Keyboard>/slash", KeyBinding.Slash },
            { "<Keyboard>/backslash", KeyBinding.Backslash },
            { "<Keyboard>/leftBracket", KeyBinding.LeftBracket },
            { "<Keyboard>/rightBracket", KeyBinding.RightBracket },
            { "<Keyboard>/minus", KeyBinding.Minus },
            { "<Keyboard>/equals", KeyBinding.Equals },
            { "<Keyboard>/escape", KeyBinding.Escape },
            { "<Keyboard>/backspace", KeyBinding.Backspace },
            { "<Keyboard>/capsLock", KeyBinding.CapsLock },
            { "<Keyboard>/numLock", KeyBinding.NumLock },
            { "<Keyboard>/scrollLock", KeyBinding.ScrollLock },
            { "<Keyboard>/pageUp", KeyBinding.PageUp },
            { "<Keyboard>/pageDown", KeyBinding.PageDown },
            { "<Keyboard>/home", KeyBinding.Home },
            { "<Keyboard>/end", KeyBinding.End },
            { "<Keyboard>/insert", KeyBinding.Insert },
            { "<Keyboard>/delete", KeyBinding.Delete },
            { "<Keyboard>/printScreen", KeyBinding.PrintScreen },
            { "<Keyboard>/pause", KeyBinding.Pause },
            { "<Keyboard>/numpadEnter", KeyBinding.NumEnter },
            { "<Keyboard>/numpadPlus", KeyBinding.NumPlus },
            { "<Keyboard>/numpadEquals", KeyBinding.NumEquals },
            { "<Keyboard>/numpadMinus", KeyBinding.NumMinus },
            { "<Keyboard>/numpadDivide", KeyBinding.NumDivide },
            { "<Keyboard>/numpadMultiply", KeyBinding.NumMultiply },
            { "<Keyboard>/numpadPeriod", KeyBinding.NumPeriod },
            { "<Keyboard>/leftArrow", KeyBinding.ArrowLeft },
            { "<Keyboard>/rightArrow", KeyBinding.ArrowRight },
            { "<Keyboard>/upArrow", KeyBinding.ArrowUp },
            { "<Keyboard>/downArrow", KeyBinding.ArrowDown },
            { "<Keyboard>/a", KeyBinding.A },
            { "<Keyboard>/b", KeyBinding.B },
            { "<Keyboard>/c", KeyBinding.C },
            { "<Keyboard>/d", KeyBinding.D },
            { "<Keyboard>/e", KeyBinding.E },
            { "<Keyboard>/f", KeyBinding.F },
            { "<Keyboard>/g", KeyBinding.G },
            { "<Keyboard>/h", KeyBinding.H },
            { "<Keyboard>/i", KeyBinding.I },
            { "<Keyboard>/j", KeyBinding.J },
            { "<Keyboard>/k", KeyBinding.K },
            { "<Keyboard>/l", KeyBinding.L },
            { "<Keyboard>/m", KeyBinding.M },
            { "<Keyboard>/n", KeyBinding.N },
            { "<Keyboard>/o", KeyBinding.O },
            { "<Keyboard>/p", KeyBinding.P },
            { "<Keyboard>/q", KeyBinding.Q },
            { "<Keyboard>/r", KeyBinding.R },
            { "<Keyboard>/s", KeyBinding.S },
            { "<Keyboard>/t", KeyBinding.T },
            { "<Keyboard>/u", KeyBinding.U },
            { "<Keyboard>/v", KeyBinding.V },
            { "<Keyboard>/w", KeyBinding.W },
            { "<Keyboard>/x", KeyBinding.X },
            { "<Keyboard>/y", KeyBinding.Y },
            { "<Keyboard>/z", KeyBinding.Z },
            { "<Keyboard>/0", KeyBinding.Digit0 },
            { "<Keyboard>/1", KeyBinding.Digit1 },
            { "<Keyboard>/2", KeyBinding.Digit2 },
            { "<Keyboard>/3", KeyBinding.Digit3 },
            { "<Keyboard>/4", KeyBinding.Digit4 },
            { "<Keyboard>/5", KeyBinding.Digit5 },
            { "<Keyboard>/6", KeyBinding.Digit6 },
            { "<Keyboard>/7", KeyBinding.Digit7 },
            { "<Keyboard>/8", KeyBinding.Digit8 },
            { "<Keyboard>/9", KeyBinding.Digit9 },
            { "<Keyboard>/numpad0", KeyBinding.Num0 },
            { "<Keyboard>/numpad1", KeyBinding.Num1 },
            { "<Keyboard>/numpad2", KeyBinding.Num2 },
            { "<Keyboard>/numpad3", KeyBinding.Num3 },
            { "<Keyboard>/numpad4", KeyBinding.Num4 },
            { "<Keyboard>/numpad5", KeyBinding.Num5 },
            { "<Keyboard>/numpad6", KeyBinding.Num6 },
            { "<Keyboard>/numpad7", KeyBinding.Num7 },
            { "<Keyboard>/numpad8", KeyBinding.Num8 },
            { "<Keyboard>/numpad9", KeyBinding.Num9 },
            { "<Keyboard>/f1", KeyBinding.F1 },
            { "<Keyboard>/f2", KeyBinding.F2 },
            { "<Keyboard>/f3", KeyBinding.F3 },
            { "<Keyboard>/f4", KeyBinding.F4 },
            { "<Keyboard>/f5", KeyBinding.F5 },
            { "<Keyboard>/f6", KeyBinding.F6 },
            { "<Keyboard>/f7", KeyBinding.F7 },
            { "<Keyboard>/f8", KeyBinding.F8 },
            { "<Keyboard>/f9", KeyBinding.F9 },
            { "<Keyboard>/f10", KeyBinding.F10 },
            { "<Keyboard>/f11", KeyBinding.F11 },
            { "<Keyboard>/f12", KeyBinding.F12 },
            { "<Mouse>/leftButton", KeyBinding.MouseLeft },
            { "<Mouse>/rightButton", KeyBinding.MouseRight },
            { "<Mouse>/middleButton", KeyBinding.MouseMiddle },
            { "<Mouse>/forwardButton", KeyBinding.MouseForward },
            { "<Mouse>/backButton", KeyBinding.MouseBack },
            { "<Gamepad>/buttonSouth", KeyBinding.GamepadSouth },
            { "<Gamepad>/buttonEast", KeyBinding.GamepadEast },
            { "<Gamepad>/buttonWest", KeyBinding.GamepadWest },
            { "<Gamepad>/buttonNorth", KeyBinding.GamepadNorth },
            { "<Gamepad>/dpad/left", KeyBinding.GamepadLeft },
            { "<Gamepad>/dpad/right", KeyBinding.GamepadRight },
            { "<Gamepad>/dpad/up", KeyBinding.GamepadUp },
            { "<Gamepad>/dpad/down", KeyBinding.GamepadDown },
            { "<Gamepad>/leftShoulder", KeyBinding.GamepadBumperLeft },
            { "<Gamepad>/rightShoulder", KeyBinding.GamepadBumperRight },
            { "<Gamepad>/leftTrigger", KeyBinding.GamepadTriggerLeft },
            { "<Gamepad>/rightTrigger", KeyBinding.GamepadTriggerRight },
            { "<Gamepad>/leftStickPress", KeyBinding.GamepadStickButtonLeft },
            { "<Gamepad>/rightStickPress", KeyBinding.GamepadStickButtonRight },
            { "<Gamepad>/select", KeyBinding.GamepadSelect },
            { "<Gamepad>/start", KeyBinding.GamepadStart },
        };
    }

}