using System;
using System.Runtime.CompilerServices;
using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Common.Inputs {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class DeviceService : MonoBehaviour, IDeviceService, IUpdate {
        
        [SerializeField] private GamepadVibration _gamepadVibration;
        [SerializeField] private DualSenseAdapter _dualSenseAdapter;
        
        public static IDeviceService Instance { get; private set; }
        
        public event Action<DeviceType> OnDeviceChanged = delegate { };
        
        public DeviceType CurrentDevice { get; private set; }
        public int LastPointerDeviceId { get; private set; }
        
        public IGamepadVibration GamepadVibration => _gamepadVibration;
        public IDualSenseAdapter DualSenseAdapter => _dualSenseAdapter;

        private void Awake() {
            Instance = this; 
        }

        private void OnDestroy() {
            Instance = null;
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
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
            var lastDevice = CurrentDevice;
            CurrentDevice = GetCurrentDeviceType();

            if (lastDevice != CurrentDevice) OnDeviceChanged.Invoke(CurrentDevice);
        }

        private DeviceType GetCurrentDeviceType() {
            if (IsAnyKeyboardMouseControlPressed()) {
                return DeviceType.KeyboardMouse;
            }
            
            if (Gamepad.current != null && IsAnyGamepadControlPressed()) {
                return DeviceType.Gamepad;
            }
            
            return CurrentDevice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAnyKeyboardMouseControlPressed() {
            return Keyboard.current.anyKey.wasPressedThisFrame 
                   || Mouse.current.leftButton.wasPressedThisFrame
                   || Mouse.current.rightButton.wasPressedThisFrame
                   || Mouse.current.middleButton.wasPressedThisFrame
                   || Mouse.current.scroll.ReadValue() != default
                   || Mouse.current.delta.ReadValue() != default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAnyGamepadControlPressed() {
            var controls = Gamepad.current.allControls;
            
            for (int i = 0; i < controls.Count; i++) {
                if (controls[i].IsPressed() && !controls[i].synthetic) return true;
            }

            return false;
        }
    }
    
}