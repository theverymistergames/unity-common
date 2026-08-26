using System;
using System.Collections.Generic;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public sealed class GamepadVibration : MonoBehaviour, IGamepadVibration, IUpdate {
        
        private readonly struct Data {
            
            public readonly int priority;
            public readonly Vector2 weight;
            public readonly Vector2 frequency;
            
            public Data(int priority, Vector2 weight = default, Vector2 frequency = default) {
                this.priority = priority;
                this.weight = weight;
                this.frequency = frequency;
            }
        }

        private IDeviceService _deviceService;
        private readonly Dictionary<int, Data> _dataMap = new();
        private Vector2 _resultFrequency;
        private int _topPriority;
        private int _lastMul = 1;

        private void Awake() {
            _deviceService = Services.Get<IDeviceService>();
        }

        private void OnEnable() {
            _deviceService.OnDeviceChanged += OnDeviceChanged;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _deviceService.OnDeviceChanged -= OnDeviceChanged;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            int oldMul = _lastMul;
            _lastMul = Time.timeScale > 0f ? 1 : 0;
            
            if (oldMul == _lastMul) return;
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        private void OnDeviceChanged(InputDeviceType device) {
            switch (device) {
                case InputDeviceType.KeyboardMouse:
                    ApplyFrequency(Vector2.zero);
                    break;
                
                case InputDeviceType.Gamepad:
                    ApplyFrequencyIfGamepadActive(_resultFrequency);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(device), device, null);
            }
        }

        public void Register(object source, int priority) {
            int hash = source.GetHashCode();
            _dataMap[hash] = _dataMap.TryGetValue(hash, out var data) 
                ? new Data(priority, data.weight, data.frequency)
                : new Data(priority);

            _topPriority = GetTopPriority();
            _resultFrequency = BuildResultFrequency(_topPriority);
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        public void Unregister(object source) {
            if (!_dataMap.Remove(source.GetHashCode())) return;
            
            _topPriority = GetTopPriority();
            _resultFrequency = BuildResultFrequency(_topPriority);
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        public void SetTwoMotors(object source, Vector2 frequency, float weightLeft = 1f, float weightRight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;

            var weight = new Vector2(weightLeft, weightRight);
            
            // Sources push their frequency every frame: without this the whole data map is folded
            // and a gamepad output write is issued each time, even when nothing actually changed.
            if (data.frequency == frequency && data.weight == weight) return;
            
            _dataMap[hash] = new Data(data.priority, weight, frequency);
            _resultFrequency = BuildResultFrequency(_topPriority);
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        public void SetMotor(object source, GamepadSide side, float frequency, float weight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;

            var f = data.frequency;
            var w = data.weight;

            switch (side) {
                case GamepadSide.Left:
                    f = f.WithX(frequency);
                    w = w.WithX(weight);
                    break;
                
                case GamepadSide.Right:
                    f = f.WithY(frequency);
                    w = w.WithY(weight);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            
            if (data.frequency == f && data.weight == w) return;
            
            _dataMap[hash] = new Data(data.priority, w, f);
            _resultFrequency = BuildResultFrequency(_topPriority);
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        private void ApplyFrequencyIfGamepadActive(Vector2 frequency) {
            switch (_deviceService.CurrentDevice) {
                case InputDeviceType.KeyboardMouse:
                    return;

                case InputDeviceType.Gamepad:
                    ApplyFrequency(frequency * _lastMul);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyFrequency(Vector2 frequency) {
            if (_deviceService.DualSenseAdapter.HasController()) {
                _deviceService.DualSenseAdapter.SetRumble(frequency);
                return;
            } 
            
            if (_deviceService.TryGetGamepad(out var gamepad)) {
                gamepad.SetMotorSpeeds(frequency.x, frequency.y);   
            }
        }

        private Vector2 BuildResultFrequency(int minPriority) {
            var frequency = Vector2.zero;
            var weightSum = Vector2.zero;
            
            foreach (var data in _dataMap.Values) {
                if (data.priority < minPriority) continue;
                
                var w = data.weight.Abs();
                
                weightSum += w;
                frequency += data.frequency * w;
            }

            return new Vector2(
                weightSum.x > 0f ? frequency.x / weightSum.x : 0f,
                weightSum.y > 0f ? frequency.y / weightSum.y : 0f  
            );
        }

        private int GetTopPriority() {
            int priority = 0;
            
            foreach (var data in _dataMap.Values) {
                if (data.priority > priority) priority = data.priority;
            }
            
            return priority;
        }
    }
    
}