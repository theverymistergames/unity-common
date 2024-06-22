using MisterGames.Actors;
using MisterGames.Character.View;
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
        private Vector2 _orientation;
        
        private void Awake() {
            _interactive = GetComponent<Interactive>();

            var eulers = _target.eulerAngles;
            _orientation = new Vector2(eulers.z, eulers.y);
            
            _viewClamp.ApplyVerticalClamp(_orientation, _viewClamp.Vertical);
            _viewClamp.ApplyHorizontalClamp(_orientation, _viewClamp.Horizontal);
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
            var current = _orientation;
            var target = _orientation + _inputAccum;
            _inputAccum = Vector2.zero;
            
            _viewClamp.Process(_target.position, current, ref target, dt);
            _orientation = _smoothing > 0f 
                ? Vector2.Lerp(current, target, dt * _smoothing)
                : target;
            
            _target.rotation = Quaternion.Euler(0f, _orientation.y, _orientation.x);
        }
    }
    
}