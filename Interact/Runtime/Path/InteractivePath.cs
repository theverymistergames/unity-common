using MisterGames.Splines.Utils;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Interact.Path {

    public sealed class InteractivePath : MonoBehaviour, IInteractivePath {

        [SerializeField] private SplineContainer _splineContainer;

        public float GetNearestPathPoint(Vector3 position) {
            _splineContainer.GetNearestPoint(position, out float t);
            return t;
        }

        public float MoveAlongPath(float t, Vector3 motion) {
            return _splineContainer.MoveAlongSpline(motion, t);
        }

        public void Evaluate(float t, out Vector3 position, out Vector3 tangent, out Vector3 normal) {
            _splineContainer.Evaluate(t, out var pos, out var tan, out var norm);
            position = pos;
            tangent = tan;
            normal = norm;
        }
    }

}
