using System.Collections.Generic;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace MisterGames.Input.Global {

    public static class GlobalInput {

        private static GlobalInputs _globalInputs;
        private static readonly Dictionary<KeyBinding, InputAction> keys = new Dictionary<KeyBinding, InputAction>();
        private static readonly Dictionary<AxisBinding, InputAction> axes = new Dictionary<AxisBinding, InputAction>();

        public static int DeviceId => deviceId;
        internal static int deviceId;
        
        public static bool IsActive(this KeyBinding key) {
            return key != KeyBinding.None && keys[key].phase == InputActionPhase.Performed;
        }
        
        public static bool IsActive(this KeyCode key) {
            return UnityEngine.Input.GetKey(key);
        }

        public static bool IsActive(this ShortcutModifiers key) {
            if (key == ShortcutModifiers.None) return true;

            return ((key & ShortcutModifiers.Alt) != ShortcutModifiers.Alt || 
                    KeyCode.LeftAlt.IsActive() || KeyCode.RightAlt.IsActive()) &&
                   
                   ((key & ShortcutModifiers.Action) != ShortcutModifiers.Action || 
                    KeyCode.LeftControl.IsActive() || KeyCode.LeftCommand.IsActive()) && 
                   
                   ((key & ShortcutModifiers.Shift) != ShortcutModifiers.Shift || 
                    KeyCode.LeftShift.IsActive() || KeyCode.RightShift.IsActive()) &&
                   
                   ((key & ShortcutModifiers.Control) != ShortcutModifiers.Control || 
                    KeyCode.LeftControl.IsActive() || KeyCode.RightControl.IsActive());
        }

        public static bool WasPressedThisFrame(this KeyBinding key) {
            return keys[key].WasPressedThisFrame();
        }

        public static bool WasPressedThisFrame(this KeyCode key) {
            return UnityEngine.Input.GetKeyDown(key);
        }

        public static bool WasPressedThisFrame(this ShortcutModifiers key) {
            if (key == ShortcutModifiers.None) return false;

            return ((key & ShortcutModifiers.Alt) == ShortcutModifiers.Alt && 
                    (KeyCode.LeftAlt.WasPressedThisFrame() || KeyCode.RightAlt.WasPressedThisFrame())) ||
                   
                   ((key & ShortcutModifiers.Action) == ShortcutModifiers.Action && 
                    (KeyCode.LeftControl.WasPressedThisFrame() || KeyCode.LeftCommand.WasPressedThisFrame())) ||
                   
                   ((key & ShortcutModifiers.Shift) == ShortcutModifiers.Shift && 
                    (KeyCode.LeftShift.IsActive() || KeyCode.RightShift.WasPressedThisFrame())) ||
                   
                   ((key & ShortcutModifiers.Control) == ShortcutModifiers.Control && 
                    (KeyCode.LeftControl.WasPressedThisFrame() || KeyCode.RightControl.WasPressedThisFrame()));
        }

        public static Vector2 GetValue(this AxisBinding axis) {
            return axes[axis].ReadValue<Vector2>();
        }
        
        internal static void Init(GlobalInputs globalInputs) {
            _globalInputs = globalInputs;
            FillAxisBindingMap();
            FillKeyBindingMap();
        }

        internal static void Enable() {
            _globalInputs?.Enable();
        }

        internal static void Disable() {
            _globalInputs?.Disable();
        }
        
        internal static void Terminate() {
#if UNITY_EDITOR
            if (Application.isPlaying) _globalInputs?.Dispose();
            else if (_globalInputs != null) {
                Object.DestroyImmediate(_globalInputs.asset);
            }
#else
            _globalInputs?.Dispose();
#endif

            _globalInputs = null;
            keys.Clear();
            axes.Clear();
        }

        private static void FillAxisBindingMap() {
            axes[AxisBinding.MouseDelta] = _globalInputs.Mouse.delta;
            axes[AxisBinding.MouseScroll] = _globalInputs.Mouse.scroll;
            axes[AxisBinding.JoystickStickLeft] = _globalInputs.Joystick.stickLeft;
            axes[AxisBinding.JoystickStickRight] = _globalInputs.Joystick.stickRight;
        }

        private static void FillKeyBindingMap() {
            keys[KeyBinding.A] = _globalInputs.Letters.a;
            keys[KeyBinding.B] = _globalInputs.Letters.b;
            keys[KeyBinding.C] = _globalInputs.Letters.c;
            keys[KeyBinding.D] = _globalInputs.Letters.d;
            keys[KeyBinding.E] = _globalInputs.Letters.e;
            keys[KeyBinding.F] = _globalInputs.Letters.f;
            keys[KeyBinding.G] = _globalInputs.Letters.g;
            keys[KeyBinding.H] = _globalInputs.Letters.h;
            keys[KeyBinding.I] = _globalInputs.Letters.i;
            keys[KeyBinding.J] = _globalInputs.Letters.j;
            keys[KeyBinding.K] = _globalInputs.Letters.k;
            keys[KeyBinding.L] = _globalInputs.Letters.l;
            keys[KeyBinding.M] = _globalInputs.Letters.m;
            keys[KeyBinding.N] = _globalInputs.Letters.n;
            keys[KeyBinding.O] = _globalInputs.Letters.o;
            keys[KeyBinding.P] = _globalInputs.Letters.p;
            keys[KeyBinding.Q] = _globalInputs.Letters.q;
            keys[KeyBinding.R] = _globalInputs.Letters.r;
            keys[KeyBinding.S] = _globalInputs.Letters.s;
            keys[KeyBinding.T] = _globalInputs.Letters.t;
            keys[KeyBinding.U] = _globalInputs.Letters.u;
            keys[KeyBinding.V] = _globalInputs.Letters.v;
            keys[KeyBinding.W] = _globalInputs.Letters.w;
            keys[KeyBinding.X] = _globalInputs.Letters.x;
            keys[KeyBinding.Y] = _globalInputs.Letters.y;
            keys[KeyBinding.Z] = _globalInputs.Letters.z;
            
            keys[KeyBinding.A0] = _globalInputs.Digits.Digit0;
            keys[KeyBinding.A1] = _globalInputs.Digits.Digit1;
            keys[KeyBinding.A2] = _globalInputs.Digits.Digit2;
            keys[KeyBinding.A3] = _globalInputs.Digits.Digit3;
            keys[KeyBinding.A4] = _globalInputs.Digits.Digit4;
            keys[KeyBinding.A5] = _globalInputs.Digits.Digit5;
            keys[KeyBinding.A6] = _globalInputs.Digits.Digit6;
            keys[KeyBinding.A7] = _globalInputs.Digits.Digit7;
            keys[KeyBinding.A8] = _globalInputs.Digits.Digit8;
            keys[KeyBinding.A9] = _globalInputs.Digits.Digit9;
            
            keys[KeyBinding.MouseLeft] = _globalInputs.Mouse.left;
            keys[KeyBinding.MouseRight] = _globalInputs.Mouse.right;
            keys[KeyBinding.MouseMiddle] = _globalInputs.Mouse.middle;
            
            keys[KeyBinding.ArrowDown] = _globalInputs.Arrows.down;
            keys[KeyBinding.ArrowUp] = _globalInputs.Arrows.up;
            keys[KeyBinding.ArrowLeft] = _globalInputs.Arrows.left;
            keys[KeyBinding.ArrowRight] = _globalInputs.Arrows.right;
            
            keys[KeyBinding.F1] = _globalInputs.Function.f1;
            keys[KeyBinding.F2] = _globalInputs.Function.f2;
            keys[KeyBinding.F3] = _globalInputs.Function.f3;
            keys[KeyBinding.F4] = _globalInputs.Function.f4;
            keys[KeyBinding.F5] = _globalInputs.Function.f5;
            keys[KeyBinding.F6] = _globalInputs.Function.f6;
            keys[KeyBinding.F7] = _globalInputs.Function.f7;
            keys[KeyBinding.F8] = _globalInputs.Function.f8;
            keys[KeyBinding.F9] = _globalInputs.Function.f9;
            keys[KeyBinding.F10] = _globalInputs.Function.f10;
            keys[KeyBinding.F11] = _globalInputs.Function.f11;
            keys[KeyBinding.F12] = _globalInputs.Function.f12;
            
            keys[KeyBinding.LeftControl] = _globalInputs.Controls.leftControl;
            keys[KeyBinding.LeftShift] = _globalInputs.Controls.leftShift;
            keys[KeyBinding.LeftAlt] = _globalInputs.Controls.leftAlt;
            
            keys[KeyBinding.RightControl] = _globalInputs.Controls.rightControl;
            keys[KeyBinding.RightShift] = _globalInputs.Controls.rightShift;
            keys[KeyBinding.RightAlt] = _globalInputs.Controls.rightAlt;
            
            keys[KeyBinding.Tab] = _globalInputs.Controls.tab;
            keys[KeyBinding.Space] = _globalInputs.Controls.space;
            keys[KeyBinding.Enter] = _globalInputs.Controls.enter;
            keys[KeyBinding.Escape] = _globalInputs.Controls.escape;
            keys[KeyBinding.Backspace] = _globalInputs.Controls.backspace;
            keys[KeyBinding.Caps] = _globalInputs.Controls.caps;
            keys[KeyBinding.Delete] = _globalInputs.Controls.delete;
            
            keys[KeyBinding.Tilde] = _globalInputs.Controls.tilde;
            keys[KeyBinding.Plus] = _globalInputs.Controls.plus;
            keys[KeyBinding.Minus] = _globalInputs.Controls.minus;
            keys[KeyBinding.Equals] = _globalInputs.Controls.equals;
            keys[KeyBinding.Div] = _globalInputs.Controls.div;
            keys[KeyBinding.Mul] = _globalInputs.Controls.mul;
            
            keys[KeyBinding.JoystickA] = _globalInputs.Joystick.a;
            keys[KeyBinding.JoystickB] = _globalInputs.Joystick.b;
            keys[KeyBinding.JoystickX] = _globalInputs.Joystick.x;
            keys[KeyBinding.JoystickY] = _globalInputs.Joystick.y;
            
            keys[KeyBinding.JoystickUp] = _globalInputs.Joystick.arrowUp;
            keys[KeyBinding.JoystickDown] = _globalInputs.Joystick.arrowDown;
            keys[KeyBinding.JoystickLeft] = _globalInputs.Joystick.arrowLeft;
            keys[KeyBinding.JoystickRight] = _globalInputs.Joystick.arrowRight;
            
            keys[KeyBinding.JoystickBumperLeft] = _globalInputs.Joystick.bumperLeft;
            keys[KeyBinding.JoystickBumperRight] = _globalInputs.Joystick.bumperRight;
            
            keys[KeyBinding.JoystickTriggerLeft] = _globalInputs.Joystick.triggerLeft;
            keys[KeyBinding.JoystickTriggerRight] = _globalInputs.Joystick.triggerRight;
            
            keys[KeyBinding.JoystickSelect] = _globalInputs.Joystick.select;
            keys[KeyBinding.JoystickStart] = _globalInputs.Joystick.start;
        }
    }

}
