using MisterGames.Common.Attributes;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Rendering {
    
    public sealed class LightIntensityNoise : MonoBehaviour, IUpdate {
    
        [SerializeField] private Light[] _lights;
        
        [Header("Intensity")]
        [SerializeField] [Min(0f)] private float _intensity0 = 100f;
        [SerializeField] [Min(0f)] private float _intensity1 = 1000f;
        [SerializeField] [Min(0f)] private float _intensitySmoothing = 5f;
        
        [Header("Noise")]
        [SerializeField] private float _noiseSpeed0 = 1f;
        [SerializeField] private float _noiseSpeed1 = 2f;
        [SerializeField] private float _noiseSpeedChangeRate = 0.01f;
        [SerializeField] [Min(0f)] private float _noiseSpeedSmoothing = 5f;
        
        private const float IndexOffset = 1000f;

        private float _noiseAccumulator;
        private float _noiseSpeedSmoothed;
        
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

            var intensityArray = new NativeArray<float>(_lights.Length, Allocator.TempJob);

            for (int i = 0; i < _lights.Length; i++) {
                intensityArray[i] = _lights[i].intensity;
            }
            
            var job = new CalculateIntensityJob {
                t = _noiseAccumulator,
                dt = dt,
                intensity0 = _intensity0,
                intensity1 = _intensity1,
                smoothing = _intensitySmoothing,
                intensityArray = intensityArray,
            };

            job.Schedule(_lights.Length, JobExt.BatchFor(_lights.Length)).Complete();
            
            for (int i = 0; i < _lights.Length; i++) {
                _lights[i].intensity = intensityArray[i];
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
            
            public NativeArray<float> intensityArray;
            
            public void Execute(int index) {
                float currentIntensity = intensityArray[index];
                
                float lerp = noise.cnoise((float2) (t + index * IndexOffset)) * 2f - 1f;
                float targetIntensity = math.lerp(intensity0, intensity1, lerp);
                
                intensityArray[index] = currentIntensity.SmoothExpNonZero(targetIntensity, smoothing, dt);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            FetchLightsInChildren();
        }

        [Button]
        private void FetchLightsInChildren() {
            _lights = GetComponentsInChildren<Light>();
            
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
    
}