using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WaterZone : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private BoxCollider _waterBox;
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        
        [Header("Surface")]
        [SerializeField] private float _surfaceOffset;
        
        [Header("Force")]
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
        [SerializeField] private float _distanceForce = 0f;
        [SerializeField] private float _constForce = 0f;
        [SerializeField] private float _torque = 0f;
        [SerializeField] private float _deceleration = 0f;
        [SerializeField] private float _torqueDeceleration = 0f;
        
        [Header("Random")]
        [SerializeField] private float _randomForce = 0f;
        [SerializeField] private float _randomTorque = 0f;
        [SerializeField] private float _randomForceSpeed = 0f;
        [SerializeField] private float _randomTorqueSpeed = 0f;
        
        private readonly struct WaterClientData {

            public readonly WaterClient waterClient;
            public readonly bool isMainRigidbody;
            
            public WaterClientData(WaterClient waterClient, bool isMainRigidbody) {
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

            if (rigidbody.TryGetComponent(out WaterClient waterClient)) {
                _rbWaterClientDataMap[id] = new WaterClientData(waterClient, isMainRigidbody: waterClient.Rigidbodies[0].GetInstanceID() == id);
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
            
                    ProcessRigidbody(rb, up, i);
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
            
            var surfacePoint = GetSurfacePoint(_surfaceOffset + data.waterClient.SurfaceOffset);
            var rbPos = rb.position;
            
            var velocity = rb.linearVelocity;
            var angularVelocity = rb.angularVelocity;
            
            var deceleration = _deceleration * data.waterClient.DecelerationMul * velocity;
            var torqueDeceleration = _torqueDeceleration * data.waterClient.TorqueDecelerationMul * angularVelocity;
            
            float distanceForce = _distanceForce * data.waterClient.ForceMul;
            float constForce = _constForce * data.waterClient.ForceMul;
            float torque = _torque * data.waterClient.TorqueMul;
            
            float maxSpeed = data.waterClient.MaxSpeed < -1f ? -_maxSpeed : data.waterClient.MaxSpeed;
            
            var randomForce = GetNoiseVector(_randomForceSpeed, NoiseOffset * index) * _randomForce;
            var randomTorque = GetNoiseVector(_randomTorqueSpeed, NoiseOffset * 5f * index) * _randomTorque;
            
            Vector3 vectorToSurface;
            Vector3 floatingForceVector;

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(rbPos, 0.05f, Color.yellow);
#endif
            
            if (data.isMainRigidbody) {
                for (int i = 0; i < data.waterClient.FloatingPoints.Count; i++) {
                    var floatingPoint = data.waterClient.FloatingPoints[i].position;
                     
#if UNITY_EDITOR
                    if (_showDebugInfo) DebugExt.DrawLine(rbPos, floatingPoint, Color.yellow);
                    if (_showDebugInfo) DebugExt.DrawSphere(floatingPoint, 0.03f, Color.yellow);
                    if (_showDebugInfo) DebugExt.DrawRay(floatingPoint, Vector3.Project(surfacePoint - floatingPoint, waterUp), Color.white);
                    if (_showDebugInfo) DebugExt.DrawSphere(floatingPoint + Vector3.Project(surfacePoint - floatingPoint, waterUp), 0.02f, Color.white);
#endif
                    
                    // Floating point is above the surface
                    if (Vector3.Dot(surfacePoint - floatingPoint, waterUp) <= 0f) continue;

                    vectorToSurface = Vector3.Project(surfacePoint - floatingPoint, waterUp);
                    floatingForceVector = (distanceForce * vectorToSurface.magnitude) * waterUp - deceleration;
                    var torqueVector = torque * Vector3.Cross(floatingPoint - rbPos, waterUp);
                    
                    rb.AddForceAtPosition(floatingForceVector, floatingPoint, ForceMode.Acceleration);
                    rb.AddTorque(torqueVector, ForceMode.Acceleration);
                    
#if UNITY_EDITOR
                    if (_showDebugInfo) DebugExt.DrawRay(floatingPoint, floatingForceVector, Color.magenta);
#endif
                }
                
                rb.AddForce(randomForce, ForceMode.Acceleration);
                rb.AddTorque(-torqueDeceleration + randomTorque, ForceMode.Acceleration);
                
                if (maxSpeed >= 0f) rb.linearVelocity = VectorUtils.ClampVelocity(velocity, velocity, maxSpeed);

                return;
            }

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(rbPos, Vector3.Project(surfacePoint - rbPos, waterUp), Color.white);
            if (_showDebugInfo) DebugExt.DrawSphere(rbPos + Vector3.Project(surfacePoint - rbPos, waterUp), 0.02f, Color.white);
#endif
            
            // Floating point is above the surface
            if (Vector3.Dot(surfacePoint - rbPos, waterUp) <= 0f) return;

            vectorToSurface = Vector3.Project(surfacePoint - rbPos, waterUp);
            floatingForceVector = distanceForce * vectorToSurface.magnitude * waterUp - deceleration;
            
            rb.AddForce(floatingForceVector + randomForce, ForceMode.Acceleration);
            rb.AddTorque(-torqueDeceleration + randomTorque, ForceMode.Acceleration);
            
            if (maxSpeed >= 0f) rb.linearVelocity = VectorUtils.ClampVelocity(velocity, velocity, maxSpeed);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(rbPos, floatingForceVector, Color.magenta);
#endif
        }

        private void ProcessRigidbody(Rigidbody rb, Vector3 waterUp, int index) {
            var surfacePoint = GetSurfacePoint(_surfaceOffset);
            var rbPos = rb.position;

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(rbPos, 0.05f, Color.yellow);
            if (_showDebugInfo) DebugExt.DrawRay(rbPos, Vector3.Project(surfacePoint - rbPos, waterUp), Color.white);
            if (_showDebugInfo) DebugExt.DrawSphere(rbPos + Vector3.Project(surfacePoint - rbPos, waterUp), 0.02f, Color.white);
#endif
            
            // Floating point is above the surface
            if (Vector3.Dot(surfacePoint - rbPos, waterUp) <= 0f) return;

            var velocity = rb.linearVelocity;
            var angularVelocity = rb.angularVelocity;
            
            var vectorToSurface = Vector3.Project(surfacePoint - rbPos, waterUp);
            var floatingForceVector = (_distanceForce * vectorToSurface.magnitude + _constForce) * waterUp - _deceleration * velocity;
            var torqueDeceleration = _torqueDeceleration * angularVelocity;

            var randomForce = GetNoiseVector(_randomForceSpeed, NoiseOffset * index) * _randomForce;
            var randomTorque = GetNoiseVector(_randomTorqueSpeed, NoiseOffset * 5f * index) * _randomTorque;
            
            rb.AddForce(floatingForceVector + randomForce, ForceMode.Acceleration);
            rb.AddTorque(-torqueDeceleration + randomTorque, ForceMode.Acceleration);
            
            if (_maxSpeed >= 0f) rb.linearVelocity = VectorUtils.ClampVelocity(velocity, velocity, _maxSpeed);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(rbPos, floatingForceVector, Color.magenta);
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
            var bounds = _waterBox.bounds;
            return bounds.center + _waterBoxTransform.up * (bounds.extents.y + offset);
        }
        
        private Vector3 GetWaterBoxCenter() {
            return _waterBox.bounds.center;
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

            var center = GetWaterBoxCenter();
            var surfacePoint = GetSurfacePoint(_surfaceOffset);

            var right = _waterBoxTransform.right;
            var forward = _waterBoxTransform.forward;
            
            DebugExt.DrawSphere(center, 0.03f, Color.white, gizmo: true);
            DebugExt.DrawLine(center, surfacePoint, Color.white, gizmo: true);
            DebugExt.DrawSphere(surfacePoint, 0.04f, Color.cyan, gizmo: true);
            DebugExt.DrawLine(surfacePoint - right * 0.4f, surfacePoint + right * 0.4f, Color.cyan, gizmo: true);
            DebugExt.DrawLine(surfacePoint - forward * 0.4f, surfacePoint + forward * 0.4f, Color.cyan, gizmo: true);
        }

        private void Reset() {
            _waterBox = GetComponent<BoxCollider>();
        }
#endif
    }
    
}