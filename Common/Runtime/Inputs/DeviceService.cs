using System;
using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Lists;
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
            var lastDevice = CurrentDevice;
            CurrentDevice = GetCurrentDeviceType();

            if (lastDevice != CurrentDevice) OnDeviceChanged.Invoke(CurrentDevice);
        }

        private DeviceType GetCurrentDeviceType() {
            if (Gamepad.current != null &&
                Gamepad.current.allControls.Any(control => control.IsPressed() && control.synthetic == false))
            {
                return DeviceType.Gamepad;
            }
            
            if (Keyboard.current.anyKey.wasPressedThisFrame 
                || Mouse.current.leftButton.wasPressedThisFrame 
                || Mouse.current.rightButton.wasPressedThisFrame
                || Mouse.current.middleButton.wasPressedThisFrame
                || Mouse.current.scroll.ReadValue() != Vector2.zero
                || Mouse.current.delta.ReadValue() != Vector2.zero)
            {
                return DeviceType.KeyboardMouse;
            }
            
            return CurrentDevice;
        }
    }
    
}