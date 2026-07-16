using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Volumes;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class ReverbVolume : MonoBehaviour, IReverbVolume {

        [Header("Volume")]
        [SerializeField] private int _priority;
        [SerializeField] [Range(0f, 1f)] private float _weight;
        [SerializeField] private Mode _mode;
        [VisibleIf(nameof(_mode), 1)]
        [SerializeField] private PositionWeightProvider _positionWeightProvider;

        [Header("Reverb")]
        [SerializeField] [Range(-10000f, 0f)] private float _level;
        [SerializeField] private ReverbSettings _reverbSettings;
        
        private enum Mode {
            Global,
            Local,
        }

        public int Id => GetEntityId();
        public int Priority => _priority;
        public float Level => _level;

        public IReverbSettings GetReverbSettings() {
            return _reverbSettings;
        }
        
        private void OnEnable() {
            AudioPool.Main?.RegisterReverbVolume(this);
        }

        private void OnDisable() {
            AudioPool.Main?.UnregisterReverbVolume(this);
        }
        
        public WeightData GetWeight(Vector3 position) {
            switch (_mode) {
                case Mode.Global:
                    return new WeightData(_weight, GetHashCode(), position);
                
                case Mode.Local:
                    var data = _positionWeightProvider.GetWeight(position);
                    return new WeightData(_weight * data.weight, data.volumeId, data.closestPoint);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}