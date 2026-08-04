using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Tick;
using UnityEngine.InputSystem;

namespace MisterGames.Common.Inputs {

    public sealed class DeviceService : IDeviceService, IDisposable, IUpdate {
        
        public event Action<InputDeviceType> OnDeviceChanged = delegate { };
        
        public InputDeviceType CurrentDevice { get; private set; }
        public GamepadType GamepadType { get; private set; }
        public int LastPointerDeviceId { get; private set; }
        
        public bool AnyKeyPressedThisFrame { get; private set; }
        public bool AnyInputActivatedThisFrame { get; private set; }
        
        public IGamepadVibration GamepadVibration { get; private set; }
        public IDualSenseAdapter DualSenseAdapter { get; private set; }

        private static readonly HashSet<string> _gamepadStickNames = new() {
            "leftStick",
            "rightStick",
        };

        public void Initialize(IGamepadVibration gamepadVibration, IDualSenseAdapter dualSenseAdapter) {
            GamepadVibration = gamepadVibration;
            DualSenseAdapter = dualSenseAdapter;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            AnyKeyPressedThisFrame = false;
            AnyInputActivatedThisFrame = false;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }
        
        public bool TryGetGamepad(out Gamepad gamepad) {
            gamepad = Gamepad.current;
            return gamepad != null;
        }

        void IUpdate.OnUpdate(float dt) {
            FetchPointerDeviceId();
            CheckDeviceType();
        }

        private void FetchPointerDeviceId() {
            if (Mouse.current != null) LastPointerDeviceId = Mouse.current.deviceId;
        }
        
        private void CheckDeviceType() {
            bool keyboardMousePressed = IsAnyKeyboardMouseControlPressed();
            bool mouseOrScrollMoved = IsMouseOrScrollMoved();
            IsAnyGamepadControlPressed(out bool gamepadPressed, out bool gamepadSticksMoved);
            
            AnyKeyPressedThisFrame = keyboardMousePressed || gamepadPressed;
            AnyInputActivatedThisFrame = keyboardMousePressed || mouseOrScrollMoved || gamepadPressed || gamepadSticksMoved;
            
            var lastDevice = CurrentDevice;
            CurrentDevice = gamepadPressed || gamepadSticksMoved ? InputDeviceType.Gamepad 
                : keyboardMousePressed || mouseOrScrollMoved ? InputDeviceType.KeyboardMouse
                : CurrentDevice;

            GamepadType = CurrentDevice is InputDeviceType.Gamepad ? Gamepad.current.GetGamepadType() : GamepadType.Default;

            if (lastDevice == CurrentDevice) return;

            OnDeviceChanged.Invoke(CurrentDevice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAnyKeyboardMouseControlPressed() {
            return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame
                   || Mouse.current != null &&
                   (Mouse.current.leftButton.wasPressedThisFrame
                   || Mouse.current.rightButton.wasPressedThisFrame
                   || Mouse.current.middleButton.wasPressedThisFrame);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMouseOrScrollMoved() {
            return Mouse.current != null &&
                   (Mouse.current.scroll.ReadValue() != default
                    || Mouse.current.delta.ReadValue() != default);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IsAnyGamepadControlPressed(out bool keysPressed, out bool sticksMoved) {
            keysPressed = false;
            sticksMoved = false;
            
            if (Gamepad.current == null) return;
            
            var controls = Gamepad.current.allControls;
            
            for (int i = 0; i < controls.Count; i++) {
                var c = controls[i];
                if (c.synthetic || !c.IsPressed()) continue;

                if (_gamepadStickNames.Contains(c.name)) {
                    sticksMoved = true;
                }
                else {
                    keysPressed = true;
                }
                
                if (sticksMoved && keysPressed) return;
            }
        }
    }
    
}