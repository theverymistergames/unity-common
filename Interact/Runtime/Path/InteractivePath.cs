using System;
using MisterGames.Dbg.Draw;
using MisterGames.Splines.Utils;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Interact.Path {

    public sealed class InteractivePath : MonoBehaviour, IInteractivePath {

        [SerializeField] private SplineContainer _splineContainer;

        [Header("Path Settings")]
        [SerializeField] [Range(0f, 0.5f)] private float _startReserveBound = 0f;
        [SerializeField] [Range(0.5f, 1f)] private float _endReserveBound = 1f;
        [SerializeField] private InteractivePathSectionSettings[] _sections;

        [Serializable]
        private struct InteractivePathSectionSettings {

            [Header("Bounds")]
            [Range(0f, 1f)] public float start;
            [Range(0f, 1f)] public float stop;

            [Header("Orientation")]
            public Vector3 orientationWeights;
            public Vector3 orientationOffset;

            [Header("Forward")]
            public Vector3 forwardWeights;
            public Vector3 forwardOffset;
        }

        public float GetStart(Vector3 position) {
            _splineContainer.GetNearestPoint(position, out float t);
            return Mathf.Clamp(t, _startReserveBound, _endReserveBound);
        }

        public float MoveAlongPath(float t, Vector3 motion) {
            return _splineContainer.MoveAlongSpline(motion, t);
        }

        public void Evaluate(float t, out Vector3 position, out Vector3 forward, out Quaternion orientation) {
            _splineContainer.Evaluate(t, out var pos, out var tangent, out var normal);
            position = pos;

            if (TryGetSection(t, out var section)) {
                forward = GetForward(section, tangent, normal);
                orientation = GetOrientation(section, tangent, normal);
                return;
            }

            forward = GetForwardDefault(tangent, normal);
            orientation = GetOrientationDefault(tangent, normal);
        }

        private bool TryGetSection(float t, out InteractivePathSectionSettings section) {
            for (int i = 0; i < _sections.Length; i++) {
                var s = _sections[i];

                if (t < s.start) continue;
                if (t > s.stop) continue;

                section = s;
                return true;
            }

            section = default;
            return false;
        }

        private static Vector3 GetForward(InteractivePathSectionSettings section, Vector3 tangent, Vector3 normal) {
            var euler = Quaternion.LookRotation(tangent, normal).eulerAngles;

            euler.x = euler.x * section.forwardWeights.x + section.forwardOffset.x;
            euler.y = euler.y * section.forwardWeights.y + section.forwardOffset.y;
            euler.z = euler.z * section.forwardWeights.z + section.forwardOffset.z;

            return Quaternion.Euler(euler) * Vector3.forward;
        }

        private static Vector3 GetForwardDefault(Vector3 tangent, Vector3 normal) {
            return Quaternion.LookRotation(tangent, normal) * Vector3.forward;
        }

        private static Quaternion GetOrientation(InteractivePathSectionSettings section, Vector3 tangent, Vector3 normal) {
            var euler = Quaternion.LookRotation(tangent, normal).eulerAngles;

            euler.x = euler.x * section.orientationWeights.x + section.orientationOffset.x;
            euler.y = euler.y * section.orientationWeights.y + section.orientationOffset.y;
            euler.z = euler.z * section.orientationWeights.z + section.orientationOffset.z;

            return Quaternion.Euler(euler);
        }

        private static Quaternion GetOrientationDefault(Vector3 tangent, Vector3 normal) {
            return Quaternion.LookRotation(tangent, normal);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_sections == null) return;

            float t = 0f;

            for (int i = 0; i < _sections.Length; i++) {
                var section = _sections[i];

                if (section.start < t) {
                    section.start = t;
                    _sections[i] = section;
                }

                if (section.stop < section.start) {
                    section.stop = section.start;
                    _sections[i] = section;
                }

                t = section.stop;
            }
        }

        private void OnDrawGizmosSelected() {
            if (_splineContainer == null) return;

            var startBoundPosition = _splineContainer.EvaluatePosition(_startReserveBound);
            var endBoundPosition = _splineContainer.EvaluatePosition(_endReserveBound);

            DbgSphere.Create().Radius(0.05f).Color(Color.white).Position(startBoundPosition).Draw();
            DbgSphere.Create().Radius(0.05f).Color(Color.white).Position(endBoundPosition).Draw();

            const float step = 0.04f;
            const float dirLength = 0.4f;

            for (float t = 0; t <= 1f; t += step) {
                if (t > 1f) t = 1f;

                _splineContainer.Evaluate(t, out var position, out var tangent, out var normal);

                Vector3 forward;
                Quaternion orientation;

                if (TryGetSection(t, out var section)) {
                    forward = GetForward(section, tangent, normal);
                    orientation = GetOrientation(section, tangent, normal);
                }
                else {
                    forward = GetForwardDefault(tangent, normal);
                    orientation = GetOrientationDefault(tangent, normal);
                }

                DbgSphere.Create().Radius(0.03f).Color(Color.red).Position(position).Draw();

                DbgRay.Create().From(position).Dir(orientation * (dirLength * Vector3.forward)).Color(Color.red).Draw();
                DbgRay.Create().From(position).Dir(dirLength * forward).Color(Color.green).Draw();
            }
        }
#endif
    }

}
