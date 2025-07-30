using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Stats;
using MisterGames.Common.Volumes;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioVolume : MonoBehaviour, IAudioVolume {
        
        [Header("Volume")]
        [SerializeField] private int _priority;
        [SerializeField] [Range(0f, 1f)] private float _weight;
        [SerializeField] private Mode _mode;
        [VisibleIf(nameof(_mode), 1)]
        [SerializeField] private PositionWeightProvider _positionWeightProvider;

        [Header("Listener")]
        [SerializeField] private Optional<ValueModifier> _occlusionWeightListener = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);

        [Header("Sound")]
        [SerializeField] [Range(0f, 1f)] private float _listenerPresence;
        [SerializeField] private Optional<ValueModifier> _pitch = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);
        [SerializeField] private Optional<ValueModifier> _attenuationDistance = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);
        [SerializeField] private Optional<ValueModifier> _occlusionWeightSound = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);
        [SerializeField] private Optional<ValueModifier> _lowPassCutoffFrequency = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);
        [SerializeField] private Optional<ValueModifier> _highPassCutoffFrequency = Optional<ValueModifier>.WithDisabled(ValueModifier.Empty);
        
        private enum Mode {
            Global,
            Local,
        }
        
        public int Priority => _priority;
        public float ListenerPresence => _listenerPresence;

        private void OnEnable() {
            AudioPool.Main?.RegisterVolume(this);
        }

        private void OnDisable() {
            AudioPool.Main?.UnregisterVolume(this);
        }

        public float GetWeight(Vector3 position, out int volumeId) {
            volumeId = 0;
            
            return _mode switch {
                Mode.Global => _weight,
                Mode.Local => _weight * _positionWeightProvider.GetWeight(position, out volumeId),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public bool ModifyOcclusionWeightForListener(ref float occlusionWeight) {
            occlusionWeight = _occlusionWeightListener.Value.Modify(occlusionWeight);
            return _occlusionWeightListener.HasValue;
        }

        public bool ModifyPitch(ref float pitch) {
            pitch = _pitch.Value.Modify(pitch);
            return _pitch.HasValue;
        }

        public bool ModifyAttenuationDistance(ref float attenuationDistance) {
            attenuationDistance = _attenuationDistance.Value.Modify(attenuationDistance);
            return _attenuationDistance.HasValue;
        }

        public bool ModifyOcclusionWeightForSound(ref float occlusionWeight) {
            occlusionWeight = _occlusionWeightSound.Value.Modify(occlusionWeight);
            return _occlusionWeightSound.HasValue;
        }

        public bool ModifyLowPassFilter(ref float lpCutoffFreq) {
            lpCutoffFreq = _lowPassCutoffFrequency.Value.Modify(lpCutoffFreq);
            return _lowPassCutoffFrequency.HasValue;
        }

        public bool ModifyHighPassFilter(ref float hpCutoffFreq) {
            hpCutoffFreq = _highPassCutoffFrequency.Value.Modify(hpCutoffFreq);
            return _highPassCutoffFrequency.HasValue;
        }
    }
    
}