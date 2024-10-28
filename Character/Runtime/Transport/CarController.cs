using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Transport {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarController : MonoBehaviour, IActorComponent {

        [Header("Inputs")]
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionVector1 _brake;
        [SerializeField] private InputActionKey _nitro;
        [SerializeField] private Vector2 _inputSmoothing = new Vector2(10f, 10f);
        [SerializeField] private Vector2 _inputSmoothingRelease = new Vector2(10f, 10f);

        [Header("Startup")]
        [SerializeField] private IgnitionMode _ignitionMode;
        [SerializeField] [Min(0f)] private float _ignitionDelayAfterEnter = 0.15f;
        [SerializeField] [Min(0f)] private float _ignitionDuration = 0.6f;
        
        [Header("Mass")]
        [SerializeField] private float _mass = 100f;
        [SerializeField] private float _wheelMass = 10f;
        [SerializeField] private Vector3 _centerOfMass;
        [SerializeField] private float _forcesScale = 1f;
        
        [Header("Acceleration")]
        [SerializeField] private DriveMode _driveMode;
        [SerializeField] [Min(0f)] private float _acceleration = 30f;
        [SerializeField] [Min(0f)] private float _inputThreshold = 0.1f;
        [SerializeField] [Min(0f)] private float _nitroAcceleration = 30f;
        [SerializeField] [Min(0f)] private float _speedToRpm = 0.1f;
        [SerializeField] [Min(0f)] private float _rpmMin = 0.6f;
        [SerializeField] [Min(0f)] private float _rpmMax = 6f;
        [SerializeField] [Min(0f)] private float _rpmUpSmoothing = 8f;
        [SerializeField] [Min(0f)] private float _rpmUpNotGroundedSmoothing = 3f;
        [SerializeField] [Min(0f)] private float _rpmDownSmoothing = 0.7f;
        
        [Header("Brakes")]
        [SerializeField] private DriveMode _brakeMode;
        [SerializeField] private DriveMode _driftMode;
        [SerializeField] [Min(0f)] private float _brakeAcceleration = 50f;
        [SerializeField] [Min(0f)] private float _brakeSideExtremumSlip = 0.4f;
        [SerializeField] [Min(0f)] private float _brakeForwardExtremumSlip = 0.4f;
        [SerializeField] [Min(0f)] private float _brakeSmoothing = 15f;
        
        [Header("Steering")]
        [SerializeField] [Range(0f, 180f)] private float _steerMaxAngle = 45f;
        [SerializeField] [Min(0f)] private float _steerSmoothing = 1f;
        
        [Header("Overturn")]
        [SerializeField] private Vector3 _overturnForceOffset;
        [SerializeField] private float _overturnForce;
        [SerializeField] [Range(0f, 1f)] private float _overturnAngleMin = 80f;
        [SerializeField] [Min(0f)] private float _minTimeOverturnedToApplyForce = 0.5f;
        
        [Header("Wheels")]
        [SerializeField] private float _sideOffset;
        [SerializeField] private WheelData[] _wheels;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private enum DriveMode {
            Forward,
            Rear,
            Full
        }
        
        private enum Axel {
            Front,
            Rear,
        }

        private enum IgnitionMode {
            OnAcceleration,
            OnEnter,
        }

        [Serializable]
        private struct WheelData {
            public Axel axel;
            public Transform geo;
            public WheelCollider collider;
            [NonSerialized] public float forwardExtremumSlip;
            [NonSerialized] public float sideExtremumSlip;
        }

        public event Action OnEnter = delegate { };
        public event Action OnExit = delegate { };
        
        /// <summary>
        /// With ignition duration as a parameter. 
        /// </summary>
        public event Action<float> OnStartIgnition = delegate { };
        
        /// <summary>
        /// With boolean 'is ignition active' as a parameter.
        /// </summary>
        public event Action<bool> OnIgnition = delegate { };
        
        /// <summary>
        /// With boolean 'is brake active' as a parameter.
        /// </summary>
        public event Action<bool> OnBrake = delegate { };

        public Transform Root { get; private set; }
        public bool IsBrakeOn { get; private set; }
        public bool IsIgnitionOn { get; private set; }
        public bool IsEntered { get; private set; }
        public float Speed { get; private set; }
        public float Rpm { get; private set; }
        public float BrakeForce { get; private set; }
        public bool AreBrakeWheelsGrounded { get; private set; }
        public bool AreDriveWheelsGrounded { get; private set; }

        private readonly Dictionary<WheelCollider, int> _wheelIndexMap = new();
        private IActor _actor;
        private Rigidbody _rigidbody;
        private Quaternion[] _wheelRotations;
        private Vector2 _input;
        private float _lastTimeNotOverturned;
        private float _ignitionStartTime;
        private float _enterTime;
        private bool _isIgnitionStartRequested;

        public bool IsWheelBrakeActive(WheelCollider wheelCollider) {
            return IsBrakeOn && 
                   _wheelIndexMap.TryGetValue(wheelCollider, out int i) && 
                   i >= 0 && i < _wheels.Length && 
                   IsMatch(_wheels[i].axel, _brakeMode);
        }
        
        void IActorComponent.OnAwake(IActor actor) {
            _rigidbody = actor.GetComponent<Rigidbody>();
            Root = _rigidbody.transform;
            
            InitializeMass();
            InitializeWheels();
            AnimateWheels();
        }
        
        private void OnEnable() {
            _enterTime = Time.time;
            IsEntered = true;
            OnEnter.Invoke();
        }

        private void OnDisable() {
            EnableIgnition(false);
            EnableBrakes(false);
            
            IsEntered = false;
            OnExit.Invoke();
            
            ResetWheelForces();
        }

        private void InitializeMass() {
            _rigidbody.mass = _mass;
            _rigidbody.centerOfMass = Vector3.Scale(_centerOfMass, Root.localScale);
        }
        
        private void InitializeWheels() {
            _wheelIndexMap.Clear();
            _wheelRotations = new Quaternion[_wheels.Length];
            
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                
                _wheelRotations[i] = wheel.geo.localRotation;
                
                var forwardFriction = wheel.collider.forwardFriction;
                var sidewaysFriction = wheel.collider.sidewaysFriction;
                
                wheel.forwardExtremumSlip = forwardFriction.extremumSlip;
                wheel.sideExtremumSlip = sidewaysFriction.extremumSlip;

                wheel.collider.mass = _wheelMass;
                
                _wheelIndexMap[wheel.collider] = i;
            }
        }

        private void Update() {
            AnimateWheels();
            EnableBrakes(!IsIgnitionOn || _brake.Value > 0f);
            CheckIgnition();
        }

        private void CheckIgnition() {
            if (IsIgnitionOn || Time.time < _enterTime + _ignitionDelayAfterEnter) return;

            switch (_ignitionMode) {
                case IgnitionMode.OnEnter:
                    StartIgnition(_ignitionDuration);
                    break;
                
                case IgnitionMode.OnAcceleration:
                    if (_move.Value.y.IsNearlyZero()) EnableIgnition(false, forceNotify: _isIgnitionStartRequested);
                    else StartIgnition(_ignitionDuration);
                    break;
            }

            if (!_isIgnitionStartRequested || Time.time < _ignitionStartTime + _ignitionDuration) return;

            EnableIgnition(true);
        }

        private void FixedUpdate() {
            float dt = Time.fixedDeltaTime;
            var targetInput = _move.Value;

            var smoothing = new Vector2(
                targetInput.x.IsNearlyZero() ? _inputSmoothingRelease.x : _inputSmoothing.x,
                targetInput.y.IsNearlyZero() ? _inputSmoothingRelease.y : _inputSmoothing.y
            );
            
            _input.x = Mathf.Lerp(_input.x, targetInput.x, dt * smoothing.x);
            _input.y = Mathf.Lerp(_input.y, targetInput.y, dt * smoothing.y);
            
            float brake = _brake.Value;
            float brakeForce = 0f;
            bool atLeastOneDriveWheelGrounded = false;
            bool atLeastOneBrakeWheelGrounded = false;
            
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                bool isGrounded = wheel.collider.isGrounded;
                
                Steer(ref wheel, _input.x, dt);
                Accelerate(ref wheel, _input.y, dt);
                Brake(ref wheel, brake, dt);
                
                brakeForce += wheel.collider.brakeTorque;

                if (IsMatch(wheel.axel, _driveMode)) atLeastOneDriveWheelGrounded |= isGrounded;
                if (IsMatch(wheel.axel, _brakeMode)) atLeastOneBrakeWheelGrounded |= isGrounded;
            }

            AreBrakeWheelsGrounded = atLeastOneBrakeWheelGrounded;
            AreDriveWheelsGrounded = atLeastOneDriveWheelGrounded;
            
            Speed = _rigidbody.linearVelocity.magnitude;
            BrakeForce = _wheels.Length > 0 ? brakeForce / _wheels.Length : 0f;
            
            ApplyAntiOverturn(_input.x);
            UpdateRpm(dt);
        }

        private void UpdateRpm(float dt) {
            bool accelerationPressed = !_move.Value.y.IsNearlyZero();
            
            float targetRpm = accelerationPressed
                ? AreDriveWheelsGrounded 
                    ? Mathf.Clamp(_rpmMin + _rigidbody.linearVelocity.magnitude * _speedToRpm, 0f, _rpmMax) 
                    : _rpmMax
                : _rpmMin;

            float f = accelerationPressed
                ? AreDriveWheelsGrounded ? _rpmUpSmoothing : _rpmUpNotGroundedSmoothing
                : _rpmDownSmoothing;
            
            Rpm = Mathf.Lerp(Rpm, targetRpm, f * dt);
        }

        private void StartIgnition(float duration) {
            if (_isIgnitionStartRequested) return;

            _ignitionStartTime = Time.time;
            _isIgnitionStartRequested = true;
            
            OnStartIgnition.Invoke(duration);
        }
        
        private void EnableBrakes(bool enabled) {
            bool notify = enabled != IsBrakeOn;
            IsBrakeOn = enabled;

            if (notify) OnBrake.Invoke(IsBrakeOn);
        }

        private void EnableIgnition(bool enabled, bool forceNotify = false) {
            bool notify = enabled != IsIgnitionOn;
            
            IsIgnitionOn = enabled;
            _isIgnitionStartRequested = false;

            if (forceNotify || notify) OnIgnition.Invoke(IsIgnitionOn);
        }

        private void ResetWheelForces() {
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];

                wheel.collider.motorTorque = 0f;
                wheel.collider.brakeTorque = 0f;
            }
        }
        
        private void Steer(ref WheelData wheel, float input, float dt) {
            if (wheel.axel == Axel.Rear) return;

            wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, input * _steerMaxAngle, _steerSmoothing * dt);
        }

        private void Accelerate(ref WheelData wheel, float input, float dt) {
            if (!IsIgnitionOn || !IsMatch(wheel.axel, _driveMode)) return;

            float acceleration = _nitro.IsPressed ? _nitroAcceleration : _acceleration;
            input = Mathf.Abs(input) < _inputThreshold ? 0f : input;
            
            wheel.collider.motorTorque = input * acceleration * _forcesScale * dt;
        }

        private void Brake(ref WheelData wheel, float brakeInput, float dt) {
            if (IsMatch(wheel.axel, _brakeMode)) {
                float brake = brakeInput * _brakeAcceleration * _forcesScale * dt;
                wheel.collider.brakeTorque = !brakeInput.IsNearlyZero() && _brakeSmoothing > 0f 
                    ? Mathf.Lerp(wheel.collider.brakeTorque, brake, _brakeSmoothing * dt)
                    : brake;
            }
            else {
                wheel.collider.brakeTorque = 0f;
            }
            
            var forwardFriction = wheel.collider.forwardFriction;
            var sidewaysFriction = wheel.collider.sidewaysFriction;
            
            if (IsMatch(wheel.axel, _driftMode)) {
                forwardFriction.extremumSlip = Mathf.Lerp(wheel.forwardExtremumSlip, _brakeForwardExtremumSlip, brakeInput);
                sidewaysFriction.extremumSlip = Mathf.Lerp(wheel.sideExtremumSlip, _brakeSideExtremumSlip, brakeInput);
            }
            else {
                forwardFriction.extremumSlip = wheel.forwardExtremumSlip;
                sidewaysFriction.extremumSlip = wheel.sideExtremumSlip;
            }
            
            wheel.collider.forwardFriction = forwardFriction;
            wheel.collider.sidewaysFriction = sidewaysFriction;   
        }
        
        private static bool IsMatch(Axel axel, DriveMode mode) {
            return axel == Axel.Front && mode != DriveMode.Rear ||
                   axel == Axel.Rear && mode != DriveMode.Forward;
        }

        private void ApplyAntiOverturn(float input) {
            var up = Root.up;
            
            if (Vector3.Angle(up, Vector3.up) < _overturnAngleMin) {
                _lastTimeNotOverturned = Time.time;
                return;
            }

            if (Time.time < _lastTimeNotOverturned + _minTimeOverturnedToApplyForce) {
                return;
            }
            
            var pos = Root.position;
            var rot = Root.rotation;
            var offset = Vector3.Scale(_overturnForceOffset, Root.localScale);
            var force = _overturnForce * up;

            var p0 = pos + rot * offset * input;
            var p1 = pos - rot * offset * input;

            var f0 = Mathf.Abs(input) * force;
            var f1 = -f0;
            
            _rigidbody.AddForceAtPosition(f0, p0, ForceMode.Acceleration);
            _rigidbody.AddForceAtPosition(f1, p1, ForceMode.Acceleration);
        }

        private void AnimateWheels() {
            var rootPos = _rigidbody.position;
            var right = _rigidbody.rotation * Vector3.right;
            
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                var rotationOffset = _wheelRotations[i];
                
                wheel.collider.GetWorldPose(out var pos, out var rot);

                rot *= Quaternion.Inverse(rotationOffset);
                float side = Mathf.Sign(Vector3.Dot(pos - rootPos, right));
                
                wheel.geo.SetPositionAndRotation(pos + rot * new Vector3(0f, side * _sideOffset, 0f), rot);
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            
            DebugExt.DrawSphere(_rigidbody.position, 0.05f, Color.green, gizmo: true);
            DebugExt.DrawSphere(_rigidbody.position + _rigidbody.centerOfMass, 0.03f, Color.cyan, gizmo: true);

            if (!Application.isPlaying) return;
            
            UnityEditor.Handles.Label(_rigidbody.position + Vector3.up, $"Ignition {(IsIgnitionOn ? "ON" : "OFF")}\n" +
                                                                        $"Speed {_rigidbody.linearVelocity.magnitude:0.00}\n" +
                                                                        $"RPM {Rpm:0.000}\n" +
                                                                        $"Brakes {(IsBrakeOn ? "ON" : "OFF")}, force {BrakeForce:0.00}");
        }
#endif
    }
    
}