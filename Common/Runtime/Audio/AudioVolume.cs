using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Jobs;
using MisterGames.Common.Stats;
using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
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

        private const int MinBatch = 16;

        public EntityId Id => GetEntityId();
        public int Priority => _priority;
        public float ListenerPresence => _listenerPresence;
        public float Weight { get => _weight; set => _weight = value; }

        private void OnEnable() {
            AudioPool.Main?.RegisterAudioVolume(this);
        }

        private void OnDisable() {
            AudioPool.Main?.UnregisterAudioVolume(this);
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

        public JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default) {
            if (count <= 0) return dependency;
            
            switch (_mode) {
                case Mode.Global:
                    var writeConstWeightJob = new WriteConstWeightJob {
                        weight = _weight,
                        defaultVolumeId = GetHashCode(),
                        results = results,
                    };
                
                    return writeConstWeightJob.Schedule(count, JobExt.BatchFor(count, MinBatch), dependency);
                
                case Mode.Local:
                    var handle = _positionWeightProvider.GetWeight(positions, results, count, dependency);
                    
                    // Multiplying by one changes nothing, and volumes with zero weight are filtered out by the pool.
                    if (_weight >= 1f) return handle;
                    
                    var multiplyWeightJob = new MultiplyWeightJob {
                        mul = _weight,
                        results = results,
                    };
                
                    return multiplyWeightJob.Schedule(count, JobExt.BatchFor(count, MinBatch), handle);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        
        [BurstCompile]
        private struct WriteConstWeightJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public int defaultVolumeId;
            [Unity.Collections.ReadOnly] public float weight;
            
            // Results can be a sub array of a buffer shared between volumes:
            // each volume writes into its own disjoint range, so aliasing check is not applicable.
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<WeightSample> results;

            public void Execute(int index) {
                results[index] = new WeightSample(weight, defaultVolumeId);
            }
        }
        
        [BurstCompile]
        private struct MultiplyWeightJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public float mul;
            
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<WeightSample> results;

            public void Execute(int index) {
                var data = results[index];
                results[index] = new WeightSample(data.weight * mul, data.volumeId);
            }
        }
    }
    
}