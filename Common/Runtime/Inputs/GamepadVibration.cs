using System.Collections.Generic;
using MisterGames.Common.Maths;
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

        public void Register(object source, int priority) {
            int hash = source.GetHashCode();
            _dataMap[hash] = _dataMap.TryGetValue(hash, out var data) 
                ? new Data(priority, data.weight, data.frequency)
                : new Data(priority);

            _topPriority = GetTopPriority();
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }

        public void Unregister(object source) {
            if (!_dataMap.Remove(source.GetHashCode())) return;
            
            _topPriority = GetTopPriority();
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }

        public void SetTwoMotors(object source, Vector2 frequency, float weight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;
            
            _dataMap[hash] = new Data(data.priority, weight * Vector2.one, frequency);
            
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }
        
        public void SetLeftMotor(object source, float frequency, float weight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;
            
            _dataMap[hash] = new Data(data.priority, data.weight.WithX(weight), data.frequency.WithX(frequency));
            
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }
        
        public void SetRightMotor(object source, float frequency, float weight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;
            
            _dataMap[hash] = new Data(data.priority, data.weight.WithY(weight), data.frequency.WithY(frequency));
            
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }

        private void ApplyFrequency(Vector2 frequency) {
            if (DeviceService.Instance.DualSenseAdapter.HasController()) {
                DeviceService.Instance.DualSenseAdapter.SetRumble(frequency);
                return;
            } 
            
            if (DeviceService.Instance.TryGetGamepad(out var gamepad)) {
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