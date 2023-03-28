using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Bezier.Utility;
using UnityEngine;

namespace MisterGames.Bezier.Generation {
    
    public struct MeshInfo {

        public Mesh mesh;
        public List<MeshVertex> vertices;
        public int[] triangles;
        public float minX;
        public float length;

        public Vector3 translation;
        public Quaternion rotation;
        public Vector3 scale;

        public MeshInfo Create() {
            var reversed = scale.x < 0;
            if (scale.y < 0) reversed = !reversed;
            if (scale.z < 0) reversed = !reversed;
            triangles = reversed ? MeshUtility.GetReversedTriangles(mesh) : mesh.triangles;

            var i = 0;
            vertices = new List<MeshVertex>(mesh.vertexCount);
            foreach (var vert in mesh.vertices) {
                var transformed = new MeshVertex {
                    position = vert,
                    normal = mesh.normals[i++],
                    uv = Vector2.zero
                }; 
                
                if (rotation != Quaternion.identity) {
                    transformed.position = rotation * transformed.position;
                    transformed.normal = rotation * transformed.normal;
                }
                
                if (scale != Vector3.one) {
                    transformed.position = Vector3.Scale(transformed.position, scale);
                    transformed.normal = Vector3.Scale(transformed.normal, scale);
                }
                
                if (translation != Vector3.zero) {
                    transformed.position += translation;
                }
                
                vertices.Add(transformed);
            }

            minX = float.MaxValue;
            var maxX = float.MinValue;
            foreach (var p in vertices.Select(vert => vert.position)) {
                maxX = Math.Max(maxX, p.x);
                minX = Math.Min(minX, p.x);
            }
            length = Math.Abs(maxX - minX);

            return this;
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var other = (MeshInfo)obj;
            return mesh == other.mesh &&
                translation == other.translation &&
                rotation == other.rotation &&
                scale == other.scale;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(MeshInfo sm1, MeshInfo sm2) {
            return sm1.Equals(sm2);
        }
        
        public static bool operator !=(MeshInfo sm1, MeshInfo sm2) {
            return sm1.Equals(sm2);
        }
        
    }
}
