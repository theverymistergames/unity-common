using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {
    
    [ExecuteInEditMode]
    public sealed class TransformFollow : MonoBehaviour, IUpdate {
    
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _follow;
        [SerializeField] private Vector3 _positonOffset;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] [Min(0f)] private float _positionSmoothing = 0f;
        [SerializeField] [Min(0f)] private float _rotationSmoothing = 0f;
        
        private void OnEnable() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            Follow(dt);
        }

        private void Follow(float dt) {
#if UNITY_EDITOR
            if (_follow.IsChildOf(_target)) {
                Debug.Log($"TransformFollow [{gameObject.GetPathInScene()}]: follow transform {_follow} cannot be child of target transform {_target}");
                return;
            }
#endif
            
            _target.GetPositionAndRotation(out var pos, out var rot);
            _follow.GetPositionAndRotation(out var followPos, out var followRot);
            
            var targetPos = followPos + followRot * _positonOffset;
            var targetRot = followRot * Quaternion.Euler(_rotationOffset);
            
            var nextPos = pos.SmoothExpNonZero(targetPos, _positionSmoothing, dt);
            var nextRot = rot.SlerpNonZero(targetRot, _rotationSmoothing, dt);
            
            _target.SetPositionAndRotation(nextPos, nextRot);

#if UNITY_EDITOR
            if (pos != nextPos || rot != nextRot) EditorUtility.SetDirty(_target);
#endif
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInEditor;
        [SerializeField] private bool _showGizmo = true;

        private float _lastTimeEditor = -1f;
        
        private void LateUpdate() {
            if (!_updateInEditor || Application.isPlaying || _target == null || _follow == null) return;

            float lastTime = _lastTimeEditor;
            _lastTimeEditor = Time.realtimeSinceStartup;
            float dt = lastTime >= 0f ? _lastTimeEditor - lastTime : 0.05f;
            
            Follow(dt);
        }

        private void Reset() {
            _target = transform;
        }
#endif
    }
    
}