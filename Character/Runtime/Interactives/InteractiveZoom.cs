using MisterGames.Actors;
using MisterGames.Character.View;
using MisterGames.Input.Actions;
using MisterGames.Interact.Interactives;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class InteractiveZoom : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Target Camera")]
        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionVector2 _zoomInput;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _fovSmoothing = 10f;
        [SerializeField] private Vector2 _fovRange;
        
        [Header("Player Camera")]
        [SerializeField] private Vector2 _playerFovRange;
        [SerializeField] private Vector2 _playerAttachDistanceRange;
        [SerializeField] private float _playerAttachDistanceStart;
        [SerializeField] [Min(0f)] private float _playerFovSmoothing = 5f;
        [SerializeField] [Min(0f)] private float _playerAttachSmoothing = 5f;
        
        [Header("Visual")]
        [SerializeField] private Transform _visualRotation;
        [SerializeField] private Transform _visualTranslation;
        [SerializeField] private Vector2 _angleRange;
        [SerializeField] private Vector3 _angleAxis;
        [SerializeField] private Vector2 _positionRange;
        [SerializeField] private Vector3 _positionAxis;

        private CharacterViewPipeline _characterView;
        private Interactive _interactive;
        private Vector2 _inputAccum;
        private Vector3 _initialVisualPosition;
        private Quaternion _initialVisualRotation;
        private float _targetFov;
        private float _playerAttachDistance;
        private float _playerFovOffset;
        private int _cameraStateId;

        private void Awake() {
            _interactive = GetComponent<Interactive>();
            _targetFov = _camera.fieldOfView;

            _initialVisualPosition = _visualTranslation.localPosition;
            _initialVisualRotation = _visualRotation.localRotation;
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
            _playerFovOffset = 0f;
            _playerAttachDistance = _playerAttachDistanceStart;
            
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

                if (_characterView == null) {
                    foreach (var user in _interactive.Users) {
                        if (user.Root.TryGetComponent(out IActor actor) && actor.TryGetComponent(out _characterView)) break; 
                    }
                }

                if (_characterView != null) {
                   _cameraStateId = _characterView.CameraContainer.CreateState();
                }
                
                return;
            }
            
            PlayerLoopStage.Update.Unsubscribe(this);

            _zoomInput.OnChanged -= OnZoomInput;
            _inputAccum = Vector2.zero;

            if (_characterView != null) {
                _characterView.CameraContainer.RemoveState(_cameraStateId);
            }
        }

        private void OnZoomInput(Vector2 delta) {
            _inputAccum += new Vector2(delta.x, delta.y) * _sensitivity;
        }

        public void OnUpdate(float dt) {
            _targetFov = Mathf.Clamp( _targetFov + _inputAccum.y, _fovRange.x, _fovRange.y);
            _inputAccum = Vector2.zero;
            
            float currentFov = Mathf.Lerp(_camera.fieldOfView, _targetFov, dt * _fovSmoothing);
            _camera.fieldOfView = currentFov;

            float t = GetZoom();
            
            UpdateVisuals(t);
            UpdatePlayerVisuals(t, dt);
        }

        private float GetZoom() {
            return Mathf.Clamp01((_camera.fieldOfView - _fovRange.x) / (_fovRange.y - _fovRange.x));
        }

        private void UpdateVisuals(float t) {
            var targetPosition = _positionAxis * Mathf.Lerp(_positionRange.x, _positionRange.y, t);
            var targetRotation = Quaternion.AngleAxis(Mathf.Lerp(_angleRange.x, _angleRange.y, t), _angleAxis);

            _visualTranslation.localPosition = _initialVisualPosition + targetPosition;
            _visualRotation.localRotation = _initialVisualRotation * targetRotation;
        }

        private void UpdatePlayerVisuals(float t, float dt) {
            if (_characterView == null || !_characterView.IsAttached) return;

            float fovOffset = Mathf.Lerp(_playerFovRange.x, _playerFovRange.y, t);
            float attachDistance = Mathf.Lerp(_playerAttachDistanceRange.x, _playerAttachDistanceRange.y, t);

            _playerFovOffset = Mathf.Lerp(_playerFovOffset, fovOffset, dt * _playerFovSmoothing);
            _playerAttachDistance = Mathf.Lerp(_playerAttachDistance, attachDistance, dt * _playerAttachSmoothing);
            
            _characterView.CameraContainer.SetFovOffset(_cameraStateId, 1f, _playerFovOffset);
            _characterView.AttachDistance = _playerAttachDistance;
        }
    }
    
}