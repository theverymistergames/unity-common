﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MisterGames.Splines.Utils {
    
    public static class MeshUtils {

        /// <summary>
        /// Returns a mesh with reserved triangles to turn back the face culling.
        /// This is useful when a mesh needs to have a negative scale.
        /// </summary>
        public static int[] GetReversedTriangles(Mesh mesh) {
            int[] triangles = mesh.triangles.ToArray();
            int triangleCount = triangles.Length / 3;

            for (int i = 0; i < triangleCount; i++) {
                (triangles[i * 3], triangles[i * 3 + 1]) = (triangles[i * 3 + 1], triangles[i * 3]);
            }

            return triangles;
        }

        /// <summary>
        /// Returns a mesh similar to the given source plus given optional parameters.
        /// </summary>
        public static void Update(
            Mesh mesh,
            Mesh source,
            IEnumerable<int> triangles = null,
            IEnumerable<Vector3> vertices = null,
            IEnumerable<Vector3> normals = null,
            IEnumerable<Vector2> uv = null,
            IEnumerable<Vector2> uv2 = null,
            IEnumerable<Vector2> uv3 = null,
            IEnumerable<Vector2> uv4 = null,
            IEnumerable<Vector2> uv5 = null,
            IEnumerable<Vector2> uv6 = null,
            IEnumerable<Vector2> uv7 = null,
            IEnumerable<Vector2> uv8 = null
        ) {
            mesh.hideFlags = source.hideFlags;
            mesh.indexFormat = source.indexFormat;
            mesh.triangles = Array.Empty<int>();
            mesh.vertices = vertices == null ? source.vertices : vertices.ToArray();
            mesh.normals = normals == null ? source.normals : normals.ToArray();
            mesh.uv = uv == null? source.uv : uv.ToArray();
            mesh.uv2 = uv2 == null ? source.uv2 : uv2.ToArray();
            mesh.uv3 = uv3 == null ? source.uv3 : uv3.ToArray();
            mesh.uv4 = uv4 == null ? source.uv4 : uv4.ToArray();
            mesh.uv5 = uv5 == null ? source.uv5 : uv5.ToArray();
            mesh.uv6 = uv6 == null ? source.uv6 : uv6.ToArray();
            mesh.uv7 = uv7 == null ? source.uv7 : uv7.ToArray();
            mesh.uv8 = uv8 == null ? source.uv8 : uv8.ToArray();
            mesh.triangles = triangles == null ? source.triangles : triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }
    }
    
}
