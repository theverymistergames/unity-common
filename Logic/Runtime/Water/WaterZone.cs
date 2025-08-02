using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Jobs;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZone : MonoBehaviour, IActorComponent, IUpdate, IWaterZone {

        [Header("Volumes")]
        [SerializeField] private WaterZoneVolume[] _predefinedVolumes;
        [SerializeField] private WaterZoneVolumeCluster[] _predefinedVolumeClusters;
        
        [Header("Surface")]
        [SerializeField] private float _surfaceOffset;
        [SerializeField] [Min(0f)] private float _forceLevelDecrease = 0f;
        
        [Header("Motion")]
        [SerializeField] private float _buoyancyDefault = 0f;
        [SerializeField] private float _deceleration = 0f;
        [SerializeField] private float _torqueDeceleration = 0f;
        
        [Header("Force")]
        [SerializeField] private float _force = 0f;
        [SerializeField] private ForceSource _forceSource;
        
        [Header("Random")]
        [SerializeField] private float _randomForce = 0f;
        [SerializeField] private float _randomTorque = 0f;
        [SerializeField] private float _randomForceSpeed = 0f;
        [SerializeField] private float _randomTorqueSpeed = 0f;
        
        private enum ForceSource {
            Constant,
            UseGravityMagnitude,
        }
        
        private readonly struct WaterClientData {

            public readonly bool isMainRigidbody;
            public readonly IWaterClient waterClient;
            
            public WaterClientData(bool isMainRigidbody, IWaterClient waterClient) {
                this.isMainRigidbody = isMainRigidbody;
                this.waterClient = waterClient;
            }
        }
        
        private readonly struct VolumeData {
            
            public readonly float3 position;
            public readonly quaternion rotation;
            public readonly float3 size;
            public readonly float surfaceOffset;
            public readonly float forceMul;

            public VolumeData(float3 position, quaternion rotation, float3 size, float surfaceOffset, float forceMul) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.surfaceOffset = surfaceOffset;
                this.forceMul = forceMul;
            }
        }

        private readonly struct FloatingPointsPerRbData {

            public readonly int firstFloatingPointIndex;
            public readonly int floatingPointCount;
            
            public FloatingPointsPerRbData(int firstFloatingPointIndex, int floatingPointCount) {
                this.firstFloatingPointIndex = firstFloatingPointIndex;
                this.floatingPointCount = floatingPointCount;
            }
        }

        private readonly struct FloatingData {
            
            public readonly int rbId;
            public readonly int rbIndex;
            public readonly float3 point;
            public readonly float3 velocity;
            public readonly float3 angularVelocity;
            public readonly float buoyancy;
            public readonly float deceleration;
#if UNITY_EDITOR
            public readonly bool drawGizmos;
#endif
            
            public FloatingData(int rbId, int rbIndex, float3 point, float3 velocity, float3 angularVelocity, float buoyancy, float deceleration
#if UNITY_EDITOR
                , bool drawGizmos
#endif            
            ) {
                this.rbId = rbId;
                this.rbIndex = rbIndex;
                this.point = point;
                this.velocity = velocity;
                this.angularVelocity = angularVelocity;
                this.buoyancy = buoyancy;
                this.deceleration = deceleration;
#if UNITY_EDITOR
                this.drawGizmos = drawGizmos;          
#endif
            }
        }
        
        private readonly struct ForceData {
            
            public readonly int rbId;
            public readonly int rbIndex;
            public readonly float3 point;
            public readonly float3 force;
            public readonly float3 torque;
#if UNITY_EDITOR
            public readonly float3 surfacePoint;
            public readonly bool drawGizmos;
#endif
            
            public ForceData(int rbId, int rbIndex, float3 point, float3 force, float3 torque
#if UNITY_EDITOR
                , float3 surfacePoint
                , bool drawGizmos
#endif
            ) {
                this.rbId = rbId;
                this.rbIndex = rbIndex;
                this.point = point;
                this.force = force;
                this.torque = torque;
#if UNITY_EDITOR
                this.surfacePoint = surfacePoint;
                this.drawGizmos = drawGizmos;
#endif
            }
        }

        public delegate void TriggerAction(Collider collider, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal);
        
        public event TriggerAction OnColliderEnter = delegate { };
        public event TriggerAction OnColliderExit = delegate { };

        public HashSet<IWaterZoneVolume> Volumes { get; } = new();
        private readonly Dictionary<int, int> _volumeIdMap = new();
        
        private const float NoiseOffset = 100f;

        private readonly Dictionary<int, int> _colliderToRbIdMap = new();
        private readonly Dictionary<int, int> _rbIdToColliderCountMap = new();

        private readonly List<(int id, Rigidbody rb)> _rbList = new();
        private readonly Dictionary<int, int> _rbIdToIndexMap = new();
        private readonly Dictionary<int, WaterClientData> _rbIdToWaterClientDataMap = new();
        private readonly Dictionary<int, int> _rbIdToVolumeCountMap = new();

        private NativeArray<VolumeData> _volumeDataArray;
        private int _floatingPointsCount;
        
        private void OnEnable() {
            for (int i = 0; i < _predefinedVolumes.Length; i++) {
                AddVolume(_predefinedVolumes[i]);
            }
            
            for (int i = 0; i < _predefinedVolumeClusters.Length; i++) {
                AddVolumeCluster(_predefinedVolumeClusters[i]);
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            for (int i = 0; i < _predefinedVolumes.Length; i++) {
                RemoveVolume(_predefinedVolumes[i]);
            }

            for (int i = 0; i < _predefinedVolumeClusters.Length; i++) {
                RemoveVolumeCluster(_predefinedVolumeClusters[i]);
            }
            
            _volumeDataArray.Dispose();
        }

        private void OnDestroy() {
            _rbIdToWaterClientDataMap.Clear();
            _rbIdToIndexMap.Clear();
            _rbList.Clear();
            
            Volumes.Clear();
            _volumeIdMap.Clear();
        }

        public void AddVolume(IWaterZoneVolume volume) {
            Volumes.Add(volume);
            volume.BindZone(this);
        }

        public void RemoveVolume(IWaterZoneVolume volume) {
            Volumes.Remove(volume);
            volume.UnbindZone(this);
        }

        public void AddVolumeCluster(IWaterZoneVolumeCluster cluster) {
            int id = cluster.ClusterId;
            int count = cluster.VolumeCount;

            for (int i = 0; i < count; i++) {
                _volumeIdMap[cluster.GetVolumeId(i)] = id;
            }
        }

        public void RemoveVolumeCluster(IWaterZoneVolumeCluster cluster) {
            int count = cluster.VolumeCount;
            
            for (int i = 0; i < count; i++) {
                _volumeIdMap.Remove(cluster.GetVolumeId(i));
            }
        }

        public int GetVolumeId(IWaterZoneVolume volume) {
            return _volumeIdMap.GetValueOrDefault(volume.VolumeId, volume.VolumeId);
        }

        public void TriggerEnter(Collider collider, IWaterZoneVolume volume) {
            if (collider.attachedRigidbody is not {} rb) return;
            
            int id = collider.GetHashCode();
            int rbId = rb.GetHashCode();
            
            if (!_colliderToRbIdMap.TryAdd(id, rbId)) return;
            
            var pos = collider.transform.position;
            volume.SampleSurface(pos, out var surfacePoint, out var surfaceNormal);
            OnColliderEnter.Invoke(collider, volume.GetClosestPoint(pos), surfacePoint, surfaceNormal);
            
            int oldCount = _rbIdToColliderCountMap.GetValueOrDefault(rbId);
            _rbIdToColliderCountMap[rbId] = oldCount + 1;

            if (oldCount <= 0) TriggerEnterRigidbody(rb);
        }

        public void TriggerExit(Collider collider, IWaterZoneVolume volume) {
            if (collider == null) return;
            
            int id = collider.GetHashCode();
            if (collider == null || !_colliderToRbIdMap.Remove(id, out int rbId)) return;

            var pos = collider.transform.position;
            volume.SampleSurface(pos, out var surfacePoint, out var surfaceNormal);
            OnColliderExit.Invoke(collider, volume.GetClosestPoint(pos), surfacePoint, surfaceNormal);
            
            int newCount = _rbIdToColliderCountMap.GetValueOrDefault(rbId) - 1;
            if (newCount > 0) {
                _rbIdToColliderCountMap[rbId] = newCount;
                return;
            }

            _rbIdToColliderCountMap.Remove(rbId);
            TriggerExitRigidbody(rbId);
        }

        private void TriggerEnterRigidbody(Rigidbody rigidbody) {
            int id = rigidbody.GetHashCode();
            _rbIdToVolumeCountMap[id] = _rbIdToVolumeCountMap.GetValueOrDefault(id) + 1;
            
            if (!_rbIdToIndexMap.TryAdd(id, _rbList.Count)) return;
            
            _rbList.Add((id, rigidbody));

            if (rigidbody.TryGetComponent(out IWaterClient waterClient)) {
                _rbIdToWaterClientDataMap[id] = new WaterClientData(
                    isMainRigidbody: waterClient.Rigidbody == rigidbody,
                    waterClient
                );
            
                _floatingPointsCount += waterClient.FloatingPointCount;
            }
            else {
                _floatingPointsCount++;
            }
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void TriggerExitRigidbody(int rbId) {
            int volumesLeft = _rbIdToVolumeCountMap[rbId] - 1;

            if (volumesLeft > 0) _rbIdToVolumeCountMap[rbId] = volumesLeft;
            else _rbIdToVolumeCountMap.Remove(rbId);
            
            if (volumesLeft > 0 || !_rbIdToIndexMap.Remove(rbId, out int index)) return;

            _rbList[index] = default;
            
            if (_rbIdToWaterClientDataMap.Remove(rbId, out var data)) {
                _floatingPointsCount -= data.waterClient.FloatingPointCount;
            }
            else {
                _floatingPointsCount--;
            }
        }

        void IUpdate.OnUpdate(float dt) {
            CreateVolumeDataArray(out var volumeDataArray, out int volumeCount);
            CreateFloatingData(out var rbDataArray, out int rbCount, out var floatingDataArray, out int floatingPointsCount);

            var forceDataArray = new NativeArray<ForceData>(floatingPointsCount, Allocator.TempJob);
            var forceMulArray = new NativeArray<float>(rbCount, Allocator.TempJob);

            float time = Time.time;
            
            var calculateForceJob = new CalculateForceJob {
                floatingDataArray = floatingDataArray,
                volumeDataArray = volumeDataArray,
                volumeCount = volumeCount,
                forceLevel = _forceLevelDecrease,
                forceDataArray = forceDataArray,
                torqueDeceleration = _torqueDeceleration,
                randomForce = _randomForce,
                randomForceT = time * _randomForceSpeed,
                randomTorque = _randomTorque,
                randomTorqueT = time * _randomTorqueSpeed,
            };

            var calculateValidFloatingPointsCountJob = new CalculateValidFloatingPointsPerRbJob {
                rbDataArray = rbDataArray,
                forceDataArray = forceDataArray,
                forceMulArray = forceMulArray,
            };
            
            var calculateForceJobHandle = calculateForceJob.Schedule(floatingPointsCount, JobExt.BatchFor(floatingPointsCount));
            calculateValidFloatingPointsCountJob.Schedule(rbCount, JobExt.BatchFor(rbCount), calculateForceJobHandle).Complete();

            for (int i = 0; i < forceDataArray.Length; i++) {
                var forceData = forceDataArray[i];
                if (forceData.rbId == 0) continue;
                
                var rb = _rbList[_rbIdToIndexMap[forceData.rbId]].rb;
                float mul = forceMulArray[forceData.rbIndex];
                
                rb.AddForceAtPosition(mul * forceData.force, forceData.point, ForceMode.Acceleration);
                rb.AddTorque(mul * forceData.torque, ForceMode.Acceleration);
                
#if UNITY_EDITOR
                if (forceData.drawGizmos) DebugExt.DrawLine(forceData.point, forceData.surfacePoint, Color.white);
                if (forceData.drawGizmos) DebugExt.DrawRay(forceData.point, forceData.force * mul, Color.magenta);
#endif
            }

            rbDataArray.Dispose();
            volumeDataArray.Dispose();
            floatingDataArray.Dispose();
            forceMulArray.Dispose();
            forceDataArray.Dispose();
            
            if (_rbList.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void CreateVolumeDataArray(out NativeArray<VolumeData> volumeDataArray, out int count) {
            count = Volumes.Count;
            
            int index = 0;
            volumeDataArray = new NativeArray<VolumeData>(count, Allocator.TempJob);
            
            foreach (var volume in Volumes) {
                volume.GetBox(out var position, out var rotation, out var size);
                
                volumeDataArray[index++] = new VolumeData(
                    position,
                    rotation,
                    size,
                    surfaceOffset: _surfaceOffset + volume.SurfaceOffset,
                    forceMul: _force * _forceSource switch {
                        ForceSource.Constant => 1f,
                        ForceSource.UseGravityMagnitude => 
                            CustomGravity.Main.TryGetGlobalGravity(position, out var g) 
                                ? g.magnitude 
                                : Physics.gravity.magnitude,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                );
            }
        }

        private void CreateFloatingData(
            out NativeArray<FloatingPointsPerRbData> rbDataArray, out int validRbCount,
            out NativeArray<FloatingData> floatingDataArray, out int floatingPointsCount) 
        {
            int rbCount = _rbList.Count;
            
            floatingPointsCount = 0;
            validRbCount = rbCount;
            int rbIndexer = 0;
            
            rbDataArray = new NativeArray<FloatingPointsPerRbData>(rbCount, Allocator.TempJob);
            floatingDataArray = new NativeArray<FloatingData>(_floatingPointsCount, Allocator.TempJob);

            for (int i = rbCount - 1; i >= 0; i--) {
                (int id, var rb) = _rbList[i];
                
                if (id != 0 && rb != null) {
                    if (rb.isKinematic) continue;

                    int rbIndex = rbIndexer++;
                    int firstPointIndex = floatingPointsCount;
                    
                    float3 velocity = rb.linearVelocity;
                    float3 angularVelocity = rb.angularVelocity;
                    
                    if (_rbIdToWaterClientDataMap.TryGetValue(id, out var data) && data.isMainRigidbody) {
                        if (data.waterClient.IgnoreWaterZone) continue;

                        int fpCount = data.waterClient.FloatingPointCount;
                        float buoyancy = data.waterClient.Buoyancy;
                        float decelerationMul = data.waterClient.DecelerationMul * _deceleration;
                        
                        for (int j = 0; j < fpCount; j++) {
                            floatingDataArray[floatingPointsCount++] = new FloatingData(
                                id,
                                rbIndex,
                                data.waterClient.GetFloatingPoint(j),
                                velocity,
                                angularVelocity,
                                buoyancy,
                                decelerationMul
#if UNITY_EDITOR
                                , data.waterClient.DrawGizmos
#endif
                            );
                        }
                        
                        rbDataArray[rbIndex] = new FloatingPointsPerRbData(firstPointIndex, fpCount);
                        continue;
                    }
                    
                    floatingDataArray[floatingPointsCount++] = new FloatingData(
                        id,
                        rbIndex,
                        rb.position,
                        velocity,
                        angularVelocity,
                        _buoyancyDefault,
                        _deceleration
#if UNITY_EDITOR
                        , _showNoClientGizmo
#endif
                    );
                    
                    rbDataArray[rbIndex] = new FloatingPointsPerRbData(firstPointIndex, 1);
                    continue;
                }

                if (_rbIdToIndexMap.Remove(id)) {
                    if (_rbIdToWaterClientDataMap.Remove(id, out var data)) {
                        _floatingPointsCount -= data.waterClient.FloatingPointCount;
                    }
                    else {
                        _floatingPointsCount--;
                    }
                }
                
                if (_rbList[--validRbCount] is var swap && swap.rb != null) 
                {
                    _rbList[i] = swap;
                    _rbIdToIndexMap[swap.id] = i;
                }
            }

            _rbList.RemoveRange(validRbCount, rbCount - validRbCount);
        }

        private static float3 GetNoiseVector(float t, float offset) {
            return new float3(
                noise.cnoise((float2) t + offset),
                noise.cnoise((float2) t + 7f * offset),
                noise.cnoise((float2) t + 11f * offset)
            );
        }
        
        [BurstCompile]
        private struct CalculateForceJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<FloatingData> floatingDataArray;
            [ReadOnly] public NativeArray<VolumeData> volumeDataArray;
            [ReadOnly] public int volumeCount;
            [ReadOnly] public float forceLevel;
            [ReadOnly] public float torqueDeceleration;
            
            [ReadOnly] public float randomForceT;
            [ReadOnly] public float randomForce;
            [ReadOnly] public float randomTorqueT;
            [ReadOnly] public float randomTorque;
            
            [WriteOnly] public NativeArray<ForceData> forceDataArray;
            
            public void Execute(int index) {
                var floatingData = floatingDataArray[index];
                var point = floatingData.point;
                float3 force = default;
                
                var up = new float3(0f, 1f, 0f);
                int validProxyCount = 0;

#if UNITY_EDITOR
                float3 surfacePoint = default;
#endif
                
                for (int i = 0; i < volumeCount; i++) {
                    var volumeData = volumeDataArray[i];

                    var localPoint = math.mul(math.inverse(volumeData.rotation), point - volumeData.position);
                    var halfSize = volumeData.size * 0.5f;

                    if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                        localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                        localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                    {
                        continue;
                    }
                    
                    var sn = math.mul(volumeData.rotation, up);
                    var sc = volumeData.position + sn * (halfSize.y + volumeData.surfaceOffset);
                    var sp = point + math.project(sc - point, sn);
                    
#if UNITY_EDITOR
                    surfacePoint += sp;
#endif
                    
                    // Point is above the surface
                    if (math.dot(sp - point, sn) <= 0f) continue;
                    
                    validProxyCount++;
                    
                    float forceMul = forceLevel > 0f ? math.clamp(math.distance(sp, point) / forceLevel, 0f, 1f) : 1f;
                    force += volumeData.forceMul * forceMul * sn;
                }

                if (validProxyCount <= 0) {
                    forceDataArray[index] = default;
                    return;
                }

                force = (1f + floatingData.buoyancy) * force / validProxyCount 
                        - floatingData.deceleration * floatingData.velocity 
                        + GetNoiseVector(randomForceT, NoiseOffset * index) * randomForce;

                var torque = - floatingData.angularVelocity * torqueDeceleration 
                             + GetNoiseVector(randomTorqueT, NoiseOffset * 5f * index) * randomTorque;
                
                forceDataArray[index] = new ForceData(
                    floatingData.rbId,
                    floatingData.rbIndex,
                    floatingData.point,
                    force,
                    torque 
#if UNITY_EDITOR
                    , surfacePoint / validProxyCount
                    , floatingData.drawGizmos
#endif
                );
            }
        }
        
        [BurstCompile]
        private struct CalculateValidFloatingPointsPerRbJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<FloatingPointsPerRbData> rbDataArray;
            [ReadOnly] public NativeArray<ForceData> forceDataArray;

            [WriteOnly] public NativeArray<float> forceMulArray;
            
            public void Execute(int index) {
                var data = rbDataArray[index];
                int validFloatingPointsCount = 0;
                
                for (int i = data.firstFloatingPointIndex; i < data.floatingPointCount; i++) {
                    if (forceDataArray[i].rbId != 0) validFloatingPointsCount++;
                }
                
                forceMulArray[index] = validFloatingPointsCount > 0 ? 1f / validFloatingPointsCount : 0f;
            }
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showNoClientGizmo;
#endif
    }
    
}