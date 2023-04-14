using System;
using MisterGames.Splines.Utils;
using UnityEngine;

namespace MisterGames.Splines.Tools {
    
    public readonly struct MeshInfo : IEquatable<MeshInfo> {

        public readonly Mesh mesh;

        public readonly Vector3 translation;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;

        public readonly MeshVertex[] vertices;
        public readonly int[] triangles;
        public readonly float xMin;
        public readonly float length;

        public MeshInfo(Mesh mesh, Vector3 translation, Quaternion rotation, Vector3 scale) {
            this.mesh = mesh;

            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;

            bool isReversed = scale.x < 0;

            if (scale.y < 0) isReversed = !isReversed;
            if (scale.z < 0) isReversed = !isReversed;

            triangles = isReversed ? MeshUtils.GetReversedTriangles(mesh) : mesh.triangles;
            vertices = new MeshVertex[mesh.vertexCount];

            float minX = 0f;
            float maxX = 0f;

            for (int i = 0; i < mesh.vertices.Length; i++) {
                var position = Vector3.Scale(rotation * mesh.vertices[i], scale) + translation;
                var normal = Vector3.Scale(rotation * mesh.normals[i], scale);

                vertices[i] = new MeshVertex(position, normal, Vector2.zero);

                if (i == 0) {
                    minX = position.x;
                    maxX = position.x;
                    continue;
                }

                if (position.x < minX) minX = position.x;
                if (position.x > maxX) maxX = position.x;
            }

            xMin = minX;
            length = Math.Abs(maxX - minX);
        }

        public override bool Equals(object obj) {
            return obj is MeshInfo other && Equals(other);
        }

        public bool Equals(MeshInfo other) {
            return mesh == other.mesh &&
                   translation == other.translation &&
                   rotation == other.rotation &&
                   scale == other.scale;
        }

        public override int GetHashCode() {
            return HashCode.Combine(mesh, translation, rotation, scale);
        }

        public static bool operator ==(MeshInfo sm1, MeshInfo sm2) {
            return sm1.Equals(sm2);
        }
        
        public static bool operator !=(MeshInfo sm1, MeshInfo sm2) {
            return sm1.Equals(sm2);
        }
    }

}
