using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using MisterGames.Input.Actions;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Objects {

    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveGrab : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain; 
        
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

        private Interactive _interactive;
        
        private Transform _transform;
        private Transform _userTransform;
        
        private Vector3 _smoothedGrabPoint;
        private Vector3 _grabPoint;
        private Vector3 _userPosition;
        private Vector3 _inputDelta;
        
        private bool _isGrabbed;

        private void Awake() {
            _interactive = GetComponent<Interactive>();
            _transform = transform; 
        }

        private void OnEnable() {
            _interactive.OnStartInteractBy += OnStartInteractBy;
            _interactive.OnStopInteract += OnStopInteract;
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _interactive.OnStartInteractBy -= OnStartInteractBy;
            _interactive.OnStopInteract -= OnStopInteract;
            _timeDomain.UnsubscribeUpdate(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isGrabbed) return;
            
            var prevPosition = _userPosition;
            _userPosition = _userTransform.position;
            var userPositionDelta = _walkSensitivity * (_userPosition - prevPosition);
            
            _grabPoint += userPositionDelta + _inputDelta;
            
            _inputDelta = Vector3.zero;

            var prevSmoothedGrabPoint = _smoothedGrabPoint;
            _smoothedGrabPoint = Vector3.Lerp(_smoothedGrabPoint, _grabPoint, _smoothFactor * dt);
            
            OnGrab.Invoke(prevSmoothedGrabPoint, _smoothedGrabPoint);
        }

        private void OnStartInteractBy(InteractiveUser user) {
            _userTransform = user.transform;
            _userPosition = _userTransform.position;
            
            _grabPoint = user.Handle;
            _smoothedGrabPoint = _grabPoint;
            
            _inputAxis.OnChanged -= OnInputAxisChanged;
            _inputAxis.OnChanged += OnInputAxisChanged;

            _isGrabbed = true;
            
            OnStartGrab.Invoke();
        }

        private void OnStopInteract() {
            _inputAxis.OnChanged -= OnInputAxisChanged;
            _userTransform = null;
            
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
            
            var userRotationPlain = Quaternion.Euler(_userTransform.rotation.eulerAngles.WithX(0f));
            
            _inputDelta += Quaternion.FromToRotation(Vector3.up, normal) * userRotationPlain * delta3;
        }

        private enum NormalMode {
            Up,
            Forward,
            Right
        }
        
    }

}
