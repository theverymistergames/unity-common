using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Components {
    
    public sealed class RadialObjectPlacer : MonoBehaviour {

        [SerializeField] private Transform _center;
        [SerializeField] [Min(1f)] private float _distance = 1f;
        [SerializeField] private Optional<float> _runtimeDistance = new Optional<float>(1f, false);
        [SerializeField] [Min(0.001f)] private float _tolerance = 0.1f;
        [SerializeField] private Transform[] _objects;
        
        private void Awake() {
            if (_runtimeDistance.HasValue) ApplyPositions(_runtimeDistance.Value);
        }

        private void ApplyPositions(float distance) {
            if (_center == null || _objects is not {Length: > 0}) return;

            var center = _center.position;
            var forward = _center.forward;
            
            for (int i = 0; i < _objects.Length; i++) {
                PlaceObject(_objects[i], center, forward, distance);
            }
        }

        private void PlaceObject(Transform obj, Vector3 center, Vector3 forward, float distance) {
            if (obj == null) return;
            
            var startPosition = obj.position;
            var diff = startPosition - center;
            float startDistance = diff.magnitude;
            
            if (startDistance.IsNearlyEqual(distance, _tolerance)) return;
            
#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.RecordObject(obj, $"{nameof(RadialObjectPlacer)}.PlaceObject");
#endif
            
            obj.position = startDistance > 0f 
                ? center + diff.normalized * Mathf.Max(distance, _tolerance)
                : center + forward * distance;

            if (distance > _tolerance && startDistance > _tolerance) {
                obj.localScale *= distance / startDistance;    
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(obj);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            float distance = Application.isPlaying && _runtimeDistance.HasValue ? _runtimeDistance.Value : _distance;
            ApplyPositions(distance);
        }
#endif
    }

}