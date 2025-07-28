using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZone))]
    public sealed class WaterZoneWeightProvider : PositionWeightProvider {

        [SerializeField] private WaterZone _waterZone;

        private readonly struct ProxyData {
            
            public readonly float3 position;
            public readonly quaternion rotation;
            public readonly float3 size;
            
            public ProxyData(float3 position, quaternion rotation, float3 size) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
            }
        }

        private NativeArray<ProxyData> _proxyDataArray;
        private int _proxyDataArrayCreationFrame;
        private int _proxyCount;

        private void OnDisable() {
            _proxyDataArray.Dispose();
        }

        public override float GetWeight(Vector3 position) {
            if (!enabled) return 0f;
            
            UpdateProxyDataArray();
            
            var weightArray = new NativeArray<float>(_proxyCount, Allocator.TempJob);
            var resultArray = new NativeArray<float>(2, Allocator.TempJob);

            var calculateWeightJob = new CalculateWeightJob {
                proxyDataArray = _proxyDataArray,
                position = position,
                weightArray = weightArray,
            };

            var calculateMaxWeightJob = new CalculateMaxWeightJob {
                weightArray = weightArray,
                result = resultArray,
            };
            
            var calculateWeightJobHandle = calculateWeightJob.Schedule(_proxyCount, innerloopBatchCount: 256);
            calculateMaxWeightJob.Schedule(calculateWeightJobHandle).Complete();

            float weight = resultArray[0];
            
            weightArray.Dispose();
            resultArray.Dispose();
            
            return weight;
        }

        private void UpdateProxyDataArray() {
            int frame = Time.frameCount;
            if (_proxyDataArrayCreationFrame >= frame && _proxyDataArray.IsCreated) return;

            var proxySet = _waterZone.WaterProxies;
            _proxyCount = proxySet.Count;
            _proxyDataArrayCreationFrame = frame;
            
            if (_proxyDataArray.Length < _proxyCount) {
                _proxyDataArray.Dispose();
                _proxyDataArray = new NativeArray<ProxyData>(_proxyCount, Allocator.Persistent);
            }
            
            int index = 0;
            
            foreach (var proxy in proxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                _proxyDataArray[index++] = new ProxyData(position, rotation, size);
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public float3 position;
            
            public NativeArray<float> weightArray;
            
            public void Execute(int index) {
                var proxyData = proxyDataArray[index];
                
                var localPoint = math.mul(math.inverse(proxyData.rotation), position - proxyData.position);
                var halfSize = proxyData.size * 0.5f;

                if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                    localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                    localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                {
                    weightArray[index] = 0f;
                    return;
                }
                
                weightArray[index] = 1f;
            }
        }
        
        [BurstCompile]
        private struct CalculateMaxWeightJob : IJob {
            
            [ReadOnly] public NativeArray<float> weightArray;
            
            public NativeArray<float> result;
            
            public void Execute() {
                float max = float.MinValue;
                
                for (int i = 0; i < weightArray.Length; i++) {
                    max = Mathf.Max(max, weightArray[i]);
                    if (max >= 1f) break;
                }

                result[0] = max;
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _waterZone = GetComponent<WaterZone>();
        }
#endif
    }
    
}