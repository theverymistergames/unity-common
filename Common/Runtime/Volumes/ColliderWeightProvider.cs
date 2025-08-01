using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class ColliderWeightProvider : PositionWeightProvider {
    
        [SerializeField] private Collider _collider;
        [SerializeField] [Min(0f)] private float _blendDistance = 1f;

        public override WeightData GetWeight(Vector3 position) {
            float w = GetWeight(position, _collider.ClosestPoint(position), _blendDistance);
            return new WeightData(w, volumeId: GetHashCode());
        }

        public override void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count) {
            if (count <= 0) return;
            
            var commands = new NativeArray<ClosestPointCommand>(count, Allocator.TempJob);
            var closestPoints = new NativeArray<Vector3>(count, Allocator.TempJob);

            var trf = _collider.transform;
            trf.GetPositionAndRotation(out var pos, out var rot);
            
            var prepareCommandsJob = new PrepareColliderCommandsJob {
                positions = positions,
                colliderInstanceId = _collider.GetInstanceID(),
                position = pos,
                rotation = rot,
                scale = trf.localScale,
                commands = commands,
            };
            
            var weightJob = new CalculateWeightJob {
                positions = positions,
                closestPoints = closestPoints,
                blend = _blendDistance,
                volumeId = GetHashCode(),
                results = results
            };
            
            int batchCount = UnityJobsExt.BatchCount(count);

            var prepareCommandsJobHandle = prepareCommandsJob.Schedule(count, batchCount);
            var commandsJobHandle = ClosestPointCommand.ScheduleBatch(commands, closestPoints, batchCount, prepareCommandsJobHandle);
            
            weightJob.Schedule(count, UnityJobsExt.BatchCount(count), commandsJobHandle).Complete();
        }

        private static float GetWeight(float3 position, float3 closestPoint, float blend) {
            if (position.Approx(closestPoint)) return 1f;
            if (math.lengthsq(position - closestPoint) > blend * blend) return 0f;

            return blend > 0f 
                ? math.clamp(1f - math.length(position - closestPoint) / blend, 0f, 1f)
                : 1f;
        }
        
        [BurstCompile]
        private struct PrepareColliderCommandsJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public NativeArray<float3> positions;
            [Unity.Collections.ReadOnly] public int colliderInstanceId;
            [Unity.Collections.ReadOnly] public float3 position;
            [Unity.Collections.ReadOnly] public quaternion rotation;
            [Unity.Collections.ReadOnly] public float3 scale;
            
            public NativeArray<ClosestPointCommand> commands;
            
            public void Execute(int index) {
                commands[index] = new ClosestPointCommand(positions[index], colliderInstanceId, position, rotation, scale);
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public NativeArray<float3> positions;
            [Unity.Collections.ReadOnly] public NativeArray<Vector3> closestPoints;
            [Unity.Collections.ReadOnly] public float blend;
            [Unity.Collections.ReadOnly] public int volumeId;
            
            public NativeArray<WeightData> results;

            public void Execute(int index) {
                float w = GetWeight(positions[index], closestPoints[index], blend);
                results[index] = new WeightData(w, volumeId);
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private Vector3 _testPoint;

        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            var p = transform.TransformPoint(_testPoint);
            var b = _collider.ClosestPoint(p);
            
            DebugExt.DrawSphere(p, 0.05f, Color.white);
            DebugExt.DrawLine(p, b, Color.white);
            
            float w = GetWeight(p, b, _blendDistance);
            
            DebugExt.DrawLabel(p + transform.up * 0.1f, $"W = {w:0.000}");
        }
#endif
    }
    
}