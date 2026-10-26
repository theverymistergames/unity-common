using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Collisions.Tools {

    public sealed class PrimitiveCollidersGenerator : MonoBehaviour {

        [SerializeField] private MeshRenderer _meshRenderer;

        [Header("Quality")]
        [SerializeField] [Min(1)] private int _maxColliders = 6;
        [SerializeField] private Shapes _shapes = Shapes.Box | Shapes.Sphere | Shapes.Capsule;
        [SerializeField] [Range(8, 64)] private int _resolution = 16;
        [SerializeField] [Range(0f, 0.5f)] private float _tolerance = 0.03f;
        [SerializeField] private bool _useConvexHull = true;

        [Header("Output")]
        [SerializeField] private Transform _parent;
        [SerializeField] private bool _overrideLayer;
        [SerializeField] [Layer] private int _layer;
        [SerializeField] private bool _disableSourceMeshCollider = true;

        [Header("Created")]
        [SerializeField] [ReadOnly] private List<Collider> _createdColliders = new();

        [System.Flags]
        private enum Shapes {
            None = 0,
            Box = 1,
            Sphere = 1 << 1,
            Capsule = 1 << 2,
        }

#if UNITY_EDITOR
        private const string UndoName = "Primitive Colliders";

        private const int RefineIterations = 8;

        private static readonly float[] RadiusPercentiles = { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
        private static readonly float[] BoxTrimPercentiles = { 0f, 0.02f, 0.05f, 0.1f };

        private enum ShapeType {
            Box,
            Sphere,
            Capsule,
        }

        private struct Shape {
            public ShapeType type;
            public Vector3 center;
            public Quaternion rotation;
            public Vector3 size;
            public float radius;
            public float height;
            public float error;
        }

        private sealed class Cluster {
            public readonly List<int> cells;
            public Shape shape;
            public bool canSplit = true;

            public Cluster(List<int> cells) {
                this.cells = cells;
            }
        }

        private sealed class Occupancy {

            private readonly Vector3 _min;
            private readonly float _cellSize;
            private readonly Vector3Int _dims;
            private readonly bool[] _occupied;
            private readonly int[] _stamps;
            private int _stamp;

            public Occupancy(List<Vector3> points, float cellSize) {
                var min = Vector3.positiveInfinity;
                var max = Vector3.negativeInfinity;

                for (int i = 0; i < points.Count; i++) {
                    min = Vector3.Min(min, points[i]);
                    max = Vector3.Max(max, points[i]);
                }

                _min = min;
                _cellSize = cellSize;
                _dims = Vector3Int.RoundToInt((max - min) / cellSize) + Vector3Int.one;
                _occupied = new bool[_dims.x * _dims.y * _dims.z];
                _stamps = new int[_occupied.Length];

                for (int i = 0; i < points.Count; i++) {
                    var cell = Vector3Int.RoundToInt((points[i] - min) / cellSize);
                    cell = Vector3Int.Min(Vector3Int.Max(cell, Vector3Int.zero), _dims - Vector3Int.one);
                    _occupied[(cell.x * _dims.y + cell.y) * _dims.z + cell.z] = true;
                }
            }

            public void BeginUnion() {
                _stamp++;
            }

            public int CountInside(Shape shape) {
                return Count(shape, union: false, out _);
            }

            public int CountInsideUnion(Shape shape, out int newCovered) {
                return Count(shape, union: true, out newCovered);
            }

            private int Count(Shape shape, bool union, out int newCovered) {
                newCovered = 0;
                GetBounds(shape, out var boundsMin, out var boundsMax);

                var lo = Vector3Int.Max(Vector3Int.CeilToInt((boundsMin - _min) / _cellSize), Vector3Int.zero);
                var hi = Vector3Int.Min(Vector3Int.FloorToInt((boundsMax - _min) / _cellSize), _dims - Vector3Int.one);
                int count = 0;

                for (int x = lo.x; x <= hi.x; x++) {
                    for (int y = lo.y; y <= hi.y; y++) {
                        for (int z = lo.z; z <= hi.z; z++) {
                            int index = (x * _dims.y + y) * _dims.z + z;
                            if (!_occupied[index] || !Contains(shape, _min + new Vector3(x, y, z) * _cellSize)) continue;

                            count++;

                            if (!union || _stamps[index] == _stamp) continue;

                            _stamps[index] = _stamp;
                            newCovered++;
                        }
                    }
                }

                return count;
            }
        }

        private Occupancy _occupancy;

        private const double DebugPointsDuration = 5d;
        private static readonly Color DebugPointsColor = new(0f, 1f, 0.5f, 0.6f);

        private readonly List<Vector3> _debugPoints = new();
        private float _debugCellSize;
        private double _debugPointsEndTime;

        private void Reset() {
            _meshRenderer = GetComponent<MeshRenderer>();
            _parent = transform;
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void SampleInsidePoints() {
            if (!TrySamplePoints(out var points, out float cellSize, out int gridCells)) return;

            _debugPoints.Clear();
            _debugPoints.AddRange(points);
            _debugCellSize = cellSize;
            _debugPointsEndTime = EditorApplication.timeSinceStartup + DebugPointsDuration;

            EditorApplication.update -= UpdateDebugPoints;
            EditorApplication.update += UpdateDebugPoints;
            SceneView.RepaintAll();

            Debug.Log($"{nameof(PrimitiveCollidersGenerator)}[{name}]: sampled points {points.Count}/{gridCells}, cell size {cellSize:0.###}");
        }

        private void UpdateDebugPoints() {
            if (this != null && EditorApplication.timeSinceStartup < _debugPointsEndTime) return;

            EditorApplication.update -= UpdateDebugPoints;
            _debugPoints.Clear();
            SceneView.RepaintAll();
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void CreatePrimitiveColliders() {
            if (_shapes == Shapes.None) {
                Debug.LogWarning($"{nameof(PrimitiveCollidersGenerator)}[{name}]: at least one shape type must be allowed.");
                return;
            }

            if (!TrySamplePoints(out var points, out float cellSize, out int gridCells)) return;

            var source = _meshRenderer.transform;
            var clusters = BuildClusters(points, cellSize, out float error);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            DestroyCreatedObjects();

            source.TryGetComponent(out MeshCollider sourceCollider);
            CreateColliders(clusters, source, sourceCollider);

            if (_disableSourceMeshCollider && sourceCollider != null) {
                Undo.RecordObject(sourceCollider, UndoName);
                sourceCollider.enabled = false;
            }

            Undo.CollapseUndoOperations(undoGroup);

            string stopReason = clusters.Count >= _maxColliders ? "max colliders"
                : error <= _tolerance * points.Count ? "tolerance reached"
                : "no better split";

            Debug.Log($"{nameof(PrimitiveCollidersGenerator)}[{name}]: created {clusters.Count} colliders ({stopReason}), " +
                      $"error {error / points.Count:P1}, sampled points {points.Count}/{gridCells}, cell size {cellSize:0.###}");
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void DestroyPrimitiveColliders() {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);

            DestroyCreatedObjects();

            if (_disableSourceMeshCollider && _meshRenderer != null && _meshRenderer.TryGetComponent(out MeshCollider sourceCollider)) {
                Undo.RecordObject(sourceCollider, UndoName);
                sourceCollider.enabled = true;
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        private bool TrySamplePoints(out List<Vector3> points, out float cellSize, out int gridCells) {
            if (_meshRenderer == null ||
                !_meshRenderer.TryGetComponent(out MeshFilter meshFilter) ||
                meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"{nameof(PrimitiveCollidersGenerator)}[{name}]: mesh renderer with mesh filter and mesh is required.");
                points = null;
                cellSize = 0f;
                gridCells = 0;
                return false;
            }

            var mesh = meshFilter.sharedMesh;
            var scale = _meshRenderer.transform.lossyScale;

            points = _useConvexHull
                ? SampleConvexHullPoints(mesh, scale, _resolution, out cellSize, out gridCells)
                : SampleMeshPoints(mesh, scale, _resolution, out cellSize, out gridCells);

            if (points.Count == 0) {
                Debug.LogWarning($"{nameof(PrimitiveCollidersGenerator)}[{name}]: no points found inside mesh {mesh.name}.");
                return false;
            }

            return true;
        }

        private void OnDrawGizmos() {
            if (_meshRenderer == null || _debugPoints.Count == 0) return;

            var source = _meshRenderer.transform;
            var size = 0.5f * _debugCellSize * Vector3.one;

            Gizmos.matrix = Matrix4x4.TRS(source.position, source.rotation, Vector3.one);
            Gizmos.color = DebugPointsColor;

            for (int i = 0; i < _debugPoints.Count; i++) {
                Gizmos.DrawCube(_debugPoints[i], size);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DestroyCreatedObjects() {
            Undo.RecordObject(this, UndoName);

            for (int i = 0; i < _createdColliders.Count; i++) {
                var collider = _createdColliders[i];
                if (collider == null) continue;

                var go = collider.gameObject;
                bool isOnlyComponent = go.GetComponents<Component>().Length == 2 && go.transform.childCount == 0;

                if (isOnlyComponent) Undo.DestroyObjectImmediate(go);
                else Undo.DestroyObjectImmediate(collider);
            }

            _createdColliders.Clear();
        }

        private void CreateColliders(List<Cluster> clusters, Transform source, MeshCollider sourceCollider) {
            var parent = _parent != null ? _parent : transform;
            int layer = _overrideLayer ? _layer : parent.gameObject.layer;

            Undo.RecordObject(this, UndoName);

            for (int i = 0; i < clusters.Count; i++) {
                var shape = clusters[i].shape;

                var go = new GameObject($"{source.name}_{shape.type}_{i}") { layer = layer };
                Undo.RegisterCreatedObjectUndo(go, UndoName);

                var t = go.transform;
                t.SetParent(parent, worldPositionStays: false);
                t.SetPositionAndRotation(source.position + source.rotation * shape.center, source.rotation * shape.rotation);
                t.localScale = Vector3.one;

                var lossyScale = Abs(t.lossyScale);
                Collider collider;

                switch (shape.type) {
                    case ShapeType.Box:
                        var box = go.AddComponent<BoxCollider>();
                        box.size = new Vector3(shape.size.x / lossyScale.x, shape.size.y / lossyScale.y, shape.size.z / lossyScale.z);
                        collider = box;
                        break;

                    case ShapeType.Sphere:
                        var sphere = go.AddComponent<SphereCollider>();
                        sphere.radius = shape.radius / Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
                        collider = sphere;
                        break;

                    case ShapeType.Capsule:
                        var capsule = go.AddComponent<CapsuleCollider>();
                        capsule.direction = 1;
                        capsule.radius = shape.radius / Mathf.Max(lossyScale.x, lossyScale.z);
                        capsule.height = shape.height / lossyScale.y;
                        collider = capsule;
                        break;

                    default:
                        continue;
                }

                if (sourceCollider != null) {
                    collider.sharedMaterial = sourceCollider.sharedMaterial;
                    collider.isTrigger = sourceCollider.isTrigger;
                }

                _createdColliders.Add(collider);
            }
        }

        private static List<Vector3> SampleMeshPoints(Mesh mesh, Vector3 scale, int resolution, out float cellSize, out int gridCells) {
            var points = new List<Vector3>();
            gridCells = 0;

            var bounds = mesh.bounds;
            var center = Vector3.Scale(bounds.center, scale);
            var size = Abs(Vector3.Scale(bounds.size, scale));

            cellSize = Mathf.Max(size.x, size.y, size.z) / resolution;
            if (cellSize <= 0f) return points;

            var dims = Vector3Int.Max(Vector3Int.one, Vector3Int.CeilToInt(size / cellSize)) + Vector3Int.one * 2;
            var gridMin = center - (Vector3) dims * (0.5f * cellSize);
            gridCells = dims.x * dims.y * dims.z;

            var surfaceCells = new bool[gridCells];

            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = Vector3.Scale(vertices[i], scale);
            }

            var triangles = mesh.triangles;
            float step = 0.4f * cellSize;

            for (int i = 0; i + 2 < triangles.Length; i += 3) {
                var a = vertices[triangles[i]];
                var ab = vertices[triangles[i + 1]] - a;
                var ac = vertices[triangles[i + 2]] - a;

                float maxEdge = Mathf.Max(ab.magnitude, ac.magnitude, (ac - ab).magnitude);
                int steps = Mathf.Max(1, Mathf.CeilToInt(maxEdge / step));

                for (int u = 0; u <= steps; u++) {
                    for (int v = 0; v <= steps - u; v++) {
                        var p = a + ab * ((float) u / steps) + ac * ((float) v / steps);
                        surfaceCells[GetCellIndex(p, gridMin, cellSize, dims)] = true;
                    }
                }
            }

            var insideVotes = new byte[gridCells];
            var validVotes = new byte[gridCells];

            for (int axis = 0; axis < 3; axis++) {
                CastWindingRays(vertices, triangles, gridMin, cellSize, dims, axis, insideVotes, validVotes);
            }

            for (int x = 0; x < dims.x; x++) {
                for (int y = 0; y < dims.y; y++) {
                    for (int z = 0; z < dims.z; z++) {
                        int index = (x * dims.y + y) * dims.z + z;
                        if (!surfaceCells[index] && insideVotes[index] * 2 <= validVotes[index]) continue;

                        points.Add(gridMin + (new Vector3(x, y, z) + 0.5f * Vector3.one) * cellSize);
                    }
                }
            }

            return points;
        }

        private static void CastWindingRays(
            Vector3[] vertices, int[] triangles, Vector3 gridMin, float cellSize, Vector3Int dims, int axis,
            byte[] insideVotes, byte[] validVotes)
        {
            int u = (axis + 1) % 3;
            int v = (axis + 2) % 3;
            int countU = dims[u];
            int countV = dims[v];
            var columns = new List<Vector2>[countU * countV];

            for (int t = 0; t + 2 < triangles.Length; t += 3) {
                var a = vertices[triangles[t]];
                var b = vertices[triangles[t + 1]];
                var c = vertices[triangles[t + 2]];

                float area = (b[u] - a[u]) * (c[v] - a[v]) - (c[u] - a[u]) * (b[v] - a[v]);
                if (Mathf.Abs(area) < 1e-12f) continue;

                float sign = area > 0f ? 1f : -1f;

                int minI = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a[u], b[u], c[u]) - gridMin[u]) / cellSize - 0.5f));
                int maxI = Mathf.Min(countU - 1, Mathf.CeilToInt((Mathf.Max(a[u], b[u], c[u]) - gridMin[u]) / cellSize - 0.5f));
                int minJ = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(a[v], b[v], c[v]) - gridMin[v]) / cellSize - 0.5f));
                int maxJ = Mathf.Min(countV - 1, Mathf.CeilToInt((Mathf.Max(a[v], b[v], c[v]) - gridMin[v]) / cellSize - 0.5f));

                for (int i = minI; i <= maxI; i++) {
                    float pu = gridMin[u] + (i + 0.5f) * cellSize;

                    for (int j = minJ; j <= maxJ; j++) {
                        float pv = gridMin[v] + (j + 0.5f) * cellSize;

                        float w0 = ((b[u] - pu) * (c[v] - pv) - (c[u] - pu) * (b[v] - pv)) / area;
                        float w1 = ((c[u] - pu) * (a[v] - pv) - (a[u] - pu) * (c[v] - pv)) / area;
                        float w2 = 1f - w0 - w1;
                        if (w0 < 0f || w1 < 0f || w2 < 0f) continue;

                        float depth = w0 * a[axis] + w1 * b[axis] + w2 * c[axis];
                        (columns[i * countV + j] ??= new List<Vector2>()).Add(new Vector2(depth, sign));
                    }
                }
            }

            var cell = Vector3Int.zero;

            for (int i = 0; i < countU; i++) {
                for (int j = 0; j < countV; j++) {
                    var hits = columns[i * countV + j];
                    int totalWinding = 0;

                    if (hits != null) {
                        hits.Sort((x, y) => x.x.CompareTo(y.x));

                        for (int h = 0; h < hits.Count; h++) {
                            totalWinding += (int) hits[h].y;
                        }
                    }

                    if (totalWinding != 0) continue;

                    int hitIndex = 0;
                    int winding = 0;

                    cell[u] = i;
                    cell[v] = j;

                    for (int k = 0; k < dims[axis]; k++) {
                        float depth = gridMin[axis] + (k + 0.5f) * cellSize;

                        while (hits != null && hitIndex < hits.Count && hits[hitIndex].x < depth) {
                            winding += (int) hits[hitIndex].y;
                            hitIndex++;
                        }

                        cell[axis] = k;
                        int index = (cell.x * dims.y + cell.y) * dims.z + cell.z;

                        validVotes[index]++;
                        if (winding != 0) insideVotes[index]++;
                    }
                }
            }
        }

        private static int GetCellIndex(Vector3 point, Vector3 gridMin, float cellSize, Vector3Int dims) {
            var cell = Vector3Int.FloorToInt((point - gridMin) / cellSize);
            cell = Vector3Int.Min(Vector3Int.Max(cell, Vector3Int.zero), dims - Vector3Int.one);
            return (cell.x * dims.y + cell.y) * dims.z + cell.z;
        }

        private static List<Vector3> SampleConvexHullPoints(Mesh mesh, Vector3 scale, int resolution, out float cellSize, out int gridCells) {
            var points = new List<Vector3>();
            gridCells = 0;

            var bounds = mesh.bounds;
            var center = Vector3.Scale(bounds.center, scale);
            var size = Abs(Vector3.Scale(bounds.size, scale));

            cellSize = Mathf.Max(size.x, size.y, size.z) / resolution;
            if (cellSize <= 0f) return points;

            var dims = Vector3Int.Max(Vector3Int.one, Vector3Int.CeilToInt(size / cellSize));
            var origin = center - (Vector3) dims * (0.5f * cellSize) + Vector3.one * (0.5f * cellSize);
            gridCells = dims.x * dims.y * dims.z;
            float sqrEpsilon = cellSize * cellSize * 1e-6f;

            var go = new GameObject($"{nameof(PrimitiveCollidersGenerator)}_Temp") { hideFlags = HideFlags.HideAndDontSave };

            try {
                go.transform.localScale = scale;

                var collider = go.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = true;

                Physics.SyncTransforms();

                for (int x = 0; x < dims.x; x++) {
                    for (int y = 0; y < dims.y; y++) {
                        for (int z = 0; z < dims.z; z++) {
                            var p = origin + new Vector3(x, y, z) * cellSize;
                            if ((collider.ClosestPoint(p) - p).sqrMagnitude <= sqrEpsilon) points.Add(p);
                        }
                    }
                }
            }
            finally {
                DestroyImmediate(go);
            }

            return points;
        }

        private List<Cluster> BuildClusters(List<Vector3> points, float cellSize, out float totalError) {
            var allCells = new List<int>(points.Count);
            for (int i = 0; i < points.Count; i++) {
                allCells.Add(i);
            }

            _occupancy = new Occupancy(points, cellSize);

            var root = new Cluster(allCells);
            root.shape = FitShape(points, root.cells, cellSize);

            var clusters = new List<Cluster> { root };
            float maxError = _tolerance * points.Count;
            totalError = GetTotalError(clusters, points, cellSize);

            while (clusters.Count < _maxColliders && totalError > maxError) {
                int worstIndex = -1;

                for (int i = 0; i < clusters.Count; i++) {
                    var cluster = clusters[i];
                    if (cluster.canSplit && (worstIndex < 0 || cluster.shape.error > clusters[worstIndex].shape.error)) {
                        worstIndex = i;
                    }
                }

                if (worstIndex < 0) break;

                var worst = clusters[worstIndex];

                if (!TrySplit(points, worst, cellSize, out var a, out var b)) {
                    worst.canSplit = false;
                    continue;
                }

                var candidate = new List<Cluster>(clusters);
                candidate[worstIndex] = a;
                candidate.Add(b);

                float candidateError = GetTotalError(candidate, points, cellSize);

                if (candidateError >= totalError) {
                    worst.canSplit = false;
                    continue;
                }

                clusters = candidate;
                totalError = candidateError;
            }

            Refine(clusters, points, cellSize, ref totalError);
            _occupancy = null;

            return clusters;
        }

        private void Refine(List<Cluster> clusters, List<Vector3> points, float cellSize, ref float totalError) {
            for (int iteration = 0; iteration < RefineIterations; iteration++) {
                var assigned = new List<int>[clusters.Count];
                for (int c = 0; c < clusters.Count; c++) {
                    assigned[c] = new List<int>();
                }

                for (int i = 0; i < points.Count; i++) {
                    int bestCluster = 0;
                    float bestDistance = float.MaxValue;

                    for (int c = 0; c < clusters.Count; c++) {
                        float distance = GetSignedDistance(clusters[c].shape, points[i]);
                        if (distance >= bestDistance) continue;

                        bestDistance = distance;
                        bestCluster = c;
                    }

                    assigned[bestCluster].Add(i);
                }

                var refined = new List<Cluster>(clusters.Count);

                for (int c = 0; c < clusters.Count; c++) {
                    if (assigned[c].Count == 0) continue;

                    var cluster = new Cluster(assigned[c]);
                    cluster.shape = FitShape(points, cluster.cells, cellSize);
                    refined.Add(cluster);
                }

                float error = GetTotalError(refined, points, cellSize);
                if (error >= totalError) break;

                clusters.Clear();
                clusters.AddRange(refined);
                totalError = error;
            }
        }

        private float GetTotalError(List<Cluster> clusters, List<Vector3> points, float cellSize) {
            float cellVolume = cellSize * cellSize * cellSize;
            float falsePositive = 0f;
            int covered = 0;

            _occupancy.BeginUnion();

            for (int c = 0; c < clusters.Count; c++) {
                var shape = clusters[c].shape;
                int coveredPoints = _occupancy.CountInsideUnion(shape, out int newCovered);

                covered += newCovered;
                falsePositive += Mathf.Max(0f, GetVolume(shape) / cellVolume - coveredPoints);
            }

            return falsePositive + points.Count - covered;
        }

        private bool TrySplit(List<Vector3> points, Cluster cluster, float cellSize, out Cluster bestA, out Cluster bestB) {
            bestA = null;
            bestB = null;

            var cells = cluster.cells;
            if (cells.Count < 2) return false;

            var centroid = GetCentroid(points, cells);
            var axes = new[] {
                Vector3.right,
                Vector3.up,
                Vector3.forward,
                GetPrincipalRotation(points, cells, centroid) * Vector3.right,
            };

            var projections = new float[cells.Count];
            var sorted = new float[cells.Count];
            float bestError = float.MaxValue;

            for (int i = 0; i < axes.Length; i++) {
                var axis = axes[i];
                float mean = 0f;

                for (int j = 0; j < cells.Count; j++) {
                    float projection = Vector3.Dot(points[cells[j]] - centroid, axis);
                    projections[j] = projection;
                    sorted[j] = projection;
                    mean += projection;
                }

                System.Array.Sort(sorted);
                mean /= cells.Count;

                for (int s = 0; s < 2; s++) {
                    float threshold = s == 0 ? sorted[cells.Count / 2] : mean;

                    var cellsA = new List<int>();
                    var cellsB = new List<int>();

                    for (int j = 0; j < cells.Count; j++) {
                        if (projections[j] < threshold) cellsA.Add(cells[j]);
                        else cellsB.Add(cells[j]);
                    }

                    if (cellsA.Count == 0 || cellsB.Count == 0) continue;

                    var a = new Cluster(cellsA);
                    var b = new Cluster(cellsB);
                    a.shape = FitShape(points, cellsA, cellSize);
                    b.shape = FitShape(points, cellsB, cellSize);

                    float error = a.shape.error + b.shape.error;
                    if (error >= bestError) continue;

                    bestError = error;
                    bestA = a;
                    bestB = b;
                }
            }

            return bestA != null;
        }

        private Shape FitShape(List<Vector3> points, List<int> cells, float cellSize) {
            var centroid = GetCentroid(points, cells);
            var principalRotation = GetPrincipalRotation(points, cells, centroid);
            var best = new Shape { error = float.MaxValue };

            if ((_shapes & Shapes.Box) != 0) {
                FitBoxes(ref best, points, cells, Quaternion.identity, cellSize);
                FitBoxes(ref best, points, cells, principalRotation, cellSize);
            }

            if ((_shapes & Shapes.Sphere) != 0) {
                var distances = new float[cells.Count];
                for (int i = 0; i < cells.Count; i++) {
                    distances[i] = Vector3.Distance(points[cells[i]], centroid);
                }

                System.Array.Sort(distances);

                for (int i = 0; i < RadiusPercentiles.Length; i++) {
                    int index = GetPercentileIndex(RadiusPercentiles[i], cells.Count);

                    var sphere = new Shape {
                        type = ShapeType.Sphere,
                        center = centroid,
                        rotation = Quaternion.identity,
                        radius = distances[index] + 0.5f * cellSize,
                    };

                    TryReplaceBest(ref best, sphere, points, cells, cellSize);
                }
            }

            if ((_shapes & Shapes.Capsule) != 0) {
                FitCapsules(ref best, points, cells, centroid, Vector3.right, cellSize);
                FitCapsules(ref best, points, cells, centroid, Vector3.up, cellSize);
                FitCapsules(ref best, points, cells, centroid, Vector3.forward, cellSize);
                FitCapsules(ref best, points, cells, centroid, principalRotation * Vector3.right, cellSize);
            }

            return best;
        }

        private void FitCapsules(ref Shape best, List<Vector3> points, List<int> cells, Vector3 centroid, Vector3 axis, float cellSize) {
            var radialDistances = new float[cells.Count];
            float minProjection = float.MaxValue;
            float maxProjection = float.MinValue;

            for (int i = 0; i < cells.Count; i++) {
                var d = points[cells[i]] - centroid;
                float projection = Vector3.Dot(d, axis);

                minProjection = Mathf.Min(minProjection, projection);
                maxProjection = Mathf.Max(maxProjection, projection);
                radialDistances[i] = (d - projection * axis).magnitude;
            }

            System.Array.Sort(radialDistances);

            float halfCell = 0.5f * cellSize;
            minProjection -= halfCell;
            maxProjection += halfCell;

            var rotation = Quaternion.FromToRotation(Vector3.up, axis);

            for (int i = 0; i < RadiusPercentiles.Length; i++) {
                float radius = radialDistances[GetPercentileIndex(RadiusPercentiles[i], cells.Count)] + halfCell;
                float start = minProjection + radius;
                float end = maxProjection - radius;

                if (start > end) start = end = 0.5f * (minProjection + maxProjection);

                var capsule = new Shape {
                    type = ShapeType.Capsule,
                    center = centroid + 0.5f * (start + end) * axis,
                    rotation = rotation,
                    radius = radius,
                    height = end - start + 2f * radius,
                };

                TryReplaceBest(ref best, capsule, points, cells, cellSize);
            }
        }

        private static int GetPercentileIndex(float percentile, int count) {
            return Mathf.Clamp(Mathf.CeilToInt(percentile * count) - 1, 0, count - 1);
        }

        private void TryReplaceBest(ref Shape best, Shape candidate, List<Vector3> points, List<int> cells, float cellSize) {
            candidate.error = GetError(candidate, points, cells, cellSize);
            if (candidate.error < best.error) best = candidate;
        }

        private void FitBoxes(ref Shape best, List<Vector3> points, List<int> cells, Quaternion rotation, float cellSize) {
            var inverse = Quaternion.Inverse(rotation);
            var xs = new float[cells.Count];
            var ys = new float[cells.Count];
            var zs = new float[cells.Count];

            for (int i = 0; i < cells.Count; i++) {
                var local = inverse * points[cells[i]];
                xs[i] = local.x;
                ys[i] = local.y;
                zs[i] = local.z;
            }

            System.Array.Sort(xs);
            System.Array.Sort(ys);
            System.Array.Sort(zs);

            for (int i = 0; i < BoxTrimPercentiles.Length; i++) {
                int lo = GetPercentileIndex(BoxTrimPercentiles[i], cells.Count);
                int hi = GetPercentileIndex(1f - BoxTrimPercentiles[i], cells.Count);

                var min = new Vector3(xs[lo], ys[lo], zs[lo]);
                var max = new Vector3(xs[hi], ys[hi], zs[hi]);

                var box = new Shape {
                    type = ShapeType.Box,
                    center = rotation * (0.5f * (min + max)),
                    rotation = rotation,
                    size = max - min + Vector3.one * cellSize,
                };

                TryReplaceBest(ref best, box, points, cells, cellSize);
            }
        }

        private float GetError(Shape shape, List<Vector3> points, List<int> cells, float cellSize) {
            int coveredCells = 0;
            for (int i = 0; i < cells.Count; i++) {
                if (Contains(shape, points[cells[i]])) coveredCells++;
            }

            int coveredPoints = _occupancy.CountInside(shape);

            float falsePositive = Mathf.Max(0f, GetVolume(shape) / (cellSize * cellSize * cellSize) - coveredPoints);
            float falseNegative = cells.Count - coveredCells;

            return falsePositive + falseNegative;
        }

        private static void GetBounds(Shape shape, out Vector3 min, out Vector3 max) {
            Vector3 extents;

            switch (shape.type) {
                case ShapeType.Box:
                    var half = 0.5f * shape.size;
                    extents = Abs(shape.rotation * Vector3.right) * half.x +
                              Abs(shape.rotation * Vector3.up) * half.y +
                              Abs(shape.rotation * Vector3.forward) * half.z;
                    break;

                case ShapeType.Sphere:
                    extents = Vector3.one * shape.radius;
                    break;

                case ShapeType.Capsule:
                    float halfSegment = Mathf.Max(0f, 0.5f * shape.height - shape.radius);
                    extents = Abs(shape.rotation * Vector3.up) * halfSegment + Vector3.one * shape.radius;
                    break;

                default:
                    extents = Vector3.zero;
                    break;
            }

            min = shape.center - extents;
            max = shape.center + extents;
        }

        private static float GetVolume(Shape shape) {
            float r = shape.radius;

            return shape.type switch {
                ShapeType.Box => shape.size.x * shape.size.y * shape.size.z,
                ShapeType.Sphere => 4f / 3f * Mathf.PI * r * r * r,
                ShapeType.Capsule => Mathf.PI * r * r * (shape.height - 2f * r) + 4f / 3f * Mathf.PI * r * r * r,
                _ => 0f,
            };
        }

        private static float GetSignedDistance(Shape shape, Vector3 point) {
            switch (shape.type) {
                case ShapeType.Box:
                    var local = Quaternion.Inverse(shape.rotation) * (point - shape.center);
                    var q = Abs(local) - 0.5f * shape.size;
                    return Vector3.Max(q, Vector3.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y, q.z), 0f);

                case ShapeType.Sphere:
                    return (point - shape.center).magnitude - shape.radius;

                case ShapeType.Capsule:
                    var capsuleLocal = Quaternion.Inverse(shape.rotation) * (point - shape.center);
                    float halfSegment = Mathf.Max(0f, 0.5f * shape.height - shape.radius);
                    var closest = new Vector3(0f, Mathf.Clamp(capsuleLocal.y, -halfSegment, halfSegment), 0f);
                    return (capsuleLocal - closest).magnitude - shape.radius;

                default:
                    return float.MaxValue;
            }
        }

        private static bool Contains(Shape shape, Vector3 point) {
            switch (shape.type) {
                case ShapeType.Box:
                    var local = Quaternion.Inverse(shape.rotation) * (point - shape.center);
                    var half = 0.5f * shape.size;
                    return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;

                case ShapeType.Sphere:
                    return (point - shape.center).sqrMagnitude <= shape.radius * shape.radius;

                case ShapeType.Capsule:
                    var capsuleLocal = Quaternion.Inverse(shape.rotation) * (point - shape.center);
                    float halfSegment = Mathf.Max(0f, 0.5f * shape.height - shape.radius);
                    var closest = new Vector3(0f, Mathf.Clamp(capsuleLocal.y, -halfSegment, halfSegment), 0f);
                    return (capsuleLocal - closest).sqrMagnitude <= shape.radius * shape.radius;

                default:
                    return false;
            }
        }

        private static Vector3 GetCentroid(List<Vector3> points, List<int> cells) {
            var sum = Vector3.zero;
            for (int i = 0; i < cells.Count; i++) {
                sum += points[cells[i]];
            }

            return sum / cells.Count;
        }

        private static Quaternion GetPrincipalRotation(List<Vector3> points, List<int> cells, Vector3 centroid) {
            var m = new float[3, 3];

            for (int i = 0; i < cells.Count; i++) {
                var d = points[cells[i]] - centroid;
                m[0, 0] += d.x * d.x;
                m[0, 1] += d.x * d.y;
                m[0, 2] += d.x * d.z;
                m[1, 1] += d.y * d.y;
                m[1, 2] += d.y * d.z;
                m[2, 2] += d.z * d.z;
            }

            m[1, 0] = m[0, 1];
            m[2, 0] = m[0, 2];
            m[2, 1] = m[1, 2];

            var v = new float[3, 3];
            JacobiEigen(m, v);

            int first = 0;
            for (int i = 1; i < 3; i++) {
                if (m[i, i] > m[first, first]) first = i;
            }

            int second = (first + 1) % 3;
            int third = (first + 2) % 3;
            if (m[third, third] > m[second, second]) second = third;

            var axisX = new Vector3(v[0, first], v[1, first], v[2, first]).normalized;
            var axisY = new Vector3(v[0, second], v[1, second], v[2, second]).normalized;
            var axisZ = Vector3.Cross(axisX, axisY);

            return axisZ.sqrMagnitude > 0f ? Quaternion.LookRotation(axisZ, axisY) : Quaternion.identity;
        }

        private static void JacobiEigen(float[,] a, float[,] v) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    v[i, j] = i == j ? 1f : 0f;
                }
            }

            for (int sweep = 0; sweep < 32; sweep++) {
                float off = a[0, 1] * a[0, 1] + a[0, 2] * a[0, 2] + a[1, 2] * a[1, 2];
                if (off < 1e-12f) return;

                for (int p = 0; p < 2; p++) {
                    for (int q = p + 1; q < 3; q++) {
                        if (Mathf.Abs(a[p, q]) < 1e-12f) continue;

                        float theta = (a[q, q] - a[p, p]) / (2f * a[p, q]);
                        float t = Mathf.Sign(theta) / (Mathf.Abs(theta) + Mathf.Sqrt(theta * theta + 1f));
                        float c = 1f / Mathf.Sqrt(t * t + 1f);
                        float s = t * c;

                        for (int k = 0; k < 3; k++) {
                            float akp = a[k, p];
                            float akq = a[k, q];
                            a[k, p] = c * akp - s * akq;
                            a[k, q] = s * akp + c * akq;
                        }

                        for (int k = 0; k < 3; k++) {
                            float apk = a[p, k];
                            float aqk = a[q, k];
                            a[p, k] = c * apk - s * aqk;
                            a[q, k] = s * apk + c * aqk;
                        }

                        for (int k = 0; k < 3; k++) {
                            float vkp = v[k, p];
                            float vkq = v[k, q];
                            v[k, p] = c * vkp - s * vkq;
                            v[k, q] = s * vkp + c * vkq;
                        }
                    }
                }
            }
        }

        private static Vector3 Abs(Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
#endif
    }

}
