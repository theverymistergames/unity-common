using MisterGames.Actors;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Input.Actions;
using MisterGames.Interact.Interactives;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveRotation : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] private InputActionVector2 _rotationInput;
        [SerializeField] private Vector2 _sensitivity = Vector2.one;
        [SerializeField] private float _smoothing = 10f;
        [SerializeField] private ViewClampProcessor _viewClamp;

        private Interactive _interactive;
        private Vector2 _inputAccum;
        private Vector2 _targetOrientation;
        private Vector2 _smoothedOrientation;
        
        private void Awake() {
            _interactive = GetComponent<Interactive>();

            var eulers = _target.eulerAngles;
            _targetOrientation = new Vector2(eulers.z, eulers.y);
            _smoothedOrientation = _targetOrientation;
            
            _viewClamp.SetClampCenter(_smoothedOrientation);
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
        }

        private void OnStartInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void OnStopInteract(IInteractiveUser user) {
            ActualizeSubscriptions();
        }

        private void ActualizeSubscriptions() {
            if (_interactive.IsInteracting) {
                PlayerLoopStage.Update.Subscribe(this);
                _rotationInput.OnChanged -= OnRotationInput;
                _rotationInput.OnChanged += OnRotationInput;
                return;
            }
            
            PlayerLoopStage.Update.Unsubscribe(this);
            _rotationInput.OnChanged -= OnRotationInput;
            _inputAccum = Vector2.zero;
        }

        private void OnRotationInput(Vector2 delta) {
            _inputAccum += new Vector2(delta.y, delta.x) * _sensitivity;
        }

        public void OnUpdate(float dt) {
            _targetOrientation += _inputAccum;
            _inputAccum = Vector2.zero;
            
            _viewClamp.Process(_target.position, _smoothedOrientation, ref _targetOrientation, dt);
            _smoothedOrientation = _smoothedOrientation.SmoothExpNonZero(_targetOrientation, dt * _smoothing); 
            
            _target.rotation = Quaternion.Euler(0f, _smoothedOrientation.y, _smoothedOrientation.x);
        }   
    }
    
}