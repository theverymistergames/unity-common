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

        [Header("Proxy")]
        [SerializeField] private WaterZoneProxy[] _predefinedProxies;
        
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
        
        private readonly struct ProxyData {
            
            public readonly float3 position;
            public readonly quaternion rotation;
            public readonly float3 size;
            public readonly float surfaceOffset;
            public readonly float forceMul;

            public ProxyData(float3 position, quaternion rotation, float3 size, float surfaceOffset, float forceMul) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.surfaceOffset = surfaceOffset;
                this.forceMul = forceMul;
            }
        }
        
        private readonly struct FloatingData {
            
            public readonly int rbId;
            public readonly float3 point;
            public readonly float3 velocity;
            public readonly float3 angularVelocity;
            public readonly float buoyancy;
            public readonly float deceleration;
#if UNITY_EDITOR
            public readonly bool drawGizmos;
#endif
            
            public FloatingData(int rbId, float3 point, float3 velocity, float3 angularVelocity, float buoyancy, float deceleration
#if UNITY_EDITOR
                , bool drawGizmos
#endif            
            ) {
                this.rbId = rbId;
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
            public readonly float3 point;
            public readonly float3 force;
            public readonly float3 torque;
#if UNITY_EDITOR
            public readonly float3 surfacePoint;
            public readonly bool drawGizmos;
#endif
            
            public ForceData(int rbId, float3 point, float3 force, float3 torque
#if UNITY_EDITOR
                , float3 surfacePoint
                , bool drawGizmos
#endif
            ) {
                this.rbId = rbId;
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

        public HashSet<IWaterZoneProxy> WaterProxySet { get; } = new();
        private readonly Dictionary<int, int> _proxyVolumeIdToClusterVolumeIdMap = new();
        
        private const float NoiseOffset = 100f;

        private readonly Dictionary<int, int> _colliderToRbIdMap = new();
        private readonly Dictionary<int, int> _rbIdToColliderCountMap = new();

        private readonly List<(int id, Rigidbody rb)> _rbList = new();
        private readonly Dictionary<int, int> _rbIdToIndexMap = new();
        private readonly Dictionary<int, WaterClientData> _rbIdToWaterClientDataMap = new();
        private readonly Dictionary<int, int> _rbIdToProxyCountMap = new();

        private NativeArray<ProxyData> _proxyDataArray;
        private int _floatingPointsCount;
        
        private void OnEnable() {
            for (int i = 0; i < _predefinedProxies.Length; i++) {
                AddProxy(_predefinedProxies[i]);
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            for (int i = 0; i < _predefinedProxies.Length; i++) {
                RemoveProxy(_predefinedProxies[i]);
            }

            _proxyDataArray.Dispose();
        }

        private void OnDestroy() {
            _rbIdToWaterClientDataMap.Clear();
            _rbIdToIndexMap.Clear();
            _rbList.Clear();
            
            WaterProxySet.Clear();
            _proxyVolumeIdToClusterVolumeIdMap.Clear();
        }

        public void AddProxyCluster(IWaterZoneProxyCluster cluster) {
            int count = cluster.ProxyCount;
            int id = cluster.VolumeId;
            
            for (int i = 0; i < count; i++) {
                _proxyVolumeIdToClusterVolumeIdMap[cluster.GetVolumeId(i)] = id;
            }
        }

        public void RemoveProxyCluster(IWaterZoneProxyCluster cluster) {
            int count = cluster.ProxyCount;
            
            for (int i = 0; i < count; i++) {
                _proxyVolumeIdToClusterVolumeIdMap.Remove(cluster.GetVolumeId(i));
            }
        }

        public void AddProxy(IWaterZoneProxy proxy) {
            WaterProxySet.Add(proxy);
            proxy.BindZone(this);
        }

        public void RemoveProxy(IWaterZoneProxy proxy) {
            WaterProxySet.Remove(proxy);
            proxy.UnbindZone(this);
        }

        public int GetProxyVolumeId(IWaterZoneProxy proxy) {
            return _proxyVolumeIdToClusterVolumeIdMap.TryGetValue(proxy.VolumeId, out int clusterVolumeId) ? clusterVolumeId : proxy.VolumeId;
        }

        public void TriggerEnter(Collider collider, IWaterZoneProxy proxy) {
            if (collider.attachedRigidbody is not {} rb) return;
            
            int id = collider.GetInstanceID();
            int rbId = rb.GetInstanceID();
            
            if (!_colliderToRbIdMap.TryAdd(id, rbId)) return;
            
            var pos = collider.transform.position;
            proxy.SampleSurface(pos, out var surfacePoint, out var surfaceNormal);
            OnColliderEnter.Invoke(collider, proxy.GetClosestPoint(pos), surfacePoint, surfaceNormal);
            
            int oldCount = _rbIdToColliderCountMap.GetValueOrDefault(rbId);
            _rbIdToColliderCountMap[rbId] = oldCount + 1;

            if (oldCount <= 0) TriggerEnterRigidbody(rb);
        }

        public void TriggerExit(Collider collider, IWaterZoneProxy proxy) {
            if (collider == null) return;
            
            int id = collider.GetInstanceID();
            if (collider == null || !_colliderToRbIdMap.Remove(id, out int rbId)) return;

            var pos = collider.transform.position;
            proxy.SampleSurface(pos, out var surfacePoint, out var surfaceNormal);
            OnColliderExit.Invoke(collider, proxy.GetClosestPoint(pos), surfacePoint, surfaceNormal);
            
            int newCount = _rbIdToColliderCountMap.GetValueOrDefault(rbId) - 1;
            if (newCount > 0) {
                _rbIdToColliderCountMap[rbId] = newCount;
                return;
            }

            _rbIdToColliderCountMap.Remove(rbId);
            TriggerExitRigidbody(rbId);
        }

        private void TriggerEnterRigidbody(Rigidbody rigidbody) {
            int id = rigidbody.GetInstanceID();
            _rbIdToProxyCountMap[id] = _rbIdToProxyCountMap.GetValueOrDefault(id) + 1;
            
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
            int proxiesLeft = _rbIdToProxyCountMap[rbId] - 1;

            if (proxiesLeft > 0) _rbIdToProxyCountMap[rbId] = proxiesLeft;
            else _rbIdToProxyCountMap.Remove(rbId);
            
            if (proxiesLeft > 0 || !_rbIdToIndexMap.Remove(rbId, out int index)) return;

            _rbList[index] = default;
            
            if (_rbIdToWaterClientDataMap.Remove(rbId, out var data)) {
                _floatingPointsCount -= data.waterClient.FloatingPointCount;
            }
            else {
                _floatingPointsCount--;
            }
        }

        void IUpdate.OnUpdate(float dt) {
            var proxyDataArray = CreateProxyDataArray(out int proxyCount);
            var floatingDataArray = CreateFloatingDataArray(out int floatingPointsCount);
            var forceDataArray = new NativeArray<ForceData>(floatingPointsCount, Allocator.TempJob);
            var rbIdToFloatingPointsCountMap = new NativeHashMap<int, int>(_rbList.Count, Allocator.TempJob);

            float time = Time.time;
            
            var calculateForceJob = new CalculateForceJob {
                floatingDataArray = floatingDataArray,
                proxyDataArray = proxyDataArray,
                proxyCount = proxyCount,
                forceLevel = _forceLevelDecrease,
                forceDataArray = forceDataArray,
                torqueDeceleration = _torqueDeceleration,
                randomForce = _randomForce,
                randomForceT = time * _randomForceSpeed,
                randomTorque = _randomTorque,
                randomTorqueT = time * _randomTorqueSpeed,
            };

            var calculateValidFloatingPointsCountJob = new CalculateValidFloatingPointsPerRbJob {
                count = floatingPointsCount,
                rbIdToFloatingPointsCountMap = rbIdToFloatingPointsCountMap,
                forceDataArray = forceDataArray,
            };
            
            var calculateForceJobHandle = calculateForceJob.Schedule(floatingPointsCount, UnityJobsExt.BatchCount(floatingPointsCount));
            calculateValidFloatingPointsCountJob.Schedule(calculateForceJobHandle).Complete();
            
            proxyDataArray.Dispose();
            floatingDataArray.Dispose();

            for (int i = 0; i < forceDataArray.Length; i++) {
                var forceData = forceDataArray[i];
                if (forceData.rbId == 0) continue;
                
                int fpCount = rbIdToFloatingPointsCountMap[forceData.rbId];
                var rb = _rbList[_rbIdToIndexMap[forceData.rbId]].rb;

#if UNITY_EDITOR
                if (forceData.drawGizmos) DebugExt.DrawLine(forceData.point, forceData.surfacePoint, Color.white);
#endif
                
                float mul = 1f / fpCount;
                
                rb.AddForceAtPosition(mul * forceData.force, forceData.point, ForceMode.Acceleration);
                rb.AddTorque(mul * forceData.torque, ForceMode.Acceleration);
            
#if UNITY_EDITOR
                if (forceData.drawGizmos) DebugExt.DrawRay(forceData.point, forceData.force, Color.magenta);
#endif
            }

            rbIdToFloatingPointsCountMap.Dispose();
            forceDataArray.Dispose();
            
            if (_rbList.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private NativeArray<ProxyData> CreateProxyDataArray(out int count) {
            count = WaterProxySet.Count;
            
            int index = 0;
            var proxyDataArray = new NativeArray<ProxyData>(count, Allocator.TempJob);
            
            foreach (var proxy in WaterProxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                
                proxyDataArray[index++] = new ProxyData(
                    position,
                    rotation,
                    size,
                    surfaceOffset: _surfaceOffset + proxy.SurfaceOffset,
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

            return proxyDataArray;
        }

        private NativeArray<FloatingData> CreateFloatingDataArray(out int count) {
            int rbCount = _rbList.Count;
            int validRbCount = rbCount;
            int fpIndex = 0;
            
            var floatingDataArray = new NativeArray<FloatingData>(_floatingPointsCount, Allocator.TempJob);

            for (int i = _rbList.Count - 1; i >= 0; i--) {
                (int id, var rb) = _rbList[i];
                
                if (id != 0 && rb != null) {
                    if (rb.isKinematic) continue;

                    float3 velocity = rb.linearVelocity;
                    float3 angularVelocity = rb.angularVelocity;
                    
                    if (_rbIdToWaterClientDataMap.TryGetValue(id, out var data) && data.isMainRigidbody) {
                        if (data.waterClient.IgnoreWaterZone) continue;

                        int floatingPointCount = data.waterClient.FloatingPointCount;
                        float buoyancy = data.waterClient.Buoyancy;
                        float decelerationMul = data.waterClient.DecelerationMul * _deceleration;
                        
                        for (int j = 0; j < floatingPointCount; j++) {
                            floatingDataArray[fpIndex++] = new FloatingData(
                                id,
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
                        
                        continue;
                    }
                    
                    floatingDataArray[fpIndex++] = new FloatingData(
                        id,
                        rb.position,
                        velocity,
                        angularVelocity,
                        _buoyancyDefault,
                        _deceleration
#if UNITY_EDITOR
                        , _showNoClientGizmo
#endif
                    );
                    
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
            count = fpIndex;

            return floatingDataArray;
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
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public int proxyCount;
            [ReadOnly] public float forceLevel;
            [ReadOnly] public float torqueDeceleration;
            
            [ReadOnly] public float randomForceT;
            [ReadOnly] public float randomForce;
            [ReadOnly] public float randomTorqueT;
            [ReadOnly] public float randomTorque;
            
            public NativeArray<ForceData> forceDataArray;
            
            public void Execute(int index) {
                var floatingData = floatingDataArray[index];
                var point = floatingData.point;
                float3 force = default;
                
                var up = new float3(0f, 1f, 0f);
                int validProxyCount = 0;

#if UNITY_EDITOR
                float3 surfacePoint = default;
#endif
                
                for (int i = 0; i < proxyCount; i++) {
                    var proxyData = proxyDataArray[i];

                    var localPoint = math.mul(math.inverse(proxyData.rotation), point - proxyData.position);
                    var halfSize = proxyData.size * 0.5f;

                    if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                        localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                        localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                    {
                        continue;
                    }
                    
                    var sn = math.mul(proxyData.rotation, up);
                    var sc = proxyData.position + sn * (halfSize.y + proxyData.surfaceOffset);
                    var sp = point + math.project(sc - point, sn);
                    
#if UNITY_EDITOR
                    surfacePoint += sp;
#endif
                    
                    // Point is above the surface
                    if (math.dot(sp - point, sn) <= 0f) continue;
                    
                    validProxyCount++;
                    
                    float forceMul = forceLevel > 0f ? math.clamp(math.distance(sp, point) / forceLevel, 0f, 1f) : 1f;
                    force += proxyData.forceMul * forceMul * sn;
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
        private struct CalculateValidFloatingPointsPerRbJob : IJob {
            
            [ReadOnly] public NativeArray<ForceData> forceDataArray;
            [ReadOnly] public int count;

            public NativeHashMap<int, int> rbIdToFloatingPointsCountMap;
            
            public void Execute() {
                for (int i = 0; i < count; i++) {
                    var forceData = forceDataArray[i];
                    if (forceData.rbId == 0) continue;
                    
                    rbIdToFloatingPointsCountMap[forceData.rbId] = 
                        (rbIdToFloatingPointsCountMap.TryGetValue(forceData.rbId, out int count) ? count : 0) + 1;
                }
            }
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showNoClientGizmo;
#endif
    }
    
}