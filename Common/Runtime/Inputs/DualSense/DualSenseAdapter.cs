using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MisterGames.Common.Inputs.DualSense {
    
    public sealed class DualSenseAdapter : MonoBehaviour, IDualSenseAdapter {

        [SerializeField] private bool _replicateOutputStateForAllControllers = true;
        
        public readonly struct Controller {
            
            private readonly uint _index;
            
            public Controller(uint index) {
                _index = index;
            }

            public ControllerInputState GetInputState() {
                var inputState = DualSenseNative.GetControllerInputState(_index);
                
                inputState.LeftTrigger.TriggerValue = Math.Round(inputState.LeftTrigger.TriggerValue, 2);
                inputState.RightTrigger.TriggerValue = Math.Round(inputState.RightTrigger.TriggerValue, 2);

                return inputState;
            }

            public bool SetOutputState(ControllerOutputState outputState) {
                return DualSenseNative.SetControllerOutputState(_index, outputState);
            }
        }
        
        private ControllerOutputState[] _outputStates;
        private uint _controllerCount;

        private void Awake() {
            StartControllersCountChecks(destroyCancellationToken).Forget();
        }

        public ControllerInputState GetInputState(int index = 0) {
            if (index < 0 || index >= _controllerCount) return default;

            return DualSenseNative.GetControllerInputState((uint) index);
        }

        public bool HasController(int index = 0) {
            return index >= 0 && index < _controllerCount;
        }
        
        public void SetRumble(Vector2 rumble, int index = 0) {
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