using MisterGames.Common.Volumes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZone))]
    public sealed class WaterZoneWeightProvider : PositionWeightProvider {

        [SerializeField] private WaterZone _waterZone;

        private struct ProxyData {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 size;
        }
        
        public override float GetWeight(Vector3 position) {
            var proxyDataArray = CreateProxyDataArray(out int proxyCount);
            var weightArray = new NativeArray<float>(proxyCount, Allocator.TempJob);
            var resultArray = new NativeArray<float>(2, Allocator.TempJob);

            var calculateWeightJob = new CalculateWeightJob {
                proxyDataArray = proxyDataArray,
                position = position,
                weightArray = weightArray,
            };

            var calculateMaxWeightJob = new CalculateMaxWeightJob {
                weightArray = weightArray,
                result = resultArray,
            };
            
            var calculateWeightJobHandle = calculateWeightJob.Schedule(proxyCount, innerloopBatchCount: 256);
            calculateMaxWeightJob.Schedule(calculateWeightJobHandle).Complete();

            float weight = resultArray[0];
            
            proxyDataArray.Dispose();
            weightArray.Dispose();
            resultArray.Dispose();
            
            return weight;
        }

        private NativeArray<ProxyData> CreateProxyDataArray(out int count) {
            var proxySet = _waterZone.WaterProxies;
            count = proxySet.Count;
            
            int index = 0;
            var proxyDataArray = new NativeArray<ProxyData>(count, Allocator.TempJob);
            
            foreach (var proxy in proxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                
                proxyDataArray[index++] = new ProxyData {
                    position = position,
                    rotation = rotation,
                    size = size,
                };
            }

            return proxyDataArray;
        }
        
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public Vector3 position;
            
            public NativeArray<float> weightArray;
            
            public void Execute(int index) {
                var proxyData = proxyDataArray[index];

                var localPoint = Quaternion.Inverse(proxyData.rotation) * (position - proxyData.position);
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