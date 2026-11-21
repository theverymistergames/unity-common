using MisterGames.Common.Attributes;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Rendering {
    
    public sealed class LampIntensityNoise : MonoBehaviour, IUpdate {
    
        [SerializeField] private LampBehaviour[] _lamps;
        
        [Header("Intensity (normalized)")]
        [SerializeField] [Min(0f)] private float _intensityMul = 1f;
        [SerializeField] [Min(0f)] private float _intensity0 = 0f;
        [SerializeField] [Min(0f)] private float _intensity1 = 1f;
        [SerializeField] [Min(0f)] private float _intensitySmoothing = 5f;
        
        [Header("Noise")]
        [SerializeField] private float _noiseSpeed0 = 1f;
        [SerializeField] private float _noiseSpeed1 = 2f;
        [SerializeField] private float _noiseSpeedChangeRate = 0.01f;
        [SerializeField] [Min(0f)] private float _noiseSpeedSmoothing = 5f;
        
        public float IntensityMul { get => _intensityMul; set => _intensityMul = value; }
        
        private float _indexOffset;
        private float _noiseAccumulator;
        private float _noiseSpeedSmoothed;
        private float[] _initialIntensities;

        private void Awake() {
            _indexOffset = Random.Range(1000f, 10000f);
            _initialIntensities = CaptureIntensities(_lamps);
        }

        public void SetLamps(LampBehaviour[] lamps) {
            for (int i = 0; i < _lamps.Length; i++) {
                if (_lamps[i] != null) _lamps[i].Intensity = _initialIntensities[i];
            }

            _lamps = lamps ?? System.Array.Empty<LampBehaviour>();
            _initialIntensities = CaptureIntensities(_lamps);

            float intensity0 = _intensity0 * _intensityMul;
            float intensity1 = _intensity1 * _intensityMul;

            for (int i = 0; i < _lamps.Length; i++) {
                _lamps[i].Intensity = GetNoiseIntensity(_noiseAccumulator, i, _indexOffset, intensity0, intensity1);
            }
        }

        private static float[] CaptureIntensities(LampBehaviour[] lamps) {
            var intensities = new float[lamps.Length];

            for (int i = 0; i < lamps.Length; i++) {
                if (lamps[i] != null) intensities[i] = lamps[i].Intensity;
            }

            return intensities;
        }

        private static float GetNoiseIntensity(float t, int index, float indexOffset, float intensity0, float intensity1) {
            return math.lerp(intensity0, intensity1, noise.cnoise((float2) (t + index * indexOffset)));
        }

        private void OnEnable() {
            _noiseSpeedSmoothed = GetTargetNoiseSpeed(TimeSources.scaledTime);
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        private float GetTargetNoiseSpeed(float time) {
            return Mathf.Lerp(_noiseSpeed0, _noiseSpeed1, Mathf.PerlinNoise1D(time * _noiseSpeedChangeRate));
        }
        
        void IUpdate.OnUpdate(float dt) {
            float time = TimeSources.scaledTime;
            float targetSpeed = GetTargetNoiseSpeed(time);
            
            _noiseSpeedSmoothed = _noiseSpeedSmoothing.SmoothExpNonZero(targetSpeed, _noiseSpeedSmoothing, dt);
            _noiseAccumulator += _noiseSpeedSmoothed * dt;

            var intensityArray = new NativeArray<float>(_lamps.Length, Allocator.TempJob);

            for (int i = 0; i < _lamps.Length; i++) {
                intensityArray[i] = _lamps[i].Intensity;
            }
            
            var job = new CalculateIntensityJob {
                t = _noiseAccumulator,
                dt = dt,
                intensity0 = _intensity0 * _intensityMul,
                intensity1 = _intensity1 * _intensityMul,
                smoothing = _intensitySmoothing,
                intensityArray = intensityArray,
                indexOffset = _indexOffset,
            };

            job.Schedule(_lamps.Length, JobExt.BatchFor(_lamps.Length)).Complete();
            
            for (int i = 0; i < _lamps.Length; i++) {
                _lamps[i].Intensity = intensityArray[i];
            }

            intensityArray.Dispose();
        }

        [BurstCompile]
        private struct CalculateIntensityJob : IJobParallelFor {

            [Unity.Collections.ReadOnly] public float t;
            [Unity.Collections.ReadOnly] public float dt;
            [Unity.Collections.ReadOnly] public float intensity0;
            [Unity.Collections.ReadOnly] public float intensity1;
            [Unity.Collections.ReadOnly] public float smoothing;
            [Unity.Collections.ReadOnly] public float indexOffset;
            
            public NativeArray<float> intensityArray;
            
            public void Execute(int index) {
                float currentIntensity = intensityArray[index];
                float targetIntensity = GetNoiseIntensity(t, index, indexOffset, intensity0, intensity1);

                intensityArray[index] = currentIntensity.SmoothExpNonZero(targetIntensity, smoothing, dt);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            FetchLightsInChildren();
        }

        [Button]
        private void FetchLightsInChildren() {
            _lamps = GetComponentsInChildren<LampBehaviour>();
            
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
    
}