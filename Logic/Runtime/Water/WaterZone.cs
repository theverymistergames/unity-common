using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    public sealed class WaterZone : MonoBehaviour, IActorComponent, IUpdate, IWaterZone {

        [Header("Proxy")]
        [SerializeField] private WaterZoneProxy[] _predefinedProxies;
        
        [Header("Force")]
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
        [SerializeField] private float _buoyancyDefault = 0f;
        [SerializeField] private float _deceleration = 0f;
        [SerializeField] private float _torqueDeceleration = 0f;
        
        [Header("Random")]
        [SerializeField] private float _randomForce = 0f;
        [SerializeField] private float _randomTorque = 0f;
        [SerializeField] private float _randomForceSpeed = 0f;
        [SerializeField] private float _randomTorqueSpeed = 0f;
        
        private readonly struct WaterClientData {

            public readonly IWaterClient waterClient;
            public readonly bool isMainRigidbody;
            
            public WaterClientData(IWaterClient waterClient, bool isMainRigidbody) {
                this.waterClient = waterClient;
                this.isMainRigidbody = isMainRigidbody;
            }
        }

        private const float NoiseOffset = 100f;
        
        private readonly MultiValueDictionary<Rigidbody, IWaterZoneProxy> _rbWaterProxyMap = new();
        
        private readonly Dictionary<Collider, Rigidbody> _colliderToRigidbodyMap = new();
        private readonly Dictionary<Rigidbody, int> _rigidbodyColliderCountMap = new();
        
        private readonly Dictionary<Rigidbody, WaterClientData> _rbWaterClientDataMap = new();
        private readonly Dictionary<Rigidbody, int> _rbIndexMap = new();
        private readonly List<Rigidbody> _rbList = new();

        private void OnEnable() {
            for (int i = 0; i < _predefinedProxies.Length; i++) {
                _predefinedProxies[i].BindZone(this);
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            for (int i = 0; i < _predefinedProxies.Length; i++) {
                _predefinedProxies[i].UnbindZone(this);
            }
        }

        private void OnDestroy() {
            _rbWaterClientDataMap.Clear();
            _rbIndexMap.Clear();
            _rbList.Clear();
        }

        public void TriggerEnter(Collider collider, IWaterZoneProxy proxy) {
            if (collider.attachedRigidbody is not {} rb || 
                !_colliderToRigidbodyMap.TryAdd(collider, rb)) 
            {
                return;
            }

            int oldCount = _rigidbodyColliderCountMap.GetValueOrDefault(rb);
            _rigidbodyColliderCountMap[rb] = oldCount + 1;

            if (oldCount <= 0) TriggerEnterRigidbody(rb, proxy);
        }

        public void TriggerExit(Collider collider, IWaterZoneProxy proxy) {
            if (!_colliderToRigidbodyMap.Remove(collider, out var rb)) {
                return;
            }
            
            int newCount = _rigidbodyColliderCountMap.GetValueOrDefault(rb) - 1;
            if (newCount > 0) {
                _rigidbodyColliderCountMap[rb] = newCount;
                return;
            }

            _rigidbodyColliderCountMap.Remove(rb);
            TriggerExitRigidbody(rb, proxy);
        }

        private void TriggerEnterRigidbody(Rigidbody rigidbody, IWaterZoneProxy proxy) {
            _rbWaterProxyMap.AddValue(rigidbody, proxy);
            
            if (!_rbIndexMap.TryAdd(rigidbody, _rbList.Count)) return;
            
            _rbList.Add(rigidbody);
            
            if (!rigidbody.TryGetComponent(out IWaterClient waterClient)) return;
            
            _rbWaterClientDataMap[rigidbody] = new WaterClientData(
                waterClient,
                isMainRigidbody: waterClient.Rigidbody == rigidbody
            );
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void TriggerExitRigidbody(Rigidbody rigidbody, IWaterZoneProxy proxy) {
            _rbWaterProxyMap.RemoveValue(rigidbody, proxy);
            int proxiesLeft = _rbWaterProxyMap.GetCount(rigidbody);
            
            if (proxiesLeft > 0 || !_rbIndexMap.Remove(rigidbody, out int index)) return;

            _rbList[index] = null;
            _rbWaterClientDataMap.Remove(rigidbody);
            
            if (_rbIndexMap.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            int count = _rbList.Count;
            
            for (int i = 0; i < count; i++) {
                var rb = _rbList[i];
                if (rb == null || rb.isKinematic) continue;
            
                if (_rbWaterClientDataMap.TryGetValue(rb, out var data)) {
                    ProcessWaterClient(rb, data, i);
                    continue;
                }

                int proxyCount = _rbWaterProxyMap.GetCount(rb);
                if (proxyCount <= 0) continue;
                
                var pos = rb.position;
                SampleSurface(rb, pos, proxyCount, surfaceOffset: 0f, out var surfacePoint, out var surfaceNormal, out var force);

#if UNITY_EDITOR
                DrawFloatingPoint(pos, surfacePoint, surfaceNormal);          
#endif
                
                // Floating point is above the surface
                if (Vector3.Dot(surfacePoint - pos, surfaceNormal) <= 0f) continue;
                
                ProcessRigidbody(rb, pos, force * (1f + _buoyancyDefault), _maxSpeed, i);
            }
            
            count = _rbList.Count;
            int validCount = count;

            for (int i = _rbList.Count - 1; i >= 0; i--) {
                var rb = _rbList[i];
                if (rb != null) continue;

                if (rb is {} notNull) _rbIndexMap.Remove(notNull);
                
                if (_rbList[--validCount] is { } swap && swap != null) 
                {
                    _rbList[i] = swap;
                    _rbIndexMap[swap] = i;
                }
            }

            _rbList.RemoveRange(validCount, count - validCount);
        }

        private void ProcessWaterClient(Rigidbody rb, WaterClientData data, int index) {
            if (data.waterClient.IgnoreWaterZone) return;

            int proxyCount = _rbWaterProxyMap.GetCount(rb);
            if (proxyCount <= 0) return;
            
            var client = data.waterClient;
            
            Vector3 pos = default;
            Vector3 force = default;

            if (data.isMainRigidbody) {
                int count = client.FloatingPointCount;
                int belowSurfaceCount = 0;
                
                for (int i = 0; i < count; i++) {
                    var point = client.GetFloatingPoint(i);
                    SampleSurface(rb, point, proxyCount, client.SurfaceOffset, out var p, out var n, out var f);
                    
#if UNITY_EDITOR
                    DrawFloatingPoint(point, p, n);          
#endif
                    
                    if (Vector3.Dot(p - point, n) <= 0f) continue;

                    belowSurfaceCount++;

                    pos += point;
                    force += f;
                }

                if (belowSurfaceCount > 0) {
                    ProcessRigidbody(rb, pos / belowSurfaceCount, force / belowSurfaceCount * (1f + client.Buoyancy), client.MaxSpeed, index);    
                }
                
                return;
            }

            pos = rb.position;
            SampleSurface(rb, pos, proxyCount, client.SurfaceOffset, out var surfacePoint, out var surfaceNormal, out force);
            
#if UNITY_EDITOR
            DrawFloatingPoint(pos, surfacePoint, surfaceNormal);          
#endif
            
            // Floating point is above the surface
            if (Vector3.Dot(surfacePoint - pos, surfaceNormal) <= 0f) return;
            
            ProcessRigidbody(rb, pos, force * (1f + client.Buoyancy), client.MaxSpeed, index);
        }

        private void ProcessRigidbody(Rigidbody rb, Vector3 position, Vector3 force, float maxSpeed, int index) {
            var velocity = rb.linearVelocity;
            var angularVelocity = rb.angularVelocity;

            var forceVector = force - _deceleration * velocity;
            var torqueVector = -_torqueDeceleration * angularVelocity;

            var randomForce = GetNoiseVector(_randomForceSpeed, NoiseOffset * index) * _randomForce;
            var randomTorque = GetNoiseVector(_randomTorqueSpeed, NoiseOffset * 5f * index) * _randomTorque;
            
            rb.AddForceAtPosition(forceVector + randomForce, position, ForceMode.Acceleration);
            rb.AddTorque(torqueVector + randomTorque, ForceMode.Acceleration);
            
            if (maxSpeed >= 0f) rb.linearVelocity = VectorUtils.ClampVelocity(velocity, velocity, _maxSpeed);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(position, forceVector, Color.magenta);
#endif
        }

        private void SampleSurface(
            Rigidbody rb,
            Vector3 position,
            int proxyCount,
            float surfaceOffset,
            out Vector3 surfacePoint,
            out Vector3 surfaceNormal,
            out Vector3 force) 
        {
            surfacePoint = default;
            surfaceNormal = default;
            force = default;

            for (int i = 0; i < proxyCount; i++) {
                _rbWaterProxyMap.GetValue(rb, i).SampleSurface(position, out var p, out var n, out var f);

                surfacePoint += p + n * surfaceOffset;
                surfaceNormal += n;
                force += f;
            }

            surfaceNormal = (surfaceNormal / proxyCount).normalized;
            surfacePoint /= proxyCount;
            force /= proxyCount;
        }

        private static Vector3 GetNoiseVector(float speed, float offset) {
            float t = Time.time * speed;
            return new Vector3(
                Mathf.PerlinNoise1D(t + offset) - 0.5f,
                Mathf.PerlinNoise1D(t + 7f * offset) - 0.5f,
                Mathf.PerlinNoise1D(t + 11f * offset) - 0.5f
            );
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void DrawFloatingPoint(Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal) {
            if (!_showDebugInfo) return;
            
            bool isBelowSurface = Vector3.Dot(surfacePoint - position, surfaceNormal) > 0f;
            
            DebugExt.DrawSphere(position, 0.05f, isBelowSurface ? Color.green : Color.red);
            DebugExt.DrawRay(position, Vector3.Project(surfacePoint - position, surfaceNormal), Color.white);
            DebugExt.DrawSphere(position + Vector3.Project(surfacePoint - position, surfaceNormal), 0.02f, Color.white);
        }
#endif
    }
    
}