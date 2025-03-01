using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Inputs {
    
    public sealed class GamepadVibration : MonoBehaviour, IGamepadVibration {
        
        private readonly struct Data {
            
            public readonly int priority;
            public readonly float weight;
            public readonly Vector2 frequency;
            
            public Data(int priority, float weight, Vector2 frequency) {
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
                : new Data(priority, weight: 0f, Vector2.zero);

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

        public void SetFrequency(object source, Vector2 frequency, float weight = 1f) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;
            
            _dataMap[hash] = new Data(data.priority, weight, frequency);
            
            _resultFrequency = BuildResultFrequency(_topPriority);
            ApplyFrequency(_resultFrequency);
        }

        private void ApplyFrequency(Vector2 frequency) {
            if (DeviceService.Instance.DualSenseAdapter.HasController()) {
                DeviceService.Instance.DualSenseAdapter.SetRumble(frequency);
                return;
            } 
            return;
            if (DeviceService.Instance.TryGetGamepad(out var gamepad)) {
                gamepad.SetMotorSpeeds(frequency.x, frequency.y);   
            }
        }

        private Vector2 BuildResultFrequency(int minPriority) {
            var frequency = Vector2.zero;
            float weightSum = 0f;
            
            foreach (var data in _dataMap.Values) {
                if (data.priority < minPriority) continue;
                
                weightSum += data.weight;
                frequency += data.frequency * data.weight;
            }
            
            return weightSum > 0f ? frequency / weightSum : default;
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