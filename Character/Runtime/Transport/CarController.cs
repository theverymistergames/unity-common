using System;
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

        [Header("Startup")]
        [SerializeField] private IgnitionMode _ignitionMode;
        [SerializeField] [Min(0f)] private float _ignitionDuration = 0.6f;
        
        [Header("Mass")]
        [SerializeField] private Vector3 _centerOfMass;
        
        [Header("Acceleration")]
        [SerializeField] private DriveMode _driveMode;
        [SerializeField] [Min(0f)] private float _acceleration = 30f;
        [SerializeField] [Min(0f)] private float _nitroAcceleration = 30f;
        [SerializeField] [Min(0f)] private float _speedToRpm = 0.1f;
        [SerializeField] [Min(0f)] private float _rpmMin = 0.6f;
        [SerializeField] [Min(0f)] private float _rpmMax = 6f;
        [SerializeField] [Min(0f)] private float _rpmDropSmoothing = 2f;
        
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

        public bool IsBrakeOn { get; private set; }
        public bool IsIgnitionOn { get; private set; }
        public bool IsEntered { get; private set; }
        public float Rpm { get; private set; }

        private IActor _actor;
        private Transform _transform;
        private Rigidbody _rigidbody;
        private Quaternion[] _wheelRotations;
        private Vector2 _input;
        private float _lastTimeNotOverturned;
        private float _ignitionStartTime;
        private bool _isIgnitionStartRequested;

        public void OnAwake(IActor actor) {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _transform = _rigidbody.transform;
            _rigidbody.centerOfMass = Vector3.Scale(_centerOfMass, _transform.localScale);
            
            FetchInitialWheelData();
        }

        private void OnEnable() {
            IsEntered = true;
            OnEnter.Invoke();

            if (_ignitionMode == IgnitionMode.OnEnter) StartIgnition(_ignitionDuration);
        }

        private void OnDisable() {
            EnableIgnition(false);
            EnableBrakes(false);
            
            IsEntered = false;
            OnExit.Invoke();
            
            ResetWheelForces();
        }

        private void Update() {
            AnimateWheels();
            EnableBrakes(!IsIgnitionOn || _brake.Value > 0f);
            CheckIgnition();
        }

        private void CheckIgnition() {
            if (IsIgnitionOn) return;

            if (_ignitionMode == IgnitionMode.OnAcceleration) {
                if (_move.Value.y.IsNearlyZero()) EnableIgnition(false, forceNotify: _isIgnitionStartRequested);
                else StartIgnition(_ignitionDuration);
            }
            
            if (!_isIgnitionStartRequested || Time.time < _ignitionStartTime + _ignitionDuration) return;

            EnableIgnition(true);
        }

        private void FixedUpdate() {
            float dt = Time.fixedDeltaTime;
            var targetInput = _move.Value;

            _input.x = Mathf.Lerp(_input.x, targetInput.x, dt * _inputSmoothing.x);
            _input.y = Mathf.Lerp(_input.y, targetInput.y, dt * _inputSmoothing.y);
            
            float brake = _brake.Value;
            
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                
                Steer(ref wheel, _input.x, dt);
                Accelerate(ref wheel, _input.y, dt);
                Brake(ref wheel, brake, dt);
            }
            
            ApplyAntiOverturn(_input.x);

            float targetRpm = Mathf.Clamp(_rpmMin + _rigidbody.velocity.magnitude * _speedToRpm, 0f, _rpmMax);

            Rpm = _move.Value.y.IsNearlyZero()
                ? Mathf.Lerp(Rpm, targetRpm, _rpmDropSmoothing * dt)
                : targetRpm;
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
            if (!IsIgnitionOn ||
                wheel.axel == Axel.Front && _driveMode == DriveMode.Rear || 
                wheel.axel == Axel.Rear && _driveMode == DriveMode.Forward) return;

            float acceleration = _nitro.IsPressed ? _nitroAcceleration : _acceleration;
            wheel.collider.motorTorque = input * acceleration * dt;
        }

        private void Brake(ref WheelData wheel, float input, float dt) {
            if (wheel.axel == Axel.Front && _brakeMode != DriveMode.Rear ||
                wheel.axel == Axel.Rear && _brakeMode != DriveMode.Forward
            ) {
                float brake = input * _brakeAcceleration * dt;
                wheel.collider.brakeTorque = input > 0f && _brakeSmoothing > 0f 
                    ? Mathf.Lerp(wheel.collider.brakeTorque, brake, _brakeSmoothing * dt)
                    : brake;
            }
            else {
                wheel.collider.brakeTorque = 0f;
            }
            
            var forwardFriction = wheel.collider.forwardFriction;
            var sidewaysFriction = wheel.collider.sidewaysFriction;
            
            if (wheel.axel == Axel.Front && _driftMode != DriveMode.Rear ||
                wheel.axel == Axel.Rear && _driftMode != DriveMode.Forward
            ) {
                forwardFriction.extremumSlip = Mathf.Lerp(wheel.forwardExtremumSlip, _brakeForwardExtremumSlip, input);
                sidewaysFriction.extremumSlip = Mathf.Lerp(wheel.sideExtremumSlip, _brakeSideExtremumSlip, input);
            }
            else {
                forwardFriction.extremumSlip = wheel.forwardExtremumSlip;
                sidewaysFriction.extremumSlip = wheel.sideExtremumSlip;
            }
            
            wheel.collider.forwardFriction = forwardFriction;
            wheel.collider.sidewaysFriction = sidewaysFriction;   
        }

        private void ApplyAntiOverturn(float input) {
            var up = _transform.up;
            
            if (Vector3.Angle(up, Vector3.up) < _overturnAngleMin) {
                _lastTimeNotOverturned = Time.time;
                return;
            }

            if (Time.time < _lastTimeNotOverturned + _minTimeOverturnedToApplyForce) {
                return;
            }
            
            var pos = _transform.position;
            var rot = _transform.rotation;
            var offset = Vector3.Scale(_overturnForceOffset, _transform.localScale);
            var force = _overturnForce * up;

            var p0 = pos + rot * offset * input;
            var p1 = pos - rot * offset * input;

            var f0 = Mathf.Abs(input) * force;
            var f1 = -f0;
            
            _rigidbody.AddForceAtPosition(f0, p0, ForceMode.Acceleration);
            _rigidbody.AddForceAtPosition(f1, p1, ForceMode.Acceleration);
        }

        private void AnimateWheels() {
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                var rotationOffset = _wheelRotations[i];
                
                wheel.collider.GetWorldPose(out var pos, out var rot);
                wheel.geo.SetPositionAndRotation(pos, rot * Quaternion.Inverse(rotationOffset));
            }
        }

        private void FetchInitialWheelData() {
            _wheelRotations = new Quaternion[_wheels.Length];
            for (int i = 0; i < _wheels.Length; i++) {
                ref var wheel = ref _wheels[i];
                
                _wheelRotations[i] = wheel.geo.localRotation;
                
                var forwardFriction = wheel.collider.forwardFriction;
                var sidewaysFriction = wheel.collider.sidewaysFriction;
                
                wheel.forwardExtremumSlip = forwardFriction.extremumSlip;
                wheel.sideExtremumSlip = sidewaysFriction.extremumSlip;
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            
            DebugExt.DrawSphere(_rigidbody.position, 0.05f, Color.green, gizmo: true);
            DebugExt.DrawSphere(_rigidbody.position + _rigidbody.centerOfMass, 0.03f, Color.cyan, gizmo: true);
        }
#endif
    }
    
}