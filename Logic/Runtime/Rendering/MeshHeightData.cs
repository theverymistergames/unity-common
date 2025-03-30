using System.Collections.Generic;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class MeshHeightData : MonoBehaviour, IUpdate {
        
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] [Min(0f)] private float _updateVerticesPeriod = 0.5f;
        [SerializeField] [Min(1)] private int _gridSizeX = 10;
        [SerializeField] [Min(1)] private int _gridSizeZ = 10;

        private struct Cell {
            public int firstIndex;
            public int lastIndex;
        }
        
        private readonly List<Vector3> _vertices = new();
        private int[] _vertexIndexToNextMap;
        private Cell[] _grid;

        private Transform _transform;
        private Bounds _localBounds;
        private float _lastUpdateTime;
        
        private void Awake() {
            FetchVertices();
            CreateGrid();
        }

        private void OnEnable() {
            _lastUpdateTime = -_updateVerticesPeriod;
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float time = Time.realtimeSinceStartup;
            if (time < _lastUpdateTime + _updateVerticesPeriod) return;
            
            _lastUpdateTime = time;
            FetchVertices();
        }

        public bool TrySamplePosition(ref Vector3 point) {
            return TrySamplePosition(LocalPositionToCell(_transform.InverseTransformPoint(point) - _localBounds.center), out point);
        }

        private bool TrySamplePosition(int cellIndex, out Vector3 point) {
            point = default;
            if (cellIndex < 0) return false;
            
            ref var cell = ref _grid[cellIndex];
            int index = cell.firstIndex;
            int count = 0;
            
            while (index >= 0) {
                point += _vertices[index];
                count++;

                index = _vertexIndexToNextMap[index];
            }
            
            if (count <= 0) return false;
            
            point = _transform.TransformPoint(point / count + _localBounds.center);
            return true;
        }
        
        private void CreateGrid() {
            _transform = _meshFilter.transform;
            _localBounds = _meshRenderer.localBounds;
            
            _grid = new Cell[_gridSizeX * _gridSizeZ];
            _vertexIndexToNextMap = new int[_vertices.Count];

            for (int i = 0; i < _grid.Length; i++) {
                ref var cell = ref _grid[i];
                cell.firstIndex = -1;
            }

            for (int i = 0; i < _vertices.Count; i++) {
                int cellIndex = LocalPositionToCell(_vertices[i]);
                
                if (cellIndex < 0) {
                    _vertexIndexToNextMap[i] = -1;
                    continue;
                }
                
                ref var cell = ref _grid[cellIndex];
                
                if (cell.firstIndex == -1) {
                    _vertexIndexToNextMap[i] = -1;
                    cell.firstIndex = i;
                    cell.lastIndex = i;
                    continue;
                }

                _vertexIndexToNextMap[i] = -1;
                _vertexIndexToNextMap[cell.lastIndex] = i;
                
                cell.lastIndex = i;
            }
        }

        private void FetchVertices() {
            _meshFilter.mesh.GetVertices(_vertices);
        }
        
        private int LocalPositionToCell(Vector3 localPosition) {
            var size = _localBounds.size;
            float x = localPosition.x / size.x + 0.5f;
            float z = localPosition.z / size.z + 0.5f;
            
            if (x is < 0f or > 1f || z is < 0f or > 1f) return -1;
            
            int xi = Mathf.Min(Mathf.FloorToInt(x * _gridSizeX), _gridSizeX - 1);
            int zi = Mathf.Min(Mathf.FloorToInt(z * _gridSizeZ), _gridSizeZ - 1);
            
            return xi * _gridSizeZ + zi;
        }

        private Vector3 CellToLocalPosition(int cell) {
            float x = (Mathf.FloorToInt((float) cell / _gridSizeZ) + 0.5f) / _gridSizeX - 0.5f;
            float z = (cell % _gridSizeZ + 0.5f) / _gridSizeZ - 0.5f;
            
            var size = _localBounds.size;
            return new Vector3(size.x * x, 0f, size.z * z);
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private readonly Dictionary<int, Color> _gridColors = new();
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            DrawGrid();
            DrawVertices();
            DrawCellPositions();
        }

        private void DrawVertices() {
            if (_grid == null) return;

            var up = _transform.up;
            var center = _localBounds.center;
            
            for (int i = 0; i < _grid.Length; i++) {
                var cell = _grid[i];

                int v = cell.firstIndex;

                if (!_gridColors.TryGetValue(i, out var color)) {
                    color = RandomExtensions.GetRandomColor();
                    _gridColors[i] = color;
                }

                while (v >= 0) {
                    DebugExt.DrawRay(_transform.TransformPoint(_vertices[v] + center), up * 0.005f, color);
                    v = _vertexIndexToNextMap[v];
                }
            }
        }
        
        private void DrawGrid() {
            if (_meshRenderer == null) return;

            var bounds = _meshRenderer.localBounds;
            var center  = bounds.center;
            var ext = bounds.extents.WithY(0f);
            var trf = _meshRenderer.transform;
            
            for (int i = 0; i <= _gridSizeX; i++) {
                float x = ((float) i / _gridSizeX - 0.5f) * ext.x * 2f;
                float z0 = ext.z;
                float z1 = -ext.z;
                
                var p0 = trf.TransformPoint(center + new Vector3(x, 0, z0));
                var p1 = trf.TransformPoint(center + new Vector3(x, 0, z1));
                
                DebugExt.DrawLine(p0, p1, Color.yellow, gizmo: true);
            }
            
            for (int i = 0; i <= _gridSizeZ; i++) {
                float z = ((float) i / _gridSizeZ - 0.5f) * ext.z * 2f;
                float x0 = ext.x;
                float x1 = -ext.x;
                
                var p0 = trf.TransformPoint(center + new Vector3(x0, 0, z));
                var p1 = trf.TransformPoint(center + new Vector3(x1, 0, z));
                
                DebugExt.DrawLine(p0, p1, Color.yellow, gizmo: true);
            }
        }

        private void DrawCellPositions() {
            if (_grid == null) return;

            for (int i = 0; i < _grid.Length; i++) {
                if (!TrySamplePosition(i, out var point)) continue;

                var cellPoint = _transform.TransformPoint(_localBounds.center + CellToLocalPosition(i));
                
                DebugExt.DrawSphere(cellPoint, 0.005f, Color.yellow, step: 0.25f, gizmo: true);
                DebugExt.DrawSphere(point, 0.01f, Color.yellow, step: 0.125f, gizmo: true);
                DebugExt.DrawLine(point, cellPoint, Color.yellow, gizmo: true);
            }
        }
#endif
    }
    
}