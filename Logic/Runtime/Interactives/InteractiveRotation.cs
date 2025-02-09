using MisterGames.Actors;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Input.Actions;
using MisterGames.Interact.Interactives;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Interactives {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveRotation : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] private InputActionVector2 _rotationInput;
        [SerializeField] private Vector2 _sensitivity = Vector2.one;
        [SerializeField] [Min(0f)] private float _smoothing = 10f;
        [SerializeField] [Min(0f)] private float _inputSmoothing = 1f;
        [SerializeField] private ViewClampProcessor _viewClamp;

        private Interactive _interactive;
        private Vector2 _inputAccum;
        private Vector2 _targetOrientation;
        private Vector2 _smoothedOrientation;
        private float _smoothFactor;
        private bool _finishingFlag;
        
        private void Awake() {
            _interactive = GetComponent<Interactive>();

            var eulers = _target.eulerAngles;
            _targetOrientation = new Vector2(eulers.z, eulers.y);
            _smoothedOrientation = _targetOrientation;
            
            _viewClamp.SetViewOrientation(_smoothedOrientation);
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
            
            ActualizeSubscriptions();
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
            
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
            _rotationInput.OnChanged -= OnRotationInput;
            
            _inputAccum = Vector2.zero;
            _finishingFlag = false;
        }

        private void OnStartInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void OnStopInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void ActualizeSubscriptions() {
            if (_interactive.IsInteracting) {
                _finishingFlag = false;
                PlayerLoopStage.Update.Subscribe(this);
                _rotationInput.OnChanged -= OnRotationInput;
                _rotationInput.OnChanged += OnRotationInput;
                return;
            }
            
            _rotationInput.OnChanged -= OnRotationInput;
            _finishingFlag = true;
        }

        private void OnRotationInput(Vector2 delta) {
            _inputAccum += new Vector2(delta.y, delta.x) * _sensitivity;
        }

        void IUpdate.OnUpdate(float dt) {
            float consume = _inputSmoothing * dt;
            _targetOrientation += consume * _inputAccum;
            _inputAccum *= Mathf.Max(1f - consume, 0f);

            _viewClamp.Process(_target.position, Quaternion.identity, ref _smoothedOrientation, ref _targetOrientation, dt);
            _smoothedOrientation = _smoothedOrientation.SmoothExpNonZero(_targetOrientation, _smoothing, dt);

            _target.rotation = Quaternion.Euler(0f, _smoothedOrientation.y, _smoothedOrientation.x);

            if (_finishingFlag && _smoothedOrientation == _targetOrientation && _inputAccum == Vector2.zero) {
                _finishingFlag = false;
                PlayerLoopStage.Update.Unsubscribe(this);
            }
        }
    }
    
}