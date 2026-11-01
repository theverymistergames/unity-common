using MisterGames.Actors;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Easing;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Transport {

    [RequireComponent(typeof(CarController))]
    public sealed class CarVibrationController : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Priority")]
        [SerializeField] private LabelValue _priority;

        [Header("Gas (right motor)")]
        [SerializeField] [Range(0f, 1f)] private float _gasForwardFrequency = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float _gasReverseFrequency = 0.15f;
        [SerializeField] private AnimationCurve _gasCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] [Min(0f)] private float _gasSmoothing = 5f;

        [Header("Landing (left motor)")]
        [SerializeField] [Min(0f)] private float _landingMinAirTime = 0.2f;
        [SerializeField] [Min(0f)] private float _landingSpeedMin = 1f;
        [SerializeField] [Min(0f)] private float _landingSpeedMax = 10f;
        [SerializeField] [Range(0f, 1f)] private float _landingFrequency = 1f;
        [SerializeField] [Min(0f)] private float _landingDuration = 0.3f;
        [SerializeField] private AnimationCurve _landingCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Collisions (both motors)")]
        [SerializeField] private CollisionEmitter _collisionEmitter;
        [SerializeField] [Min(0f)] private float _collisionSpeedMin = 1f;
        [SerializeField] [Min(0f)] private float _collisionSpeedMax = 10f;
        [SerializeField] [Range(0f, 1f)] private float _collisionFrequencyLeft = 1f;
        [SerializeField] [Range(0f, 1f)] private float _collisionFrequencyRight = 0.6f;
        [SerializeField] [Min(0f)] private float _collisionDuration = 0.25f;
        [SerializeField] private AnimationCurve _collisionCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Turns (left motor)")]
        [SerializeField] [Min(0f)] private float _turnAccelerationMin = 5f;
        [SerializeField] [Min(0f)] private float _turnAccelerationMax = 20f;
        [SerializeField] [Range(0f, 1f)] private float _turnFrequency = 0.5f;
        [SerializeField] private AnimationCurve _turnCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] [Min(0f)] private float _turnSmoothing = 5f;

        private IGamepadVibration _gamepadVibration;
        private CarController _carController;
        private Rigidbody _rigidbody;

        private float _gas;
        private float _turn;
        private float _landing;
        private float _landingProgress;
        private float _landingImpact;
        private float _collision;
        private float _collisionProgress;
        private float _collisionImpact;
        private float _airTime;
        private float _minVerticalSpeedInAir;
        private bool _wasGrounded;

        void IActorComponent.OnAwake(IActor actor) {
            _carController = GetComponent<CarController>();
            _rigidbody = actor.GetComponent<Rigidbody>();
        }

        private void Awake() {
            _gamepadVibration = Services.Get<IDeviceService>().GamepadVibration;
        }

        private void OnEnable() {
            _carController.OnEnter += OnEnter;
            _carController.OnExit += OnExit;

            if (_carController.IsEntered) OnEnter();
        }

        private void OnDisable() {
            _carController.OnEnter -= OnEnter;
            _carController.OnExit -= OnExit;

            OnExit();
        }

        private void OnEnter() {
            _gas = 0f;
            _turn = 0f;
            _landing = 0f;
            _landingProgress = 1f;
            _landingImpact = 0f;
            _collision = 0f;
            _collisionProgress = 1f;
            _collisionImpact = 0f;
            _airTime = 0f;
            _minVerticalSpeedInAir = 0f;
            _wasGrounded = true;

            if (_collisionEmitter != null) _collisionEmitter.CollisionEnter += OnCollision;

            _gamepadVibration.Register(this, _priority.GetValue());
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnExit() {
            if (_collisionEmitter != null) _collisionEmitter.CollisionEnter -= OnCollision;

            Services.Get<IDeviceService>()?.GamepadVibration.Unregister(this);
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            UpdateGas(dt);
            UpdateLanding(dt);
            UpdateTurn(dt);
            UpdateCollision(dt);

            var freq = new Vector2(
                Mathf.Max(_landing, _turn, _collision * _collisionFrequencyLeft),
                Mathf.Max(_gas, _collision * _collisionFrequencyRight)
            );
            _gamepadVibration.SetTwoMotors(this, freq);
        }

        private void UpdateGas(float dt) {
            float gas = _carController.GasInput;
            float reverse = _carController.ReverseInput;

            float target = gas >= reverse
                ? _gasCurve.Evaluate(gas) * _gasForwardFrequency
                : _gasCurve.Evaluate(reverse) * _gasReverseFrequency;

            _gas = _gas.SmoothExpNonZero(target, _gasSmoothing, dt);
        }

        private void UpdateLanding(float dt) {
            bool isGrounded = _carController.IsGrounded;

            if (!isGrounded) {
                if (_wasGrounded) {
                    _airTime = 0f;
                    _minVerticalSpeedInAir = 0f;
                }

                _airTime += dt;
                _minVerticalSpeedInAir = Mathf.Min(_minVerticalSpeedInAir, _rigidbody.linearVelocity.y);
            }
            else if (!_wasGrounded && _airTime >= _landingMinAirTime) {
                float impact = Mathf.InverseLerp(_landingSpeedMin, _landingSpeedMax, -_minVerticalSpeedInAir);

                if (impact > 0f) {
                    _landingProgress = 0f;
                    _landingImpact = impact;
                }
            }

            _wasGrounded = isGrounded;

            if (_landingProgress >= 1f) {
                _landing = 0f;
                return;
            }

            _landingProgress = _landingDuration > 0f ? Mathf.Clamp01(_landingProgress + dt / _landingDuration) : 1f;
            _landing = _landingCurve.Evaluate(_landingProgress) * _landingImpact * _landingFrequency;
        }

        private void OnCollision(Collision collision) {
            float impact = Mathf.InverseLerp(_collisionSpeedMin, _collisionSpeedMax, collision.relativeVelocity.magnitude);
            if (impact <= 0f || impact < _collision) return;

            _collisionProgress = 0f;
            _collisionImpact = impact;
        }

        private void UpdateCollision(float dt) {
            if (_collisionProgress >= 1f) {
                _collision = 0f;
                return;
            }

            _collisionProgress = _collisionDuration > 0f ? Mathf.Clamp01(_collisionProgress + dt / _collisionDuration) : 1f;
            _collision = _collisionCurve.Evaluate(_collisionProgress) * _collisionImpact;
        }

        private void UpdateTurn(float dt) {
            float target = 0f;

            if (_carController.IsGrounded) {
                var velocity = _rigidbody.linearVelocity;
                float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
                float lateralAcceleration = Mathf.Abs(_rigidbody.angularVelocity.y) * horizontalSpeed;

                float t = Mathf.InverseLerp(_turnAccelerationMin, _turnAccelerationMax, lateralAcceleration);
                target = _turnCurve.Evaluate(t) * _turnFrequency;
            }

            _turn = _turn.SmoothExpNonZero(target, _turnSmoothing, dt);
        }
    }

}
