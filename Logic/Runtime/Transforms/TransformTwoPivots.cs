using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {
    
    [ExecuteInEditMode]
    public sealed class TransformTwoPivots : MonoBehaviour, IUpdate {
        
        [Header("State")]
        [SerializeField] private bool _setupOnAwake = true;
        [SerializeField] [ReadOnly] private bool _initialized;
        
        [Header("Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _p0;
        [SerializeField] private Transform _p1;
        [SerializeField] private Vector3 _p0Offset;
        [SerializeField] private Vector3 _p1Offset;
        [SerializeField] [Range(0f, 1f)] private float _pivot = 0.5f;
        [SerializeField] private StretchMode _stretch;
        
        [SerializeField] [HideInInspector] private Vector3 _p0Local;
        [SerializeField] [HideInInspector] private Vector3 _p1Local;
        [SerializeField] [HideInInspector] private Vector3 _originDir;
        [SerializeField] [HideInInspector] private Quaternion _localRotOffset;
        [SerializeField] [HideInInspector] private Vector3 _initialScale;
        [SerializeField] [HideInInspector] private float _initialDistance;
        [SerializeField] [HideInInspector] private int _stretchAxis;

        private enum StretchMode {
            None,
            LongAxis,
            Full,
        }
        
        private void Awake() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            
            if (_setupOnAwake || !_initialized) SetupOriginPositions();
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

        private void SetupOriginPositions() {
            _target.GetPositionAndRotation(out var pos, out var rot);

            _p0.GetPositionAndRotation(out var p0, out var r0);
            _p1.GetPositionAndRotation(out var p1, out var r1);

            p0 += r0 * _p0Offset;
            p1 += r1 * _p1Offset;

            _originDir = p1 - p0;

            var invRot = Quaternion.Inverse(rot);
            _p0Local = invRot * (pos - p0);
            _p1Local = invRot * (pos - p1);
            _localRotOffset = rot;

            _initialScale = _target.localScale;
            _initialDistance = _originDir.magnitude;
            _stretchAxis = GetDominantAxis(invRot * _originDir);

            _initialized = true;

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        private void SetPivotPoints(Vector3 p0, Vector3 p1, float t) {
#if UNITY_EDITOR
            _target.GetPositionAndRotation(out var oldPos, out var oldRot);
            var oldScale = _target.localScale;
#endif
            
            var diff = p1 - p0;
            var deltaRot = Quaternion.FromToRotation(_originDir, diff);
            var currentRot = deltaRot * _localRotOffset;

            var p0Local = _p0Local;
            var p1Local = _p1Local;
            
            if (_stretch != StretchMode.None && _initialDistance > 0f) {
                float factor = diff.magnitude / _initialDistance;
                var scale = _initialScale;
                
                if (_stretch == StretchMode.LongAxis) {
                    switch (_stretchAxis) {
                        case 0:
                            p0Local.x *= factor;
                            p1Local.x *= factor;
                            scale.x *= factor;
                            break;
                        case 1:
                            p0Local.y *= factor;
                            p1Local.y *= factor;
                            scale.y *= factor;
                            break;
                        default:
                            p0Local.z *= factor;
                            p1Local.z *= factor;
                            scale.z *= factor;
                            break;
                    }   
                }
                else {
                    p0Local *= factor;
                    p1Local *= factor;
                    scale *= factor;
                }
                
                _target.localScale = scale;
            }
            
            var pos = Vector3.Lerp(p0, p1, t) + currentRot * Vector3.Lerp(p0Local, p1Local, t);

            _target.SetPositionAndRotation(pos, currentRot);

#if UNITY_EDITOR
            if (!Application.isPlaying &&
                (oldPos != _target.position || oldRot != _target.rotation || oldScale != _target.localScale)) 
            {
                EditorUtility.SetDirty(_target);
            }
#endif
        }

        private static int GetDominantAxis(Vector3 v) {
            float ax = Mathf.Abs(v.x);
            float ay = Mathf.Abs(v.y);
            float az = Mathf.Abs(v.z);
            if (ax >= ay && ax >= az) return 0;
            return ay >= az ? 1 : 2;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInEditor;

        private void Reset() {
            _target = transform;
        }

        [Button]
        private void SetupInitialPositions() {
            if (_target == null || _p0 == null || _p1 == null) return;

            SetupOriginPositions();
        }

        private void LateUpdate() {
            if (!_updateInEditor || !_initialized || Application.isPlaying || _target == null || _p0 == null || _p1 == null) return;
            
            _p0.GetPositionAndRotation(out var p0, out var r0);
            _p1.GetPositionAndRotation(out var p1, out var r1);
            
            SetPivotPoints(p0 + r0 * _p0Offset, p1 + r1 * _p1Offset, _pivot);
        }
#endif
    }
    
}