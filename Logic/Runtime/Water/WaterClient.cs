using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WaterClient : MonoBehaviour, IActorComponent, IWaterClient {

        [SerializeField] private bool _ignoreWaterZone;

        [Header("Floating Body")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Vector3[] _mainFloatingPoints;

        [Header("Settings")]
        [SerializeField] private float _buoyancy = 0f;
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
        
        public bool IgnoreWaterZone { get => _ignoreWaterZone; set => _ignoreWaterZone = value; }

        public Rigidbody Rigidbody => _rigidbody;
        public int FloatingPointCount => _mainFloatingPoints.Length;
        public Vector3 GetFloatingPoint(int index) => _transform.TransformPoint(_mainFloatingPoints[index]);

        public float Buoyancy { get => _buoyancy; set => _buoyancy = value; }
        public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }

        private Transform _transform;
        
        private void Awake() {
            _transform = transform;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void Reset() {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
            _mainFloatingPoints = new []{ _transform.InverseTransformPoint(_rigidbody.transform.position) };
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            if (_transform == null) _transform = transform;
            
            var center = Vector3.zero;
            int count = _mainFloatingPoints?.Length ?? 0;
            
            for (int i = 0; i < count; i++) {
                var fp = GetFloatingPoint(i);
                center += fp;
                
                DebugExt.DrawSphere(fp, 0.03f, Color.yellow, gizmo: true);
            }
            
            if (count > 0) center /= count;
            
            for (int i = 0; i < _mainFloatingPoints?.Length; i++) {
                DebugExt.DrawLine(center, GetFloatingPoint(i), Color.yellow, gizmo: true);
            }
        }
        
        [CustomEditor(typeof(WaterClient))]
        private sealed class WaterClientEditor : Editor {
            
            private const string UndoKey = "WaterClient_SetPointsPosition";
            private const string Edit = "Edit Points";

            private readonly HashSet<Object> _editSet = new();
            
            private void OnDisable() {
                _editSet.Clear();
            }

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                
                bool edit = EditorGUILayout.Toggle(Edit, _editSet.Contains(target));
                
                if (edit) _editSet.Add(target);
                else _editSet.Remove(target);
            }

            private void OnSceneGUI() {
                if (target is not WaterClient waterClient || !_editSet.Contains(target)) return;
                
                Undo.RecordObject(waterClient, UndoKey);

                var points = waterClient._mainFloatingPoints;
                if (waterClient._transform == null) waterClient._transform = waterClient.transform;
                
                for (int i = 0; i < points?.Length; i++) {
                    var p = waterClient.GetFloatingPoint(i);
                    var newP = Handles.PositionHandle(p, Quaternion.identity);
                    
                    if (newP != p)
                    {
                        points[i] = waterClient._transform.InverseTransformPoint(newP);
                        EditorUtility.SetDirty(waterClient);
                    }
                }
            }
        }
#endif
    }
    
}