using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Stats;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioMixerService : IAudioMixerService {

        private readonly MultiValueDictionary<string, int> _parameterToSourceModifiersMap = new();
        private readonly Dictionary<(string par, int hash), (ValueModifier mod, float time)> _modifiersMap = new();
        private readonly Dictionary<string, float> _defaultValuesMap = new();
        private AudioMixer _mixer;

        public void Initialize(AudioMixer mixer) {
            _mixer = mixer;
        }

        public void SetModifier(object source, string parameter, ValueModifier modifier) {
            if (!_defaultValuesMap.ContainsKey(parameter) && _mixer.GetFloat(parameter, out float def)) {
                _defaultValuesMap.Add(parameter, def);
            }

            if (!_defaultValuesMap.ContainsKey(parameter)) {
                Debug.LogError($"{nameof(AudioMixerService).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, " +
                               $"source [{source}] is trying to set non-existent parameter {parameter}");
                return;
            }
            
            int hash = source.GetHashCode();
            
            if (!_parameterToSourceModifiersMap.ContainsValue(parameter, hash)) {
                _parameterToSourceModifiersMap.AddValue(parameter, hash);
            }
            
            _modifiersMap[(parameter, hash)] = (modifier, Time.realtimeSinceStartup);
            
            UpdateValue(parameter);
        }

        public void RemoveModifier(object source, string parameter) {
            int hash = source.GetHashCode();
            
            _parameterToSourceModifiersMap.RemoveValue(parameter, hash);
            if (!_modifiersMap.Remove((parameter, hash))) return;
            
            UpdateValue(parameter);
        }

        public float GetFloat(string parameter) {
            return _mixer.GetFloat(parameter, out float value) ? value : 0f;
        }

        private void UpdateValue(string parameter) {
            _mixer.SetFloat(parameter, GetValue(parameter, _defaultValuesMap[parameter]));
        }

        private float GetValue(string parameter, float defaultValue) {
            int count = _parameterToSourceModifiersMap.GetCount(parameter);

            float accumMul = defaultValue;
            float accumAdd = 0f;
            float lowerBound = float.MinValue;
            float upperBound = float.MaxValue;
            float set = 0f;
            float lastSetTime = -1f;
            
            for (int i = 0; i < count; i++) {
                int hash = _parameterToSourceModifiersMap.GetValueAt(parameter, i);
                if (!_modifiersMap.TryGetValue((parameter, hash), out var data)) continue;

                var modifier = data.mod;
                
                switch (modifier.operation) {
                    case OperationType.Mul:
                        accumMul *= modifier.modifier;
                        break;
                    case OperationType.Add:
                        accumAdd += modifier.modifier;
                        break;
                    case OperationType.Min:
                        lowerBound = Mathf.Max(lowerBound, modifier.modifier);
                        break;
                    case OperationType.Max:
                        upperBound = Mathf.Min(upperBound, modifier.modifier);
                        break;
                    case OperationType.Set:
                        if (lastSetTime < 0f || data.time > lastSetTime) {
                            set = modifier.modifier;
                            lastSetTime = data.time;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            return lastSetTime >= 0f 
                ? set 
                : Mathf.Clamp(accumMul + accumAdd, lowerBound, upperBound);
        }
    }
    
}