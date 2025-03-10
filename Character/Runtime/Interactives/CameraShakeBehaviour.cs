using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    public sealed class CameraShakeBehaviour : MonoBehaviour, IUpdate {

        [Header("Camera Settings")]
        [SerializeField] [Range(0f, 1f)] private float _targetWeight;
        [SerializeField] private float _cameraStateWeight = 1f;
        [SerializeField] [Min(0f)] private float _weightSmoothing = 3f;
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
        
        private CameraShaker _cameraShaker;
        private CameraContainer _cameraContainer;
        private float _weightSmoothed;
        private float _fovOffsetSmoothed;
        private int _shakerStateId;
        private int _containerStateId;
        private bool _isRegistered;

        public void SetWeight(float weight) {
            _targetWeight = Mathf.Clamp01(weight);
            
            Register();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void SetSmoothing(float smoothing) {
            _weightSmoothing = smoothing;
        }
        
        private void OnEnable() {
            _cameraShaker = CharacterSystem.Instance.GetCharacter().GetComponent<CameraShaker>();
            _cameraContainer = CharacterSystem.Instance.GetCharacter().GetComponent<CameraContainer>();

            Register();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDestroy() {
            Unregister();
        }

        private void Register() {
            if (_isRegistered) return;
            
            _shakerStateId = _cameraShaker.CreateState(_weightSmoothed);
            _containerStateId = _cameraContainer.CreateState();
            _isRegistered = true;
        }

        private void Unregister() {
            if (!_isRegistered) return;
            
            _cameraShaker.RemoveState(_shakerStateId);
            _cameraContainer.RemoveState(_containerStateId);
            _isRegistered = false;
        }

        void IUpdate.OnUpdate(float dt) {
            float targetWeight = enabled ? _targetWeight : 0f;
            _weightSmoothed = _weightSmoothed.SmoothExpNonZero(targetWeight, _weightSmoothing, dt);
            
            _cameraShaker.SetWeight(_shakerStateId, _cameraStateWeight);
            _cameraShaker.SetSpeed(_shakerStateId, Vector3.Lerp(_noiseSpeedStart, _noiseSpeedEnd, _weightSmoothed));
            _cameraShaker.SetPosition(_shakerStateId, _noisePositionOffset, _noisePositionMul * _weightSmoothed);
            _cameraShaker.SetRotation(_shakerStateId, _noiseRotationOffset, _noiseRotationMul * _weightSmoothed);

            float targetFov = Mathf.Lerp(_fovOffsetStart, _fovOffsetEnd, _weightSmoothed);
            _cameraContainer.SetFovOffset(_containerStateId, _cameraStateWeight, targetFov);
            
            if (_weightSmoothed > 0f) return;

            Unregister();
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }
    }
    
}