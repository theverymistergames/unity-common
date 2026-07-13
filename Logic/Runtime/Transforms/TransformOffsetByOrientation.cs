using System;
using MisterGames.Common.Tick;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {

    [ExecuteInEditMode]
    public sealed class TransformOffsetByOrientation : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Vector3 _axis = Vector3.up;
        [SerializeField] [Range(0.001f, 180f)] private float _maxInfluenceAngle = 45f;
        [SerializeField] private OffsetData[] _offsets;

        [Serializable]
        private struct OffsetData {
            public Vector3 orientation;
            public float offset;
        }

        private void OnEnable() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            ApplyOffset();
        }

        private void ApplyOffset() {
            var offsets = _offsets;
            if (offsets == null || offsets.Length == 0) return;

            var parentRot = _target.parent != null ? _target.parent.rotation : Quaternion.identity;
            var currentRot = _target.localRotation;
            var blendedOffset = Vector3.zero;
            float totalWeight = 0f;

            for (int i = 0; i < offsets.Length; i++) {
                var data = offsets[i];
                var orient = Quaternion.Euler(data.orientation);
                float angle = Vector3.Angle(currentRot * _axis, orient * _axis);
                float weight = Mathf.Clamp01(1f - angle / _maxInfluenceAngle);

                if (weight <= 0f) continue;

                blendedOffset += parentRot * orient * _axis * (weight * data.offset);
                totalWeight += weight;
            }

            if (totalWeight > 1f) blendedOffset /= totalWeight;

            var pos = _pivot.position + blendedOffset;

#if UNITY_EDITOR
            bool changed = _target.position != pos;
#endif

            _target.position = pos;

#if UNITY_EDITOR
            if (changed) EditorUtility.SetDirty(_target);
#endif
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInEditor;

        private void LateUpdate() {
            if (!_updateInEditor || Application.isPlaying || _target == null || _pivot == null) return;

            ApplyOffset();
        }

        private void Reset() {
            _target = transform;
        }
#endif
    }

}
