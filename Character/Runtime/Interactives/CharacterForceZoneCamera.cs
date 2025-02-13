using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    public sealed class CharacterForceZoneCamera : MonoBehaviour, IUpdate {

        [SerializeField] private RigidbodyForceZone _rigidbodyForceZone;
        
        [Header("Camera Settings")]
        [SerializeField] private float _cameraStateWeight = 1f;
        [SerializeField] [Min(0f)] private float _forceWeightSmoothing = 3f;
        [SerializeField] [Min(0f)] private float _fovSmoothing = 3f;
        [SerializeField] private float _fovOffsetStart; 
        [SerializeField] private float _fovOffsetEnd;
        
        [Header("Camera Shake")]
        [SerializeField] private Vector3 _noiseSpeedStart;
        [SerializeField] private Vector3 _noiseSpeedEnd;
        [SerializeField] private Vector3 _noisePositionOffset;
        [SerializeField] private Vector3 _noiseRotationOffset;
        [SerializeField] private Vector3 _noisePositionMul;
        [SerializeField] private Vector3 _noiseRotationMul;
        
        private Rigidbody _characterRigidbody;
        private CameraShaker _cameraShaker;
        private CameraContainer _cameraContainer;
        private float _forceWeightSmoothed;
        private float _fovOffsetSmoothed;
        private int _shakerStateId;
        private int _containerStateId;
        
        private void OnEnable() {
            _rigidbodyForceZone.OnEnterZone += OnEnterZone;

            if (_characterRigidbody != null && _rigidbodyForceZone.InZone(_characterRigidbody)) {
                PlayerLoopStage.LateUpdate.Subscribe(this);

                _cameraShaker.RemoveState(_shakerStateId);
                _cameraContainer.RemoveState(_containerStateId);
                
                _shakerStateId = _cameraShaker.CreateState(_forceWeightSmoothed);    
                _containerStateId = _cameraContainer.CreateState();    
            }
        }

        private void OnDisable() {
            _rigidbodyForceZone.OnEnterZone -= OnEnterZone;
        }

        private void OnEnterZone(Rigidbody rigidbody) {
            if (_characterRigidbody != null ||
                !rigidbody.TryGetComponent(out IActor actor) ||
                !actor.TryGetComponent(out MainCharacter _)) 
            {
                return;
            }

            _characterRigidbody = rigidbody;
            _cameraShaker = actor.GetComponent<CameraShaker>();
            _cameraContainer = actor.GetComponent<CameraContainer>();

            _shakerStateId = _cameraShaker.CreateState(_forceWeightSmoothed);
            _containerStateId = _cameraContainer.CreateState();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float targetWeight = enabled ? _rigidbodyForceZone.GetForceWeight(_characterRigidbody) : 0f;
            _forceWeightSmoothed = _forceWeightSmoothed.SmoothExpNonZero(targetWeight, _forceWeightSmoothing, dt);
            
            _cameraShaker.SetWeight(_shakerStateId, _cameraStateWeight);
            _cameraShaker.SetSpeed(_shakerStateId, Vector3.Lerp(_noiseSpeedStart, _noiseSpeedEnd, _forceWeightSmoothed));
            _cameraShaker.SetPosition(_shakerStateId, _noisePositionOffset, _noisePositionMul * _forceWeightSmoothed);
            _cameraShaker.SetRotation(_shakerStateId, _noiseRotationOffset, _noiseRotationMul * _forceWeightSmoothed);

            float targetFov = Mathf.Lerp(_fovOffsetStart, _fovOffsetEnd, _forceWeightSmoothed);
            _cameraContainer.SetFovOffset(_containerStateId, _cameraStateWeight, targetFov);
            
            if (_rigidbodyForceZone.enabled && _rigidbodyForceZone.InZone(_characterRigidbody) || _forceWeightSmoothed > 0f) return;
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            _cameraShaker.RemoveState(_shakerStateId);
            _cameraContainer.RemoveState(_containerStateId);
            
            _characterRigidbody = null;
            _cameraShaker = null;
            _cameraContainer = null;
        }
    }
    
}