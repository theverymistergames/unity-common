using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZone : MonoBehaviour, IActorComponent, IUpdate, IWaterZone {

        [Header("Proxy")]
        [SerializeField] private WaterZoneProxy[] _predefinedProxies;
        
        [Header("Surface")]
        [SerializeField] private float _surfaceOffset;
        [SerializeField] [Min(0f)] private float _forceLevelDecrease = 0f;
        
        [Header("Motion")]
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
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
        
        private struct ProxyData {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 size;
            public float forceMul;
            public float surfaceOffset;
        }
        
        private struct FloatingData {
            public int rbId;
            public Vector3 point;
            public float buoyancy;
            public float maxSpeed;
        }
        
        private struct ForceData {
            public int rbId;
            public Vector3 point;
            public Vector3 force;
            public float maxSpeed;
#if UNITY_EDITOR
            public Vector3 surfacePoint;
            public Vector3 surfaceNormal;
#endif
        }

        private const float NoiseOffset = 100f;
        
        private readonly HashSet<IWaterZoneProxy> _proxySet = new();
        
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
            _proxySet.Clear();
        }

        public void AddProxy(IWaterZoneProxy proxy) {
            _proxySet.Add(proxy);
            proxy.BindZone(this);
        }

        public void RemoveProxy(IWaterZoneProxy proxy) {
            _proxySet.Remove(proxy);
            proxy.UnbindZone(this);
        }

        public void TriggerEnter(Collider collider, IWaterZoneProxy proxy) {
            if (collider.attachedRigidbody is not {} rb) return;
            
            int id = collider.GetInstanceID();
            int rbId = rb.GetInstanceID();
            
            if (!_colliderToRbIdMap.TryAdd(id, rbId)) return;
            
            int oldCount = _rbIdToColliderCountMap.GetValueOrDefault(rbId);
            _rbIdToColliderCountMap[rbId] = oldCount + 1;

            if (oldCount <= 0) TriggerEnterRigidbody(rb);
        }

        public void TriggerExit(Collider collider, IWaterZoneProxy proxy) {
            if (collider == null) return;
            
            int id = collider.GetInstanceID();
            if (collider == null || !_colliderToRbIdMap.Remove(id, out int rbId)) return;
            
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

            if (!rigidbody.TryGetComponent(out IWaterClient waterClient)) {
                _floatingPointsCount++;
                return;
            }
            
            _rbIdToWaterClientDataMap[id] = new WaterClientData(
                isMainRigidbody: waterClient.Rigidbody == rigidbody,
                waterClient
            );
            
            _floatingPointsCount += waterClient.FloatingPointCount;
            
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
            
            var calculateForceJob = new CalculateForceJob {
                floatingDataArray = floatingDataArray,
                proxyDataArray = proxyDataArray,
                proxyCount = proxyCount,
                forceLevel = _forceLevelDecrease,
                forceDataArray = forceDataArray,
            };

            var calculateValidFloatingPointsCountJob = new CalculateValidFloatingPointsPerRbJob {
                count = floatingPointsCount,
                rbIdToFloatingPointsCountMap = rbIdToFloatingPointsCountMap,
                forceDataArray = forceDataArray,
            };
            
            var calculateForceJobHandle = calculateForceJob.Schedule(floatingPointsCount, innerloopBatchCount: 256);
            calculateValidFloatingPointsCountJob.Schedule(calculateForceJobHandle).Complete();
            
            proxyDataArray.Dispose();
            floatingDataArray.Dispose();

            for (int i = 0; i < forceDataArray.Length; i++) {
                var forceData = forceDataArray[i];
                if (forceData.rbId == 0) continue;
                
                int fpCount = rbIdToFloatingPointsCountMap[forceData.rbId];
                var rb = _rbList[_rbIdToIndexMap[forceData.rbId]].rb;

#if UNITY_EDITOR
                if (_showDebugInfo) DrawFloatingPoint(forceData.point, forceData.surfacePoint);
#endif
                
                ProcessRigidbody(rb, forceData.point, forceData.force, forceData.maxSpeed, mul: 1f / fpCount, i);
            }

            rbIdToFloatingPointsCountMap.Dispose();
            forceDataArray.Dispose();
            
            if (_rbList.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private NativeArray<ProxyData> CreateProxyDataArray(out int count) {
            count = _proxySet.Count;
            
            int index = 0;
            var proxyDataArray = new NativeArray<ProxyData>(count, Allocator.TempJob);
            
            foreach (var proxy in _proxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                
                proxyDataArray[index++] = new ProxyData {
                    position = position,
                    rotation = rotation,
                    size = size,
                    surfaceOffset = _surfaceOffset + proxy.SurfaceOffset,
                    forceMul = _force * _forceSource switch {
                        ForceSource.Constant => 1f,
                        ForceSource.UseGravityMagnitude => 
                            CustomGravity.Main.TryGetGlobalGravity(position, out var g) 
                                ? g.magnitude 
                                : Physics.gravity.magnitude,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                };
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
                    
                    if (_rbIdToWaterClientDataMap.TryGetValue(id, out var data) && data.isMainRigidbody) {
                        if (data.waterClient.IgnoreWaterZone) continue;

                        int floatingPointCount = data.waterClient.FloatingPointCount;
                        for (int j = 0; j < floatingPointCount; j++) {
                            floatingDataArray[fpIndex++] = new FloatingData {
                                rbId = id,
                                point = data.waterClient.GetFloatingPoint(j),
                                buoyancy = data.waterClient.Buoyancy,
                                maxSpeed = data.waterClient.MaxSpeed,
                            };
                        }
                        
                        continue;
                    }
                    
                    floatingDataArray[fpIndex++] = new FloatingData {
                        rbId = id,
                        point = rb.position,
                        buoyancy = _buoyancyDefault,
                        maxSpeed = _maxSpeed,
                    };
                    
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

        private void ProcessRigidbody(Rigidbody rb, Vector3 position, Vector3 force, float maxSpeed, float mul, int index) {
            var velocity = rb.linearVelocity;
            var angularVelocity = rb.angularVelocity;

            var forceVector = force - _deceleration * velocity;
            var torqueVector = -_torqueDeceleration * angularVelocity;

            var randomForce = GetNoiseVector(_randomForceSpeed, NoiseOffset * index) * _randomForce;
            var randomTorque = GetNoiseVector(_randomTorqueSpeed, NoiseOffset * 5f * index) * _randomTorque;
            
            rb.AddForceAtPosition(mul * (forceVector + randomForce), position, ForceMode.Acceleration);
            rb.AddTorque(mul * (torqueVector + randomTorque), ForceMode.Acceleration);
            
            if (maxSpeed >= 0f) rb.linearVelocity = VectorUtils.ClampVelocity(velocity, velocity, _maxSpeed);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(position, forceVector, Color.magenta);
#endif
        }

        private static Vector3 GetNoiseVector(float speed, float offset) {
            float t = Time.time * speed;
            return new Vector3(
                Mathf.PerlinNoise1D(t + offset) - 0.5f,
                Mathf.PerlinNoise1D(t + 7f * offset) - 0.5f,
                Mathf.PerlinNoise1D(t + 11f * offset) - 0.5f
            );
        }
        
        private struct CalculateForceJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<FloatingData> floatingDataArray;
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public int proxyCount;
            [ReadOnly] public float forceLevel;
            
            public NativeArray<ForceData> forceDataArray;
            
            public void Execute(int index) {
                var floatingData = floatingDataArray[index];
                var point = floatingData.point;
                var force = Vector3.zero;
                
                var up = Vector3.up;
                int validProxyCount = 0;

#if UNITY_EDITOR
                Vector3 surfacePoint = default;
                Vector3 surfaceNormal = default;
#endif
                
                for (int i = 0; i < proxyCount; i++) {
                    var proxyData = proxyDataArray[i];

                    var localPoint = Quaternion.Inverse(proxyData.rotation) * (point - proxyData.position);
                    var halfSize = proxyData.size * 0.5f;

                    if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                        localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                        localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                    {
                        continue;
                    }
                    
                    var sn = proxyData.rotation * up;
                    var sc = proxyData.position + sn * (halfSize.y + proxyData.surfaceOffset);
                    var sp = point + Vector3.Project(sc - point, sn);
                    
#if UNITY_EDITOR
                    surfacePoint += sp;
                    surfaceNormal += sn;
#endif
                    
                    // Point is above the surface
                    if (Vector3.Dot(sp - point, sn) <= 0f) continue;
                    
                    validProxyCount++;
                    
                    float forceMul = forceLevel > 0f ? Mathf.Clamp01((sp - point).magnitude / forceLevel) : 1f;
                    force += proxyData.forceMul * forceMul * sn;
                }

                if (validProxyCount <= 0) {
                    forceDataArray[index] = default;
                    return;
                }
                
                forceDataArray[index] = new ForceData {
                    rbId = floatingData.rbId,
                    point = floatingData.point,
                    force = (1f + floatingData.buoyancy) * force / validProxyCount,
                    maxSpeed = floatingData.maxSpeed,
#if UNITY_EDITOR
                    surfacePoint = surfacePoint / validProxyCount,
                    surfaceNormal = (surfaceNormal / validProxyCount).normalized,
#endif
                };
            }
        }
        
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
        [SerializeField] private bool _showDebugInfo;

        private static void DrawFloatingPoint(Vector3 position, Vector3 surfacePoint) {
            DebugExt.DrawSphere(position, 0.05f, Color.yellow);
            DebugExt.DrawLine(position, surfacePoint, Color.white);
            DebugExt.DrawSphere(surfacePoint, 0.02f, Color.white);
        }
#endif
    }
    
}