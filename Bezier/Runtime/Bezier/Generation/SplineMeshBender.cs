using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Bezier.Objects;
using MisterGames.Bezier.Utility;
using UnityEngine;

namespace MisterGames.Bezier.Generation {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class SplineMeshBender : MonoBehaviour {
        
        private Mesh _result;
        private MeshInfo _source;
        private FillingMode _mode = FillingMode.StretchToInterval;
        private VertexPath _vertexPath;

        private void OnEnable() {
            var meshFilter = GetComponent<MeshFilter>();
            
            if (meshFilter.sharedMesh == null) {
                _result = new Mesh { name = "SplineMesh" };
                meshFilter.sharedMesh = _result;
                return;
            }

            _result = meshFilter.sharedMesh;
        }

        public void SetSourceMesh(MeshInfo sourceMesh, FillingMode fillingMode) {
            _source = sourceMesh;
            _mode = fillingMode;
        }
        
        public void SetInterval(VertexPath vertexPath) {
            _vertexPath = vertexPath;
            Compute();
        }
        
        private  void Compute() {
            switch (_mode) {
                case FillingMode.Once:
                    FillOnce();
                    break;
                case FillingMode.Repeat:
                    FillRepeat();
                    break;
                case FillingMode.StretchToInterval:
                    FillStretch();
                    break;
            }
        }

        private void FillOnce() {
            var bentVertices = new List<MeshVertex>(_source.vertices.Count);
            foreach (var vert in _source.vertices) {
                var distance = Mathf.Clamp(vert.position.x - _source.minX, 0f, _vertexPath.length);
                var bent = GetBent(vert, distance);
                bentVertices.Add(bent);
            }

            MeshUtility.Update(_result,
                _source.mesh,
                _source.triangles,
                bentVertices.Select(b => b.position),
                bentVertices.Select(b => b.normal)
            );
        }

        private void FillRepeat() {
            var intervalLength = _vertexPath.length;
            var repetitionCount = Mathf.FloorToInt(intervalLength / _source.length);
            
            var triangles = new List<int>();
            var uv1 = new List<Vector2>();
            var uv2 = new List<Vector2>();
            var uv3 = new List<Vector2>();
            var uv4 = new List<Vector2>();
            var uv5 = new List<Vector2>();
            var uv6 = new List<Vector2>();
            var uv7 = new List<Vector2>();
            var uv8 = new List<Vector2>();
            
            for (var i = 0; i < repetitionCount; i++) {
                foreach (var index in _source.triangles) {
                    triangles.Add(index + _source.vertices.Count * i);
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

            var bentVertices = new List<MeshVertex>(_source.vertices.Count);
            float offset = 0;
            for (var i = 0; i < repetitionCount; i++) {
                foreach (var vert in _source.vertices) {
                    var distance = vert.position.x - _source.minX + offset;
                    var bent = GetBent(vert, distance);
                    bentVertices.Add(bent);
                }
                
                offset += _source.length;
            }

            MeshUtility.Update(
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
            var bentVertices = new List<MeshVertex>(_source.vertices.Count);
            foreach (var vert in _source.vertices) {
                var distanceRate = _source.length > 0 
                    ? Math.Abs(vert.position.x - _source.minX) / _source.length 
                    : 0f;
                
                var distance = _vertexPath.length * distanceRate;
                var bent = GetBent(vert, distance);
                bentVertices.Add(bent);
            }

            MeshUtility.Update(
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
        
        private MeshVertex GetBent(MeshVertex vert, float distance) {
            var location = _vertexPath.GetPointAtDistance(distance, EndOfPathInstruction.Stop);
            var tangent = _vertexPath.GetDirectionAtDistance(distance, EndOfPathInstruction.Stop);
            var up = _vertexPath.GetNormalAtDistance(distance, EndOfPathInstruction.Stop);
            var scale = Vector2.one;
            var roll = 0f;
            
            var upVector = Vector3.Cross(
                tangent, 
                Vector3.Cross(Quaternion.AngleAxis(roll, Vector3.forward) * up, tangent).normalized
            );
            var rotation = Quaternion.LookRotation(tangent, upVector);

            var res = new MeshVertex { position = vert.position, normal = vert.normal, uv = vert.uv };
            res.position = Vector3.Scale(res.position, new Vector3(0, scale.y, scale.x));
            res.position = Quaternion.AngleAxis(roll, Vector3.right) * res.position;
            res.normal = Quaternion.AngleAxis(roll, Vector3.right) * res.normal;
            res.position.x = 0;
            var q = rotation * Quaternion.Euler(0, -90, 0);
            res.position = q * res.position + location;
            res.normal = q * res.normal;
            
            return res;
        }

        /// <summary>
        /// The mode used by <see cref="SplineMeshBender"/> to bend meshes on the interval.
        /// </summary>
        public enum FillingMode {
            
            /// <summary>
            /// In this mode, source mesh will be placed on the interval by preserving mesh scale.
            /// Vertices that are beyond interval end will be placed on the interval end.
            /// </summary>
            Once,
            
            /// <summary>
            /// In this mode, the mesh will be repeated to fill the interval, preserving
            /// mesh scale.
            /// This filling process will stop when the remaining space is not enough to
            /// place a whole mesh, leading to an empty interval.
            /// </summary>
            Repeat,
            
            /// <summary>
            /// In this mode, the mesh is deformed along the X axis to fill exactly the interval.
            /// </summary>
            StretchToInterval
            
        }
        
    }
}