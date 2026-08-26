using MisterGames.Common.Attributes;
using MisterGames.Common.Colors;
using MisterGames.Common.Jobs;
using MisterGames.Common.Maths;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class ColliderWeightProvider : PositionWeightProvider {
    
        [SerializeField] private Collider _collider;
        [SerializeField] [Min(0f)] private float _blendDistance = 1f;
        [SerializeReference] [SubclassSelector] private IPositionWeightProcessor[] _processors;
        
        private void OnEnable() {
            for (int i = 0; i < _processors?.Length; i++) {
                _processors[i].Initialize();
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _processors?.Length; i++) {
                _processors[i].DeInitialize();
            }
        }

        private float GetProcessorsWeight() {
            float w = 1f;

            for (int i = 0; i < _processors.Length; i++) {
                w *= _processors[i].GetWeight();
            }
            
            return w;
        }
        
        public override WeightData GetWeight(Vector3 position) {
            var closestPoint = _collider.ClosestPoint(position);
            float w = GetWeight(position, closestPoint, _blendDistance) * GetProcessorsWeight();
            return new WeightData(w, volumeId: GetHashCode(), closestPoint);
        }

        public override JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default) {
            if (count <= 0) return dependency;

            float wMul = GetProcessorsWeight();
            var commands = new NativeArray<ClosestPointCommand>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var closestPoints = new NativeArray<Vector3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var trf = _collider.transform;
            trf.GetPositionAndRotation(out var pos, out var rot);
            
            var prepareCommandsJob = new PrepareColliderCommandsJob {
                positions = positions,
                colliderInstanceId = _collider.GetEntityId(),
                position = pos,
                rotation = rot,
                scale = trf.localScale,
                commands = commands,
            };
            
            var weightJob = new CalculateWeightJob {
                positions = positions,
                closestPoints = closestPoints,
                blend = _blendDistance,
                weightMul = wMul,
                volumeId = GetHashCode(),
                results = results
            };
            
            int batchCount = JobExt.BatchFor(count, MinBatch);

            var prepareCommandsJobHandle = prepareCommandsJob.Schedule(count, batchCount, dependency);
            var commandsJobHandle = ClosestPointCommand.ScheduleBatch(commands, closestPoints, batchCount, prepareCommandsJobHandle);
            var weightJobHandle = weightJob.Schedule(count, batchCount, commandsJobHandle);

            commands.Dispose(weightJobHandle);
            closestPoints.Dispose(weightJobHandle);
            
            return weightJobHandle;
        }

        private static float GetWeight(float3 position, float3 closestPoint, float blend) {
            if (position.Approx(closestPoint)) return 1f;
            
            float distanceSqr = math.lengthsq(position - closestPoint);
            if (distanceSqr > blend * blend) return 0f;

            return blend > 0f 
                ? math.clamp(1f - math.sqrt(distanceSqr) / blend, 0f, 1f)
                : 1f;
        }
        
        [BurstCompile]
        private struct PrepareColliderCommandsJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public NativeArray<float3> positions;
            [Unity.Collections.ReadOnly] public EntityId colliderInstanceId;
            [Unity.Collections.ReadOnly] public float3 position;
            [Unity.Collections.ReadOnly] public quaternion rotation;
            [Unity.Collections.ReadOnly] public float3 scale;
            
            [WriteOnly] public NativeArray<ClosestPointCommand> commands;
            
            public void Execute(int index) {
                commands[index] = new ClosestPointCommand(positions[index], colliderInstanceId, position, rotation, scale);
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [Unity.Collections.ReadOnly] public NativeArray<float3> positions;
            [Unity.Collections.ReadOnly] public NativeArray<Vector3> closestPoints;
            [Unity.Collections.ReadOnly] public float blend;
            [Unity.Collections.ReadOnly] public float weightMul;
            [Unity.Collections.ReadOnly] public int volumeId;
            
            // Results can be a sub array of a bigger buffer shared between volumes:
            // each volume writes into its own disjoint range, so aliasing check is not applicable.
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<WeightSample> results;

            public void Execute(int index) {
                float w = GetWeight(positions[index], closestPoints[index], blend);
                results[index] = new WeightSample(w * weightMul, volumeId);
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showGizmo;
        [VisibleIf(nameof(_showGizmo))]
        [SerializeField] private Color _color = Color.lightGreen.WithAlpha(0.3f);
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private Vector3 _testPoint;

        private void Reset() {
            _collider = GetComponent<Collider>();
        }

        private void OnValidate() {
            for (int i = 0; i < _processors?.Length; i++) {
                _processors[i]?.OnValidate();
            }
        }

        private void OnDrawGizmos() {
            if (_collider == null) return;

            if (_showGizmo) {
                DebugExt.DrawCollider(_collider, _color, solid: true);
                DebugExt.DrawCollider(_collider, _color.WithAlpha(_color.a * 0.5f), expand: _blendDistance, solid: true);
            }

            if (_showDebugInfo) {
                var p = transform.TransformPoint(_testPoint);
                var b = _collider.ClosestPoint(p);

                DebugExt.DrawSphere(p, 0.05f, Color.white);
                DebugExt.DrawLine(p, b, Color.white);

                float w = GetWeight(p, b, _blendDistance);

                DebugExt.DrawLabel(p + transform.up * 0.1f, $"W = {w:0.000}");   
            }
        }
#endif
    }
    
}
