using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Inputs.DualSense {
    
    public sealed class DualSenseAdapter : MonoBehaviour, IDualSenseAdapter {

        [SerializeField] private bool _replicateOutputStateForAllControllers = true;
        
        private ControllerOutputState[] _outputStates;
        private uint _controllerCount;

        private void Awake() {
            StartControllersCountChecks(destroyCancellationToken).Forget();
        }
        
        private void OnEnable() {
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged += OnDeviceChanged;
        }

        private void OnDisable() {
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(DeviceType device) {
            switch (device) {
                case DeviceType.KeyboardMouse:
                    ResetAllGamepadsOutputState();
                    break;
                
                case DeviceType.Gamepad:
                    ActualizeAllGamepadsOutputState();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(device), device, null);
            }
        }
        
        public void ActualizeAllGamepadsOutputState() {
            for (uint i = 0; i < _controllerCount; i++) {
                ref var state = ref _outputStates[i];
                DualSenseNative.SetControllerOutputState(i, state);
            }
        }

        public void ResetAllGamepadsOutputState() {
            for (uint i = 0; i < _controllerCount; i++) {
                DualSenseNative.SetControllerOutputState(i, default);
            }
        }
        
        public ControllerInputState GetInputState(int index = 0) {
            if (index < 0 || index >= _controllerCount) return default;

            return DualSenseNative.GetControllerInputState((uint) index);
        }

        public bool HasController(int index = 0) {
            return index >= 0 && index < _controllerCount && 
                   Services.Get<IDeviceService>().CurrentDevice == DeviceType.Gamepad;
        }
        
        public void SetRumble(Vector2 rumble, int index = 0) {
            if (!HasController(index)) return;
            
            if (_replicateOutputStateForAllControllers) {
                for (uint i = 0; i < _controllerCount; i++) {
                    ref var s = ref _outputStates[i];
                    
                    s.LeftRumbleIntensity = rumble.x;
                    s.RightRumbleIntensity = rumble.y;

                    DualSenseNative.SetControllerOutputState(i, s);
                }
                
                return;
            }
            
            if (index < 0 || index >= _controllerCount) return;
            
            ref var state = ref _outputStates[index];
            
            state.LeftRumbleIntensity = rumble.x;
            state.RightRumbleIntensity = rumble.y;
            
            DualSenseNative.SetControllerOutputState((uint) index, state);
        }

        public void SetTriggerEffect(GamepadSide side, TriggerEffect effect, int index = 0) {
            if (!HasController(index)) return;
            
            if (_replicateOutputStateForAllControllers) {
                for (uint i = 0; i < _controllerCount; i++) {
                    ref var s = ref _outputStates[i];

                    switch (side) {
                        case GamepadSide.Left:
                            s.LeftTriggerEffect = effect;
                            break;
                        
                        case GamepadSide.Right:
                            s.RightTriggerEffect = effect;
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException(nameof(side), side, null);
                    }

                    DualSenseNative.SetControllerOutputState(i, s);
                }
                
                return;
            }
            
            if (index < 0 || index >= _controllerCount) return;
            
            ref var state = ref _outputStates[index];
            
            switch (side) {
                case GamepadSide.Left:
                    state.LeftTriggerEffect = effect;
                    break;
                        
                case GamepadSide.Right:
                    state.RightTriggerEffect = effect;
                    break;
                        
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            
            DualSenseNative.SetControllerOutputState((uint) index, state);
        }

        private async UniTask StartControllersCountChecks(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                UpdateOutputStates(DualSenseNative.GetControllerCount());
                
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
        }

        private void UpdateOutputStates(uint count) {
            if (_controllerCount == count) return;
            
            uint oldCount = _controllerCount;
            _controllerCount = count;
            
            var states = count <= 0 
                ? Array.Empty<ControllerOutputState>() 
                : new ControllerOutputState[count];

            for (int i = 0; i < oldCount && i < count; i++) {
                states[i] = _outputStates[i];
            }
            
            _outputStates = states;
        }
    }
    
}