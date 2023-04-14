using System;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.Input.Actions;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Objects {

    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveGrab : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Input")]
        [SerializeField] private InputActionVector2 _inputAxis;
        [SerializeField] private float _handSensitivity = 0.4f;
        [SerializeField] private float _walkSensitivity = 1f;
        [SerializeField] private float _smoothFactor = 5f;
        
        [Header("Grab plane")]
        [SerializeField] private NormalMode _normalMode = NormalMode.Up;
        [SerializeField] private bool _invertNormal = false;
        
        public event Action OnStartGrab = delegate {  }; 
        public event Action OnStopGrab = delegate {  }; 
        public event Action<Vector3, Vector3> OnGrab = delegate {  };

        private IInteractiveUser _user;
        private Interactive _interactive;

        private Transform _transform;

        private Vector3 _smoothedGrabPoint;
        private Vector3 _grabPoint;
        private Vector3 _inputDelta;
        private Vector3 _lastUserPosition;

        private bool _isGrabbed;

        private enum NormalMode {
            Up,
            Forward,
            Right
        }

        private void Awake() {
            _interactive = GetComponent<Interactive>();
            _transform = transform; 
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
            TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            if (!_isGrabbed) return;
            
            var prevPosition = _lastUserPosition;
            _lastUserPosition = _user.TransformAdapter.Position;

            var userPositionDelta = _walkSensitivity * (_lastUserPosition - prevPosition);
            
            _grabPoint += userPositionDelta + _inputDelta;
            _inputDelta = Vector3.zero;

            var prevSmoothedGrabPoint = _smoothedGrabPoint;
            _smoothedGrabPoint = Vector3.Lerp(_smoothedGrabPoint, _grabPoint, _smoothFactor * dt);
            
            OnGrab.Invoke(prevSmoothedGrabPoint, _smoothedGrabPoint);
        }

        private void OnStartInteract(IInteractiveUser user, Vector3 hitPoint) {
            _user = user;
            _lastUserPosition = _user.TransformAdapter.Position;
            
            _grabPoint = hitPoint;
            _smoothedGrabPoint = _grabPoint;
            
            _inputAxis.OnChanged -= OnInputAxisChanged;
            _inputAxis.OnChanged += OnInputAxisChanged;

            _isGrabbed = true;
            
            OnStartGrab.Invoke();
        }

        private void OnStopInteract(IInteractiveUser user) {
            _inputAxis.OnChanged -= OnInputAxisChanged;
            _isGrabbed = false;
            
            OnStopGrab.Invoke();
        }
        
        private void OnInputAxisChanged(Vector2 delta) {
            var delta3 = new Vector3(delta.x, 0f, delta.y) * _handSensitivity;
            
            var normal = _normalMode switch {
                NormalMode.Up => _transform.up,
                NormalMode.Forward => _transform.forward,
                NormalMode.Right => _transform.right,
                _ => Vector3.zero
            };

            if (_invertNormal) normal *= -1;
            
            var userRotationPlain = Quaternion.Euler(_user.TransformAdapter.Rotation.eulerAngles.WithX(0f));
            _inputDelta += Quaternion.FromToRotation(Vector3.up, normal) * userRotationPlain * delta3;
        }
    }

}
