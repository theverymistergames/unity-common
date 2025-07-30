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
            public readonly int volumeId;
            
            public ProxyData(float3 position, quaternion rotation, float3 size, int volumeId) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.volumeId = volumeId;
            }
        }
        
        private readonly struct WeightData {
            
            public readonly int volumeId;
            public readonly float weight;
            
            public WeightData(int volumeId, float weight) {
                this.volumeId = volumeId;
                this.weight = weight;
            }
        }

        private NativeArray<ProxyData> _proxyDataArray;
        private int _proxyDataArrayCreationFrame;
        private int _proxyCount;

        private void OnDisable() {
            _proxyDataArray.Dispose();
        }

        public override float GetWeight(Vector3 position, out int volumeId) {
            volumeId = GetInstanceID();
            if (!enabled) return 0f;
            
            UpdateProxyDataArray();
            
            var weightArray = new NativeArray<WeightData>(_proxyCount, Allocator.TempJob);
            var resultArray = new NativeArray<WeightData>(2, Allocator.TempJob);

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

            var result = resultArray[0];
            
            weightArray.Dispose();
            resultArray.Dispose();
            
            volumeId = result.volumeId;
            return result.weight;
        }

        private void UpdateProxyDataArray() {
            int frame = Time.frameCount;
            if (_proxyDataArrayCreationFrame >= frame && _proxyDataArray.IsCreated) return;

            var proxySet = _waterZone.WaterProxySet;
            _proxyCount = proxySet.Count;
            _proxyDataArrayCreationFrame = frame;
            
            if (_proxyDataArray.Length < _proxyCount) {
                _proxyDataArray.Dispose();
                _proxyDataArray = new NativeArray<ProxyData>(_proxyCount, Allocator.Persistent);
            }
            
            int index = 0;
            
            foreach (var proxy in proxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                _proxyDataArray[index++] = new ProxyData(position, rotation, size, _waterZone.GetProxyVolumeId(proxy));
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public float3 position;
            
            public NativeArray<WeightData> weightArray;
            
            public void Execute(int index) {
                var proxyData = proxyDataArray[index];
                
                var localPoint = math.mul(math.inverse(proxyData.rotation), position - proxyData.position);
                var halfSize = proxyData.size * 0.5f;

                if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                    localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                    localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                {
                    weightArray[index] = new WeightData(proxyData.volumeId, 0f);
                    return;
                }
                
                weightArray[index] = new WeightData(proxyData.volumeId, 1f);
            }
        }
        
        [BurstCompile]
        private struct CalculateMaxWeightJob : IJob {
            
            [ReadOnly] public NativeArray<WeightData> weightArray;
            
            public NativeArray<WeightData> result;
            
            public void Execute() {
                float max = float.MinValue;
                int volumeId = 0;
                
                for (int i = 0; i < weightArray.Length; i++) {
                    max = Mathf.Max(max, weightArray[i].weight);
                    volumeId = weightArray[i].volumeId;
                    
                    if (max >= 1f) break;
                }

                result[0] = new WeightData(volumeId, max);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _waterZone = GetComponent<WaterZone>();
        }
#endif
    }
    
}