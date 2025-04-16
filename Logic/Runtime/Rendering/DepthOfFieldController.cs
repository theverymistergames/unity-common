using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.Logic.Rendering {
    
    public sealed class DepthOfFieldController : MonoBehaviour, IUpdate {

        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private VolumeProfile _volumeProfile;
        [SerializeField] [Min(0f)] private float _defaultDistance = 5f;
        [SerializeField] [Min(0f)] private float _maxDistance = 50f;
        [SerializeField] [Min(0f)] private float _nearFadeStart = 1f;
        [SerializeField] [Min(0f)] private float _nearFadeEnd = 5f;
        [SerializeField] [Min(0f)] private float _depthStart = 3f;
        [SerializeField] [Min(0f)] private float _depthEnd = 15f;
        [SerializeField] [Min(0f)] private float _farFadeStart = 1f;
        [SerializeField] [Min(0f)] private float _farFadeEnd = 5f;
        [SerializeField] [Min(0f)] private float _smoothing = 7f;
        
        private DepthOfField _depthOfField;
        private float _distanceSmoothed;
        
        private void Awake() {
            _volumeProfile.TryGet(out _depthOfField);
            _depthOfField.focusMode.value = DepthOfFieldMode.Manual;

            _distanceSmoothed = _defaultDistance;
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var info = _collisionDetector.CollisionInfo;
            float distance = info.hasContact ? info.distance : _defaultDistance;

            _distanceSmoothed = _distanceSmoothed.SmoothExpNonZero(distance, _smoothing, dt);
            
            ApplyDistance(_distanceSmoothed);
        }

        private void ApplyDistance(float distance) {
            float t = _maxDistance > 0f ? distance / _maxDistance : 1f;
            
            float nearFade = Mathf.Lerp(_nearFadeStart, _nearFadeEnd, t);
            float depth = Mathf.Lerp(_depthStart, _depthEnd, t);
            float farFade = Mathf.Lerp(_farFadeStart, _farFadeEnd, t);
            
            _depthOfField.nearFocusStart.value = distance - depth * 0.5f - nearFade;
            _depthOfField.nearFocusEnd.value = distance - depth * 0.5f;
            
            _depthOfField.farFocusStart.value = distance + depth * 0.5f;
            _depthOfField.farFocusEnd.value = distance + depth * 0.5f + farFade;
        }
    }
    
}