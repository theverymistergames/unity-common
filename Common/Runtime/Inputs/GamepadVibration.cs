using System;
using System.Collections.Generic;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public sealed class GamepadVibration : MonoBehaviour, IGamepadVibration {
        
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
        
        private readonly Dictionary<int, Data> _dataMap = new();
        private Vector2 _resultFrequency;
        private int _topPriority;

        private void OnEnable() {
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged += OnDeviceChanged;
        }

        private void OnDisable() {
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(DeviceType device) {
            switch (device) {
                case DeviceType.KeyboardMouse:
                    ApplyFrequency(Vector2.zero);
                    break;
                
                case DeviceType.Gamepad:
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
            
            _dataMap[hash] = new Data(data.priority, new Vector2(weightLeft, weightRight), frequency);
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
            
            _dataMap[hash] = new Data(data.priority, w, f);
            _resultFrequency = BuildResultFrequency(_topPriority);
            
            ApplyFrequencyIfGamepadActive(_resultFrequency);
        }

        private void ApplyFrequencyIfGamepadActive(Vector2 frequency) {
            switch (Services.Get<IDeviceService>().CurrentDevice) {
                case DeviceType.KeyboardMouse:
                    return;

                case DeviceType.Gamepad:
                    ApplyFrequency(frequency);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyFrequency(Vector2 frequency) {
            if (Services.Get<IDeviceService>().DualSenseAdapter.HasController()) {
                Services.Get<IDeviceService>().DualSenseAdapter.SetRumble(frequency);
                return;
            } 
            
            if (Services.Get<IDeviceService>().TryGetGamepad(out var gamepad)) {
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