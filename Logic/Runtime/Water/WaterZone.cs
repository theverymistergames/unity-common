using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WaterZone : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private BoxCollider _waterBox;
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        
        [Header("Surface")]
        [SerializeField] private float _surfaceOffset;
        [SerializeField] [Min(0f)] private float _forceLevelDecrease = 0f;
        
        [Header("Force")]
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
        [SerializeField] private float _buoyancyDefault = 0f;
        [SerializeField] private float _deceleration = 0f;
        [SerializeField] private float _torqueDeceleration = 0f;
        [SerializeField] private ForceSource _forceSource;
        [SerializeField] private float _force = 0f;
        [VisibleIf(nameof(_forceSource), 1)]
        [SerializeField] private GravityProvider _gravityProvider;
        
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

            public readonly IWaterClient waterClient;
            public readonly bool isMainRigidbody;
            
            public WaterClientData(IWaterClient waterClient, bool isMainRigidbody) {
                this.waterClient = waterClient;
                this.isMainRigidbody = isMainRigidbody;
            }
        }

        private const float NoiseOffset = 100f;
        
        private readonly Dictionary<int, WaterClientData> _rbWaterClientDataMap = new();
        private readonly Dictionary<int, int> _rbIndexMap = new();
        private readonly List<Rigidbody> _rbList = new();
        
        private Transform _waterBoxTransform;

        private void Awake() {
            _waterBoxTransform = _waterBox.transform;
        }

        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;

            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void OnDestroy() {
            _rbWaterClientDataMap.Clear();
            _rbIndexMap.Clear();
            _rbList.Clear();
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            int id = rigidbody.GetInstanceID();
            if (!_rbIndexMap.TryAdd(id, _rbList.Count)) return;
            
            _rbList.Add(rigidbody);

            if (rigidbody.TryGetComponent(out IWaterClient waterClient)) {
                _rbWaterClientDataMap[id] = new WaterClientData(
                    waterClient,
                    isMainRigidbody: waterClient.Rigidbody.GetInstanceID() == id
                );
            }
        }

        private void TriggerExit(Rigidbody rigidbody) {
            int id = rigidbody.GetInstanceID();
            if (!_rbIndexMap.Remove(id, out int index)) return;

            _rbList[index] = null;
            _rbWaterClientDataMap.Remove(id);
        }

        void IUpdate.OnUpdate(float dt) {
            int count = _rbList.Count;
            int validCount = count;
            var up = GetWaterBoxUp(); 
            
            for (int i = _rbList.Count - 1; i >= 0; i--) {
                var rb = _rbList[i];
                
                if (rb != null) {
                    if (rb.isKinematic) continue;
            
                    if (_rbWaterClientDataMap.TryGetValue(rb.GetInstanceID(), out var data)) {
                        ProcessWaterClient(rb, data, up, i);
                        continue;
                    }
            
                    ProcessRigidbody(rb, rb.position, up, _buoyancyDefault, _maxSpeed, surfaceOffset: 0f, mul: 1f, i);
                    continue;
                }

                // Swap deleted with last valid and update index
                rb = _rbList[--validCount];
                if (rb != null) _rbIndexMap[rb.GetInstanceID()] = i;
                
                _rbList[i] = rb;
                _rbList[validCount] = null;
            }
            
            _rbList.RemoveRange(validCount, count - validCount);
        }

        private void ProcessWaterClient(Rigidbody rb, WaterClientData data, Vector3 waterUp, int index) {
            if (data.waterClient.IgnoreWaterZone) return;

            var client = data.waterClient;
            
            if (data.isMainRigidbody) {
                var surfacePoint = GetSurfacePoint(_surfaceOffset + client.SurfaceOffset);
                int count = client.FloatingPointCount;
                int belowSurfaceCount = 0;
                
                for (int i = 0; i < count; i++) {
                    var point = client.GetFloatingPoint(i);
                    if (Vector3.Dot(surfacePoint - point, waterUp) <= 0f) continue;
                    
                    belowSurfaceCount++;
                }

                if (belowSurfaceCount > 0) {
                    float mul = 1f / belowSurfaceCount;
                    for (int i = 0; i < count; i++) {
                        var point = client.GetFloatingPoint(i);
                        if (Vector3.Dot(surfacePoint - point, waterUp) <= 0f) return;
                        
                        ProcessRigidbody(rb, point, waterUp, client.Buoyancy, client.MaxSpeed, client.SurfaceOffset, mul, index);
                    }
                }
                
                return;
            }

            ProcessRigidbody(rb, rb.position, waterUp, client.Buoyancy, client.MaxSpeed, client.SurfaceOffset, mul: 1f, index);
        }

        private void ProcessRigidbody(
            Rigidbody rb,
            Vector3 position,
            Vector3 waterUp,
            float buoyancy,
            float maxSpeed,
            float surfaceOffset,
            float mul,
            int index) 
        {
            var surfacePoint = GetSurfacePoint(_surfaceOffset + surfaceOffset);

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(position, 0.05f, Color.yellow);
            if (_showDebugInfo) DebugExt.DrawRay(position, Vector3.Project(surfacePoint - position, waterUp), Color.white);
            if (_showDebugInfo) DebugExt.DrawSphere(position + Vector3.Project(surfacePoint - position, waterUp), 0.02f, Color.white);
#endif
            
            // Floating point is above the surface
            if (Vector3.Dot(surfacePoint - position, waterUp) <= 0f) return;

            var velocity = rb.linearVelocity;
            var angularVelocity = rb.angularVelocity;

            float distToSurface = Vector3.Project(surfacePoint - position, waterUp).magnitude;
            float forceMul = _forceLevelDecrease > 0f ? Mathf.Clamp01(distToSurface / _forceLevelDecrease) : 1f;
            
            float force = _forceSource switch {
                ForceSource.Constant => _force,
                ForceSource.UseGravityMagnitude => _force * _gravityProvider.GravityMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var forceVector = (1f + buoyancy) * force * forceMul * waterUp - _deceleration * velocity;
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

        private Vector3 GetSurfacePoint(float offset) {
            return _waterBox.bounds.center + 
                   _waterBoxTransform.up * (0.5f * _waterBox.size.y * _waterBoxTransform.localScale.y + offset);
        }

        private Vector3 GetWaterBoxUp() {
            return _waterBoxTransform.up;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _waterBox == null) return;

            if (_waterBoxTransform == null || _waterBoxTransform != _waterBox.transform) {
                _waterBoxTransform = _waterBox.transform;
            }

            var center = _waterBox.bounds.center;
            var surfacePoint = GetSurfacePoint(_surfaceOffset);
            var forceLevel = GetSurfacePoint(_surfaceOffset - _forceLevelDecrease);

            var right = _waterBoxTransform.right;
            var forward = _waterBoxTransform.forward;
            
            DebugExt.DrawSphere(center, 0.03f, Color.white, gizmo: true);
            DebugExt.DrawLine(center, surfacePoint, Color.white, gizmo: true);
            
            DebugExt.DrawSphere(surfacePoint, 0.04f, Color.cyan, gizmo: true);
            DebugExt.DrawLine(surfacePoint - right * 0.4f, surfacePoint + right * 0.4f, Color.cyan, gizmo: true);
            DebugExt.DrawLine(surfacePoint - forward * 0.4f, surfacePoint + forward * 0.4f, Color.cyan, gizmo: true);
            
            DebugExt.DrawLine(forceLevel - right * 0.4f, forceLevel + right * 0.4f, Color.magenta, gizmo: true);
            DebugExt.DrawLine(forceLevel - forward * 0.4f, forceLevel + forward * 0.4f, Color.magenta, gizmo: true);
        }

        private void Reset() {
            _waterBox = GetComponent<BoxCollider>();
        }
#endif
    }
    
}