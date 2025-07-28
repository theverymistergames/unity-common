using System;
using MisterGames.Common.Attributes;
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

        [Header("Process Listener")]
        [SerializeField] private ValueModifier _occlusionWeightListener = ValueModifier.Empty;

        [Header("Process Sound")]
        [SerializeField] private ValueModifier _pitch = ValueModifier.Empty;
        [SerializeField] private ValueModifier _occlusionWeightSound = ValueModifier.Empty;
        [SerializeField] private ValueModifier _lowPassCutoffFrequency = ValueModifier.Empty;
        [SerializeField] private ValueModifier _highPassCutoffFrequency = ValueModifier.Empty;
        
        private enum Mode {
            Global,
            Local,
        }
        
        public int Priority => _priority;

        private void OnEnable() {
            AudioPool.Main?.RegisterVolume(this);
        }

        private void OnDisable() {
            AudioPool.Main?.UnregisterVolume(this);
        }

        public float GetWeight(Vector3 position) {
            return _mode switch {
                Mode.Global => _weight,
                Mode.Local => _weight * _positionWeightProvider.GetWeight(position),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void ModifyPitch(ref float pitch) {
            pitch = _pitch.Modify(pitch);
        }

        public void ModifyOcclusionWeightForSound(ref float occlusionWeight) {
            occlusionWeight = _occlusionWeightSound.Modify(occlusionWeight);
        }

        public void ModifyOcclusionWeightForListener(ref float occlusionWeight) {
            occlusionWeight = _occlusionWeightListener.Modify(occlusionWeight);
        }

        public void ModifyLowHighPassFilters(ref float lpCutoffFreq, ref float hpCutoffFreq) {
            lpCutoffFreq = _lowPassCutoffFrequency.Modify(lpCutoffFreq);
            hpCutoffFreq = _highPassCutoffFrequency.Modify(hpCutoffFreq);
        }
    }
    
}