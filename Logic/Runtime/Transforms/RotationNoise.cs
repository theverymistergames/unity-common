using System;
using System.Linq;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Common.Trees;
using UnityEngine;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {
    
    public sealed class RotationNoise : MonoBehaviour, IUpdate {

        [Header("Targets")]
        [SerializeField] private Transform[] _targets;
        [SerializeField] private TargetLimit[] _limits;
        
        [Header("Noise")]
        [SerializeField] private Vector3 _rotationMul;
        [SerializeField] private Vector3 _noiseOffset;
        [SerializeField] private Vector3 _noiseSpeed;
        [SerializeField] private float _noiseSpeedMul = 1f;
        [SerializeField] private float _noiseAmplitude = 1f;

        [Header("Hierarchy")]
        [SerializeField] private float _hierarchyLevelMul;
        [SerializeField] private AnimationCurve _hierarchyLevelCurve = EasingType.Linear.ToAnimationCurve();
        
        [HideInInspector] [SerializeField] private Quaternion[] _originLocalRotations;
        [HideInInspector] [SerializeField] private TargetData[] _targetDataArray;
        [HideInInspector] [SerializeField] private int _minLevel = 1;
        [HideInInspector] [SerializeField] private int _maxLevel = 1;

        [Serializable]
        private struct TargetLimit {
            public Transform target;
            public Vector3 mul;
        }

        [Serializable]
        private struct TargetData {
            public Vector3 mul;
            public int level;
        }
        
        public float NoiseSpeed { get => _noiseSpeedMul; set => SetNoiseScale(_noiseSpeedMul, value); }
        public float NoiseAmplitude { get => _noiseAmplitude; set => SetNoiseAmplitude(value); }

        private float _noiseScaleOffset;
        private float _noiseScaleTime;
        
        private void Awake() {
            FetchInitialRotations();
            FetchTargetData();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }
        
        private void SetNoiseAmplitude(float value) {
            _noiseAmplitude = value;
      
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif      
        }
        
        private void SetNoiseScale(float oldValue, float newValue) {
            float time = Time.time;

#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup;
#endif
            
            _noiseScaleOffset += oldValue * (time - _noiseScaleTime);
            _noiseSpeedMul = newValue;
            _noiseScaleTime = time;

#if UNITY_EDITOR
            _lastNoiseSpeedMul = newValue;
            EditorUtility.SetDirty(this);
#endif
        }

        void IUpdate.OnUpdate(float dt) {
            float time = Time.time;
            
#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup;
#endif

            float t = (time - _noiseScaleTime) * _noiseSpeedMul + _noiseScaleOffset;
            var noiseMul = _noiseAmplitude * _rotationMul; 
            
            for (int i = 0; i < _targets.Length; i++) {
                var target = _targets[i];
                ref var data = ref _targetDataArray[i];
                
                var rotationNoise = new Vector3(
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.x) - 0.5f,
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.y) - 0.5f,
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.z) - 0.5f
                );

                float levelT = _maxLevel > _minLevel ? (float) (data.level - _minLevel) / (_maxLevel - _minLevel) : 0f;
                float levelMul = _hierarchyLevelCurve.Evaluate(levelT) * _hierarchyLevelMul + 1f;
                
                target.localRotation = _originLocalRotations[i] * 
                                       Quaternion.Euler(rotationNoise.Multiply(data.mul).Multiply(noiseMul) * levelMul);
            }
        }

        private void FetchInitialRotations(bool force = false) {
            if (_originLocalRotations != null && _originLocalRotations.Length == _targets.Length && !force) return;
            
            int dataCount = _originLocalRotations?.Length ?? 0;
            if (_targets.Length != dataCount) _originLocalRotations = new Quaternion[_targets.Length];
            
            for (int i = 0; i < _originLocalRotations!.Length; i++) {
                var t = _targets[i];
                if (t == null) continue;
                
                _originLocalRotations[i] = t.localRotation;
            }
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        private void FetchTargetData() {
            int dataCount = _targetDataArray?.Length ?? 0;
            if (_targets.Length != dataCount) {
                var arr = new TargetData[_targets.Length];
                
                if (_targetDataArray != null) {
                    Array.Copy(_targetDataArray, arr, Mathf.Min(arr.Length, dataCount));
                }
                
                for (int i = dataCount; i < arr.Length; i++) {
                    ref var data = ref arr[i];
                    data.mul = Vector3.one;
                    data.level = 1;
                }

                _targetDataArray = arr;
            }

            var map = DictionaryPool<int, Vector3>.Get();
            for (int i = 0; i < _limits.Length; i++) {
                ref var targetLimit = ref _limits[i];
                if (targetLimit.target == null) continue;
                
                map[targetLimit.target.GetInstanceID()] = targetLimit.mul;
            }

            _minLevel = 1;
            _maxLevel = 1;
            
            for (int i = 0; i < _targetDataArray!.Length; i++) {
                ref var data = ref _targetDataArray[i];
                
                _minLevel = Mathf.Min(_minLevel, data.level);
                _maxLevel = Mathf.Max(_maxLevel, data.level);
                
                data.mul = _targets[i] == null || !map.TryGetValue(_targets[i].GetInstanceID(), out var mul)
                    ? Vector3.one
                    : mul;
            }

            DictionaryPool<int, Vector3>.Release(map);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private float _lastNoiseSpeedMul = 1f;
        
        private void OnValidate() {
            if (!_lastNoiseSpeedMul.IsNearlyEqual(_noiseSpeedMul)) {
                SetNoiseScale(_lastNoiseSpeedMul, _noiseSpeedMul);
            }
        }

        [Button]
        private void StartDemo() {
            Undo.RecordObject(this, "StartDemo");
            
            FetchTargetData();
            FetchInitialRotations();
            
            PlayerLoopStage.Update.Subscribe(this);
        }
        
        [Button]
        private void StopDemo() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }
        
        [Button]
        private void SaveRotationsAsInitial() {
            Undo.RecordObject(this, "SaveRotationsAsInitial");
            FetchInitialRotations(force: true);
        }
        
        [Button]
        private void RestoreInitialRotations() {
            for (int i = 0; i < _originLocalRotations?.Length && i < _targets?.Length; i++) {
                var t = _targets[i];
                if (t == null) continue;
                
                Undo.RecordObject(t, "RestoreInitialRotations");
                t.localRotation = _originLocalRotations[i];
                EditorUtility.SetDirty(t);
            }
        }

        [Button]
        private void FetchHierarchyLevels() {
            Undo.RecordObject(this, "FetchHierarchyLevels");
            
            FetchTargetData();
            
            var nodes = PathTree
                .CreateTree(_targets, t => t?.GetPathInScene())
                .LevelOrder().Where(x => x.children.Count == 0 && x.data.data != null)
                .ToArray();
            
            var map = DictionaryPool<int, int>.Get();
            for (int i = 0; i < nodes.Length; i++) {
                var treeEntry = nodes[i];
                map[treeEntry.data.data.GetInstanceID()] = treeEntry.level;
            }

            _minLevel = 1;
            _maxLevel = 1;
            
            for (int i = 0; i < _targetDataArray.Length; i++) {
                ref var data = ref _targetDataArray[i];
                
                data.level = _targets[i] == null || !map.TryGetValue(_targets[i].GetInstanceID(), out int level)
                    ? 1
                    : level;

                _minLevel = Mathf.Min(_minLevel, data.level);
                _maxLevel = Mathf.Max(_maxLevel, data.level);
            }
            
            EditorUtility.SetDirty(this);
        }
#endif
    }
    
}