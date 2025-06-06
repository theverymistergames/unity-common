using MisterGames.Character.Core;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class AlphaByDistanceToCharacter : MonoBehaviour, IUpdate {
        
        [SerializeField] private Transform _transform;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] [Min(0f)] private float _minDistance = 0f;
        [SerializeField] [Min(0f)] private float _maxDistance = 3f;
        [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private static readonly int Alpha = Shader.PropertyToID("_alpha");
        private float _progress;
        
        private void OnEnable() {
            _progress = -1f;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float lastProgress = _progress;
            _progress = GetProgress();
            
            if (_progress.IsNearlyEqual(lastProgress)) return;

            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].material.SetFloat(Alpha, _alphaCurve.Evaluate(_progress));
            }
        }

        private float GetProgress() {
            var characterPos = CharacterSystem.Instance.GetCharacter().Transform.position;
            float distance = (_transform.position - characterPos).magnitude;
            
            return Mathf.Clamp01(distance - _minDistance) / (_maxDistance - _minDistance);
        }

#if UNITY_EDITOR
        private void Reset() {
            _transform = transform;
            _renderers = GetComponentsInChildren<Renderer>();
        }

        private void OnValidate() {
            if (_maxDistance < _minDistance) _maxDistance = _minDistance;
        }
#endif
    }
    
}