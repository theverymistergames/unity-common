using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Splines.Utils;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Splines.Tools {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class SplineMeshBender : MonoBehaviour {
        
        private Mesh _result;
        private MeshInfo _source;
        private MeshFillingMode _mode = MeshFillingMode.StretchToInterval;
        private SplineContainer _splineContainer;
        private float _splineLength;

        private void OnEnable() {
            var meshFilter = GetComponent<MeshFilter>();
            
            if (meshFilter.sharedMesh == null) {
                _result = new Mesh { name = "SplineMesh" };
                meshFilter.sharedMesh = _result;
                return;
            }

            _result = meshFilter.sharedMesh;
        }

        public void SetSourceMesh(MeshInfo sourceMesh, MeshFillingMode meshFillingMode) {
            _source = sourceMesh;
            _mode = meshFillingMode;
        }
        
        public void SetSpline(SplineContainer splineContainer) {
            _splineContainer = splineContainer;
            _splineLength = _splineContainer.CalculateLength();

            Compute();
        }
        
        private  void Compute() {
            switch (_mode) {
                case MeshFillingMode.Once:
                    FillOnce();
                    break;
                case MeshFillingMode.Repeat:
                    FillRepeat();
                    break;
                case MeshFillingMode.StretchToInterval:
                    FillStretch();
                    break;
            }
        }

        private void FillOnce() {
            var bentVertices = new List<MeshVertex>(_source.vertices.Length);

            for (int i = 0; i < _source.vertices.Length; i++) {
                var vert = _source.vertices[i];
                float distance = Mathf.Clamp(vert.position.x - _source.xMin, 0f, _splineLength);

                var bent = GetBent(vert, distance);
                bentVertices.Add(bent);
            }

            MeshUtils.Update(_result,
                _source.mesh,
                _source.triangles,
                bentVertices.Select(b => b.position),
                bentVertices.Select(b => b.normal)
            );
        }

        private void FillRepeat() {
            int repetitionCount = Mathf.FloorToInt(_splineLength / _source.length);
            
            var triangles = new List<int>();
            var uv1 = new List<Vector2>();
            var uv2 = new List<Vector2>();
            var uv3 = new List<Vector2>();
            var uv4 = new List<Vector2>();
            var uv5 = new List<Vector2>();
            var uv6 = new List<Vector2>();
            var uv7 = new List<Vector2>();
            var uv8 = new List<Vector2>();
            
            for (int i = 0; i < repetitionCount; i++) {
                for (int j = 0; j < _source.triangles.Length; j++) {
                    int index = _source.triangles[j];
                    triangles.Add(index + _source.vertices.Length * i);
                }

                uv1.AddRange(_source.mesh.uv);
                uv2.AddRange(_source.mesh.uv2);
                uv3.AddRange(_source.mesh.uv3);
                uv4.AddRange(_source.mesh.uv4);
                uv5.AddRange(_source.mesh.uv5);
                uv6.AddRange(_source.mesh.uv6);
                uv7.AddRange(_source.mesh.uv7);
                uv8.AddRange(_source.mesh.uv8);
            }

            var bentVertices = new List<MeshVertex>(_source.vertices.Length);
            float offset = 0;

            for (int i = 0; i < repetitionCount; i++) {
                for (int j = 0; j < _source.vertices.Length; j++) {
                    var vert = _source.vertices[j];
                    float distance = vert.position.x - _source.xMin + offset;

                    var bent = GetBent(vert, distance);
                    bentVertices.Add(bent);
                }

                offset += _source.length;
            }

            MeshUtils.Update(
                _result,
                _source.mesh,
                triangles,
                bentVertices.Select(b => b.position),
                bentVertices.Select(b => b.normal),
                uv1, uv2, uv3, uv4,
                uv5, uv6, uv7, uv8
            );
        }

        private void FillStretch() {
            var bentVertices = new List<MeshVertex>(_source.vertices.Length);

            for (int i = 0; i < _source.vertices.Length; i++) {
                var vert = _source.vertices[i];
                float distanceRate = _source.length > 0
                    ? Math.Abs(vert.position.x - _source.xMin) / _source.length
                    : 0f;

                float distance = _splineLength * distanceRate;
                var bent = GetBent(vert, distance);
                bentVertices.Add(bent);
            }

            MeshUtils.Update(
                _result,
                _source.mesh,
                _source.triangles,
                bentVertices.Select(b => b.position),
                bentVertices.Select(b => b.normal)
            );
            
            if (TryGetComponent(out MeshCollider meshCollider)) {
                meshCollider.sharedMesh = _result;
            }
        }
        
        private MeshVertex GetBent(MeshVertex vertex, float distance) {
            float t = _splineLength <= 0f ? distance / _splineLength : 0f;

            var location = (Vector3) _splineContainer.EvaluatePosition(t);
            var tangent = (Vector3) _splineContainer.EvaluateTangent(t);
            var up = (Vector3) _splineContainer.EvaluateUpVector(t);

            var scale = Vector2.one;
            float roll = 0f;
            
            var upVector = Vector3.Cross(
                tangent, 
                Vector3.Cross(Quaternion.AngleAxis(roll, Vector3.forward) * up, tangent).normalized
            );
            var rotation = Quaternion.LookRotation(tangent, upVector);

            var position = Vector3.Scale(vertex.position, new Vector3(0, scale.y, scale.x));
            position = Quaternion.AngleAxis(roll, Vector3.right) * position;
            position.x = 0;

            var normal = Quaternion.AngleAxis(roll, Vector3.right) * vertex.normal;
            var q = rotation * Quaternion.Euler(0, -90, 0);

            position = q * position + location;
            normal = q * normal;
            
            return new MeshVertex(vertex.position, vertex.normal, vertex.uv);
        }
    }

}
