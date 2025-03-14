using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEditor;
using UnityEngine;

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

        [HideInInspector]
        [SerializeField] private Quaternion[] _originLocalRotations;
        
        [Serializable]
        private struct TargetLimit {
            public Transform target;
            public Vector3 mul;
        }
        
        public float NoiseSpeed { get => _noiseSpeedMul; set => SetNoiseScale(_noiseSpeedMul, value); }
        public float NoiseAmplitude { get => _noiseAmplitude; set => SetNoiseAmplitude(value); }

        private readonly Dictionary<int, Vector3> _limitsMap = new();
        
        private float _noiseScaleOffset;
        private float _noiseScaleTime;
        
        private void Awake() {
            FetchInitialRotations();
            FetchLimits();
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
                
                var mul = _limitsMap.GetValueOrDefault(target.GetInstanceID(), Vector3.one).Multiply(noiseMul);
                var rotationNoise = new Vector3(
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.x) - 0.5f,
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.y) - 0.5f,
                    Mathf.PerlinNoise1D(t + (i + 1) * _noiseOffset.z) - 0.5f
                );
                
                target.localRotation = _originLocalRotations[i] * Quaternion.Euler(rotationNoise.Multiply(mul));
            }
        }

        private void FetchInitialRotations() {
            if (_originLocalRotations != null && _originLocalRotations.Length == _targets.Length) return;
            
            _originLocalRotations = new Quaternion[_targets.Length];
                
            for (int i = 0; i < _originLocalRotations.Length; i++) {
                var t = _targets[i];
                if (t == null) continue;
                
                _originLocalRotations[i] = t.localRotation;
            }
        }

        private void FetchLimits() {
            _limitsMap.Clear();
            
            for (int i = 0; i < _limits?.Length; i++) {
                var data = _limits[i];
                _limitsMap[data.target.GetInstanceID()] = data.mul;
            }
        }
        
#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private float _lastNoiseSpeedMul = 1f;
        
        private void OnValidate() {
            if (_lastNoiseSpeedMul.IsNearlyEqual(_noiseSpeedMul)) return;
            
            SetNoiseScale(_lastNoiseSpeedMul, _noiseSpeedMul);
        }

        [Button]
        private void StartDemo() {
            FetchLimits();
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
            FetchInitialRotations();
            EditorUtility.SetDirty(this);
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
            
            //var root = PathTree.CreateTree(_targets, t => t.GetPathInScene()).LevelOrder();
            //Debug.Log($"RotationNoise.FetchHierarchyLevels: f {Time.frameCount}, tree:\n{string.Join("\n", root.Select(x => $"[{x.level}] {x.data.data}"))}");
        }
#endif
    }
    
}