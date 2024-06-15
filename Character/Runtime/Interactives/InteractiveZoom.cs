using MisterGames.Actors;
using MisterGames.Input.Actions;
using MisterGames.Interact.Interactives;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveZoom : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionVector2 _zoomInput;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _fovSmoothing = 10f;
        [SerializeField] private Vector2 _fovRange;
        
        [Header("Rotation")]
        [SerializeField] private Transform _rotationTarget;
        [SerializeField] private Vector2 _angleRange;
        [SerializeField] private Vector3 _axis;

        private Interactive _interactive;
        private Vector2 _inputAccum;
        private float _targetFov;

        private void Awake() {
            _interactive = GetComponent<Interactive>();
            _targetFov = _camera.fieldOfView;
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
            _zoomInput.OnChanged -= OnZoomInput;
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
                _zoomInput.OnChanged -= OnZoomInput;
                _zoomInput.OnChanged += OnZoomInput;
                return;
            }
            
            PlayerLoopStage.Update.Unsubscribe(this);
            _zoomInput.OnChanged -= OnZoomInput;
            _inputAccum = Vector2.zero;
        }

        private void OnZoomInput(Vector2 delta) {
            _inputAccum += new Vector2(delta.x, delta.y) * _sensitivity;
        }

        public void OnUpdate(float dt) {
            _targetFov = Mathf.Clamp( _targetFov + _inputAccum.y, _fovRange.x, _fovRange.y);
            _inputAccum = Vector2.zero;
            
            float currentFov = Mathf.Lerp(_camera.fieldOfView, _targetFov, dt * _fovSmoothing);
            _camera.fieldOfView = currentFov;
            
            float t = Mathf.Clamp01((currentFov - _fovRange.x) / (_fovRange.y - _fovRange.x));
            float targetAngle = Mathf.Lerp(_angleRange.x, _angleRange.y, t);

            _rotationTarget.localRotation = Quaternion.AngleAxis(targetAngle, _axis);
        }
    }
    
}