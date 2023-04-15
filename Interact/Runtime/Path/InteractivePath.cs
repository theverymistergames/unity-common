using MisterGames.Dbg.Draw;
using MisterGames.Interact.Core;
using MisterGames.Splines.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Interact.Path {

    public sealed class InteractivePath : MonoBehaviour, IInteractivePath {

        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Interactive _interactive;

        [Header("Bounds")]
        [SerializeField] [Min(0f)] private float _startReserveBoundLength;
        [SerializeField] [Min(0f)] private float _endReserveBoundLength;

        public void Evaluate(float t, out Vector3 position, out Vector3 tangent, out Vector3 normal) {
            _splineContainer.Evaluate(t, out var pos, out var tang, out var norm);
            position = pos;
            tangent = tang;
            normal = norm;
        }

        public float MoveAlongPath(float t, Vector3 motion) {
            return _splineContainer.MoveAlongSpline(motion, t);
        }

        private  void OnEnable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStartInteract += OnStartInteract;

            _interactive.OnStopInteract -= OnStopInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
        }

        private void OnStartInteract(IInteractiveUser user, Vector3 hitPoint) {
            var interactivePathUser = user.GameObject.GetComponent<IInteractivePathUser>();

            if (interactivePathUser == null) {
                Debug.LogWarning($"{nameof(InteractivePath)}: interactive user GameObject {user.GameObject.name} " +
                                 $"should have component that implements {nameof(IInteractivePathUser)} interface " +
                                 $"in order to interact with {nameof(InteractivePath)}. Interaction was cancelled.");

                user.StopInteract();
                return;
            }

            float tStart = GetClosestSplineInterpolation(hitPoint);
            interactivePathUser.OnAttachedToPath(this, tStart);
        }

        private void OnStopInteract(IInteractiveUser user) {
            var interactivePathUser = user.GameObject.GetComponent<IInteractivePathUser>();

            if (interactivePathUser == null) {
                Debug.LogWarning($"{nameof(InteractivePath)}: interactive user GameObject {user.GameObject.name} " +
                                 $"should have component that implements {nameof(IInteractivePathUser)} interface " +
                                 $"in order to interact with {nameof(InteractivePath)}.");
                return;
            }

            interactivePathUser.OnDetachedFromPath();
        }

        private float GetClosestSplineInterpolation(Vector3 position) {
            float splineLength = _splineContainer.CalculateLength();
            if (splineLength <= 0f) return 0f;

            _splineContainer.GetNearestPoint(position, out float t);

            float inverseSplineLength = 1f / splineLength;
            float startReserveT = math.clamp(_startReserveBoundLength * inverseSplineLength, 0f, 0.5f);
            float endReserveT = math.clamp((splineLength - _endReserveBoundLength) * inverseSplineLength, 0.5f, 1f);

            return math.clamp(t, startReserveT, endReserveT);
        }

        private void OnDrawGizmosSelected() {
            if (_splineContainer == null) return;

            float splineLength = _splineContainer.CalculateLength();
            if (splineLength <= 0f) return;

            float inverseSplineLength = 1f / splineLength;
            float startReserveT = math.clamp(_startReserveBoundLength * inverseSplineLength, 0f, 0.5f);
            float endReserveT = math.clamp((splineLength - _endReserveBoundLength) * inverseSplineLength, 0.5f, 1f);

            var startBoundPosition = _splineContainer.EvaluatePosition(startReserveT);
            var endBoundPosition = _splineContainer.EvaluatePosition(endReserveT);

            DbgSphere.Create().Radius(0.1f).Color(Color.white).Position(startBoundPosition).Draw();
            DbgSphere.Create().Radius(0.1f).Color(Color.white).Position(endBoundPosition).Draw();
        }
    }

}
