using MisterGames.Common.Tick;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {
    
    [ExecuteInEditMode]
    public sealed class TransformTwoPivots : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] private Transform _p0;
        [SerializeField] private Transform _p1;
        [SerializeField] private Vector3 _p0Offset;
        [SerializeField] private Vector3 _p1Offset;
        [SerializeField] [Range(0f, 1f)] private float _pivot = 0.5f;

        private Vector3 _p0Local;
        private Vector3 _p1Local;
        private Vector3 _originDir;
        private Quaternion _localRotOffset;

        private void Awake() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            SetupOriginPositions();
        }

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
            _p0.GetPositionAndRotation(out var p0, out var r0);
            _p1.GetPositionAndRotation(out var p1, out var r1);
            
            SetPivotPoints(p0 + r0 * _p0Offset, p1 + r1 * _p1Offset, _pivot);
        }

        private void SetPivotPoints(Vector3 p0, Vector3 p1, float t) {
            var rot = Quaternion.FromToRotation(_originDir, p1 - p0);
            var pos = Vector3.Lerp(p0, p1, t) + rot * Vector3.Lerp(_p0Local, _p1Local, t);

            _target.SetPositionAndRotation(pos, rot * _localRotOffset);
        }

        private void SetupOriginPositions() {
            _target.GetPositionAndRotation(out var pos, out var rot);

            _p0.GetPositionAndRotation(out var p0, out var r0);
            _p1.GetPositionAndRotation(out var p1, out var r1);

            p0 += r0 * _p0Offset;
            p1 += r1 * _p1Offset;

            _originDir = p1 - p0;

            _p0Local = pos - p0;
            _p1Local = pos - p1;
            _localRotOffset = rot;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInEditor;
        [SerializeField] private bool _showDebugInfo;
        
        private bool _hasInitializedInEditor;

        private void Reset() {
            _target = transform;
        }

        [Button]
        private void SetupInitialPositions() {
            if (_target == null || _p0 == null || _p1 == null) return;

            _hasInitializedInEditor = true;
            SetupOriginPositions();
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (_p0 != null) DebugExt.DrawSphere(_p0.position + _p0.rotation * _p0Offset, 0.03f, Color.yellow, gizmo: true);
            if (_p1 != null) DebugExt.DrawSphere(_p1.position + _p1.rotation * _p1Offset, 0.03f, Color.yellow, gizmo: true);
        }

        private void LateUpdate() {
            if (!_updateInEditor || !_hasInitializedInEditor || Application.isPlaying || _target == null || _p0 == null || _p1 == null) return;
            
            _p0.GetPositionAndRotation(out var p0, out var r0);
            _p1.GetPositionAndRotation(out var p1, out var r1);
            
            SetPivotPoints(p0 + r0 * _p0Offset, p1 + r1 * _p1Offset, _pivot);
            
            EditorUtility.SetDirty(_target);
        }
#endif
    }
    
}