using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Logic.Transforms {
    
    public sealed class TransformNoise : MonoBehaviour, IUpdate {
        
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private Vector3 _scaleOffset;
        [SerializeField] private Vector3 _positionMultiplier;
        [SerializeField] private Vector3 _rotationMultiplier;
        [SerializeField] private Vector3 _scaleMultiplier;
        [SerializeField] private float _noiseScale = 1f;
        [SerializeField] private float _amplitude = 1f;
        [SerializeField] private bool _useLocal = true;
        [SerializeField] private ApplyMode applyMode = ApplyMode.Position | ApplyMode.Rotation | ApplyMode.Scale;
        
        [Flags]
        private enum ApplyMode {
            None = 0,
            Position = 1,
            Rotation = 2,
            Scale = 4,
        }

        public float NoiseScale { get => _noiseScale; set => SetNoiseScale(_noiseScale, value); }
        public float NoiseAmplitude { get => _amplitude; set => SetNoiseAmplitude(value); }

        private float _noiseScaleOffset;
        private float _noiseScaleTime;
        
        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        private void SetNoiseScale(float oldValue, float newValue) {
            float time = Time.time;

#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup;
#endif
            
            _noiseScaleOffset += oldValue * (time - _noiseScaleTime);
            _noiseScale = newValue;
            _noiseScaleTime = time;

#if UNITY_EDITOR
            _lastNoiseScale = newValue;
            EditorUtility.SetDirty(this);
#endif
        }

        private void SetNoiseAmplitude(float value) {
            _amplitude = value;
      
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif      
        }

        void IUpdate.OnUpdate(float dt) {
            float time = Time.time;
            
#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup;
#endif
            
            float s = _amplitude;
            float t = (time - _noiseScaleTime) * _noiseScale + _noiseScaleOffset;
            
            var positionNoise = s * new Vector3(
                (Mathf.PerlinNoise1D(t + _positionOffset.x) - 0.5f) * _positionMultiplier.x,
                (Mathf.PerlinNoise1D(t + _positionOffset.y) - 0.5f) * _positionMultiplier.y,
                (Mathf.PerlinNoise1D(t + _positionOffset.z) - 0.5f) * _positionMultiplier.z
            );
            
            var rotationNoise = s * new Vector3(
                (Mathf.PerlinNoise1D(t + _rotationOffset.x) - 0.5f) * _rotationMultiplier.x,
                (Mathf.PerlinNoise1D(t + _rotationOffset.y) - 0.5f) * _rotationMultiplier.y,
                (Mathf.PerlinNoise1D(t + _rotationOffset.z) - 0.5f) * _rotationMultiplier.z
            );

            var scaleNoise = Vector3.one + s * new Vector3(
                (Mathf.PerlinNoise1D(t + _scaleOffset.x) - 0.5f) * _scaleMultiplier.x,
                (Mathf.PerlinNoise1D(t + _scaleOffset.y) - 0.5f) * _scaleMultiplier.y,
                (Mathf.PerlinNoise1D(t + _scaleOffset.z) - 0.5f) * _scaleMultiplier.z
            );
            
            if ((applyMode & ApplyMode.Position) == ApplyMode.Position) {
                if (_useLocal) _transform.localPosition = positionNoise;
                else _transform.position = positionNoise;        
            }
            
            if ((applyMode & ApplyMode.Rotation) == ApplyMode.Rotation) {
                if (_useLocal) _transform.localRotation = Quaternion.Euler(rotationNoise);
                else _transform.rotation = Quaternion.Euler(rotationNoise);
            }
            
            if ((applyMode & ApplyMode.Scale) == ApplyMode.Scale) {
                _transform.localScale = scaleNoise;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(_transform);
#endif
        }

#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private float _lastNoiseScale = 1f;
        
        private void OnValidate() {
            if (_lastNoiseScale.IsNearlyEqual(_noiseScale)) return;
            
            SetNoiseScale(_lastNoiseScale, _noiseScale);
        }

        [Button]
        private void StartDemo() {
            if (_transform == null) return;
            
            PlayerLoopStage.Update.Subscribe(this);
        }
        
        [Button]
        private void StopDemo() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }
#endif
    }
    
}