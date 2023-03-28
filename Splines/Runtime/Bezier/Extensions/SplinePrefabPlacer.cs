using System;
using System.Collections;
using System.Collections.Generic;
using MisterGames.Bezier.Objects;
using UnityEngine;

namespace MisterGames.Bezier.Extensions {

    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineCreator))]
    public class SplinePrefabPlacer : MonoBehaviour {

        [SerializeField]
        private List<Segment> _segments = new List<Segment>();
        
        [SerializeField] [HideInInspector]
        private List<GameObject> _cache = new List<GameObject>();
        
        private SplineCreator _splineCreator;
        
        private void Awake() {
            _splineCreator = GetComponent<SplineCreator>();
        }

        private void OnEnable() {
            _splineCreator.pathUpdated += OnPathUpdated;
        }

        private void OnDisable() {
            _splineCreator.pathUpdated -= OnPathUpdated;
        }

        private void OnValidate() {
            StartCoroutine(NextFrame(RecalculateAll));
        }

        private IEnumerator NextFrame(Action action) {
            yield return null;
            action.Invoke();
        }

        private void OnPathUpdated() {
            RecalculateAll();
        }

        private void RecalculateAll() {
            foreach (var prefab in _cache) {
                DestroyImmediate(prefab.gameObject);
            }
            _cache.Clear();
            
            foreach (var segment in _segments) {
                Recalculate(segment);
            }
        }

        private void Recalculate(Segment segment) {
            if (segment.prefab == null) return;
            
            var amount = segment.amount;
            var infos = GetPlacingInfos(segment);

            for (var i = 0; i < amount; i++) {
                var info = infos[i];
                
                var createdPrefab = Instantiate(segment.prefab, info.position, info.rotation, transform);
                createdPrefab.name = $"{segment.prefab.name}_{i}";
                
                _cache.Add(createdPrefab.gameObject);
            }
        }

        private PlacingInfo[] GetPlacingInfos(Segment segment) {
            var path = _splineCreator.path;
            var infos = new PlacingInfo[segment.amount];
            var diff = (segment.endTime - segment.startTime) / (segment.amount + 1);
            
            for (var i = 0; i < segment.amount; i++) {
                var time = segment.startTime + diff * (i + 1) + segment.timeOffset;
                
                var position = path.GetPointAtTime(time) + segment.positionOffset;
                var rotation = path.GetRotation(time) * Quaternion.Euler(segment.rotationOffset);
                
                var info = new PlacingInfo {
                    position = position,
                    rotation = rotation
                };

                infos[i] = info;
            }

            return infos;
        }

        [Serializable]
        private struct Segment {
            
            public GameObject prefab;
            
            [Header("Placing")]
            [Range(0f, 1f)] public float startTime;
            [Range(0f, 1f)] public float endTime;
            [Range(-1f, 1f)] public float timeOffset;
            [Min(0)] public int amount;

            public Vector3 positionOffset;
            public Vector3 rotationOffset;
            
        }
        
        private struct PlacingInfo {
            public Vector3 position;
            public Quaternion rotation;
        }
        
    }

}