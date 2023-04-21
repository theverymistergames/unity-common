using MisterGames.Dbg.Draw;
using MisterGames.Interact.Interactives;
using MisterGames.Splines.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Interact.Path {

    public sealed class InteractivePath : MonoBehaviour, IInteractivePath {

        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Interactive _interactive;

        [Header("Bounds")]
        [SerializeField] [Range(0f, 0.5f)] private float _startReserveBound = 0f;
        [SerializeField] [Range(0.5f, 1f)] private float _endReserveBound = 1f;

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

        private void OnStartInteract(IInteractiveUser user) {
            var interactivePathUser = user.Transform.GetComponent<IInteractivePathUser>();

            if (interactivePathUser == null) {
                Debug.LogWarning($"{nameof(InteractivePath)}: interactive user GameObject {user.Transform.name} " +
                                 $"should have component that implements {nameof(IInteractivePathUser)} interface " +
                                 $"in order to interact with {nameof(InteractivePath)}. Interaction was cancelled.");

                user.TryStopInteract(_interactive);
                return;
            }

            _splineContainer.GetNearestPoint(user.Transform.position, out float t);
            t = math.clamp(t, _startReserveBound, _endReserveBound);

            interactivePathUser.OnAttachedToPath(user, this, t);
        }

        private void OnStopInteract(IInteractiveUser user) {
            var interactivePathUser = user.Transform.GetComponent<IInteractivePathUser>();

            if (interactivePathUser == null) {
                Debug.LogWarning($"{nameof(InteractivePath)}: interactive user GameObject {user.Transform.name} " +
                                 $"should have component that implements {nameof(IInteractivePathUser)} interface " +
                                 $"in order to interact with {nameof(InteractivePath)}.");
                return;
            }

            interactivePathUser.OnDetachedFromPath(user, this);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (_splineContainer == null) return;

            var startBoundPosition = _splineContainer.EvaluatePosition(_startReserveBound);
            var endBoundPosition = _splineContainer.EvaluatePosition(_endReserveBound);

            DbgSphere.Create().Radius(0.1f).Color(Color.white).Position(startBoundPosition).Draw();
            DbgSphere.Create().Radius(0.1f).Color(Color.white).Position(endBoundPosition).Draw();
        }
#endif
    }

}
