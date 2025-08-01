using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class RadialWeightProvider : PositionWeightProvider {

        [SerializeField] private Transform _center;
        [SerializeField] [Range(0f, 1f)] private float _fallOff = 1f;
        [SerializeField] [Min(0f)] private float _innerRadius = 1f;
        [SerializeField] [Min(0f)] private float _outerRadius = 2f;

        public override WeightData GetWeight(Vector3 position) {
            float w = GetWeight(position, _center.position, new float2(_innerRadius, _outerRadius), _fallOff);
            return new WeightData(w, volumeId: GetHashCode());
        }

        public override void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count) {
            if (count <= 0) return;
            
            var job = new CalculateWeightJob {
                positions = positions,
                center = _center.position,
                radiusInOut = new float2(_innerRadius, _outerRadius),
                fallOff = _fallOff,
                volumeId = GetHashCode(),
                results = results
            };
            
            job.Schedule(count, UnityJobsExt.BatchCount(count)).Complete();
        }

        private static float GetWeight(float3 position, float3 center, float2 radiusInOut, float fallOff) {
            if (fallOff <= 0f ||
                math.lengthsq(position - center) <= radiusInOut.x * radiusInOut.x) 
            {
                return 1f;
            }
            
            if (radiusInOut.y - radiusInOut.x <= 0f) {
                return 1f - fallOff;
            }
            
            float x = (math.length(center - position) - radiusInOut.x) / (radiusInOut.y - radiusInOut.x);
            return math.clamp(1f + 2f * fallOff * (1f / (x + 1f) - 1f), 0f, 1f);
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public NativeArray<float3> positions;
            [Unity.Collections.ReadOnly] public float3 center;
            [Unity.Collections.ReadOnly] public float2 radiusInOut;
            [Unity.Collections.ReadOnly] public float fallOff;
            [Unity.Collections.ReadOnly] public int volumeId;
            
            public NativeArray<WeightData> results;

            public void Execute(int index) {
                float w = GetWeight(positions[index], center, radiusInOut, fallOff);
                results[index] = new WeightData(w, volumeId);
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private float _testPoint = 1f;
        
        private void Reset() {
            _center = transform;
        }
        
        private void OnValidate() {
            if (_outerRadius < _innerRadius) _outerRadius = _innerRadius;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _center == null) return;

            _center.GetPositionAndRotation(out var position, out var rotation);

            float w = 1f;
            DebugExt.DrawLabel(position + rotation * Vector3.up * 0.12f, $"W = {w:0.000}");
            
            if (_fallOff <= 0f) return;

            var pIn = position + rotation * Vector3.forward * _innerRadius;
            w = 1f;
            DebugExt.DrawSphere(position, _innerRadius, Color.white, gizmo: true);
            DebugExt.DrawPointer(pIn, Color.white, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pIn + rotation * Vector3.right * 0.12f, $"W = {w:0.000}", color: Color.white);
            DebugExt.DrawLine(pIn, position, Color.white, gizmo: true);
            
            var pOut = position + rotation * Vector3.forward * _outerRadius; 
            DebugExt.DrawSphere(position, _outerRadius, Color.yellow, gizmo: true);
            DebugExt.DrawPointer(pOut, Color.yellow, 0.03f, gizmo: true);

            w = GetWeight(pOut, position, new float2(_innerRadius, _outerRadius), _fallOff);
            DebugExt.DrawLabel(pOut - rotation * Vector3.right * 0.12f, $"W = {w:0.000}", color: Color.yellow);
            DebugExt.DrawLine(pOut, pIn, Color.yellow, gizmo: true);
            
            var pFar = position + rotation * Vector3.forward * _testPoint;
            w = GetWeight(pFar, position, new float2(_innerRadius, _outerRadius), _fallOff);
            DebugExt.DrawPointer(pFar, Color.cyan, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pFar + rotation * Vector3.forward * 0.12f, $"W = {w:0.000}", color: Color.cyan);
            
            if (_testPoint > _outerRadius) DebugExt.DrawLine(pFar, pOut, Color.cyan, gizmo: true);
        }
#endif
    }
    
}