using System;
using MisterGames.Actors;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class LampBehaviour : MonoBehaviour, IActorComponent {

        [SerializeField] private MaterialMode _mode = MaterialMode.MaterialPropertyBlock;
        [SerializeField] private Light[] _lights;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] [Min(0f)] private float _weight = 1f;
        [SerializeField] [Min(0f)] private float _intensity = 1f;

        private enum MaterialMode {
            MaterialPropertyBlock,
            InstantiateMaterial,
        }
        
        public float Weight { get => _weight; set => SetWeight(value); }
        public float Intensity { get => _intensity; set => SetIntensity(value); }
        
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        
        private float[] _originLightIntensities;
        private Color[] _originLightColors;
        private Color[] _originMaterialColors;
        private Color[] _overrideLightColors;
        private Color[] _overrideMaterialColors;
        private MaterialPropertyBlock[] _materialPropertyBlocks;
        
        private void Awake() {
            FetchOriginalLightData();
            FetchOriginalMaterialData();
        }

        private void OnEnable() {
            for (int i = 0; i < _lights.Length; i++) {
                _lights[i].enabled = true;
            }
            
            UpdateState();
        }

        private void OnDisable() {
            for (int i = 0; i < _lights.Length; i++) {
                _lights[i].enabled = false;
            }
            
            UpdateState();
        }

        public void SetLightColor(Color color, int index) {
            if (index < 0 || index >= _lights.Length) return;
            
            _overrideLightColors[index] = color;
            
            UpdateState();
        }

        public void SetAllLightsColor(Color color) {
            for (int i = 0; i < _overrideLightColors.Length; i++) {
                _overrideLightColors[i] = color;
            }
            
            UpdateState();
        }

        public void ResetLightColor(int index) {
            if (index < 0 || index >= _lights.Length) return;
            
            _overrideLightColors[index] = _originLightColors[index];
            
            UpdateState();
        }
        
        public void ResetAllLightsColor() {
            for (int i = 0; i < _overrideLightColors.Length; i++) {
                _overrideLightColors[i] = _originLightColors[i];
            }
            
            UpdateState();
        }

        public void SetMaterialColor(Color color, int index) {
            if (index < 0 || index >= _lights.Length) return;
            
            _overrideMaterialColors[index] = color;
            
            UpdateState();
        }

        public void SetAllMaterialsColor(Color color) {
            for (int i = 0; i < _overrideMaterialColors.Length; i++) {
                _overrideMaterialColors[i] = color;
            }
            
            UpdateState();
        }

        public void ResetMaterialColor(int index) {
            if (index < 0 || index >= _lights.Length) return;
            
            _overrideMaterialColors[index] = _originMaterialColors[index];
            
            UpdateState();
        }
        
        public void ResetAllMaterialsColor() {
            for (int i = 0; i < _overrideMaterialColors.Length; i++) {
                _overrideMaterialColors[i] = _originMaterialColors[i];
            }
            
            UpdateState();
        }
        
        private void SetWeight(float weight) {
            float oldValue = _weight * _intensity;
            _weight = Mathf.Max(0f, weight);
            
            if (oldValue.IsNearlyEqual(_weight * _intensity)) return;
            
            UpdateState();
        }

        private void SetIntensity(float intensity) {
            float oldValue = _weight * _intensity;
            _intensity = Mathf.Max(0f, intensity);
            
            if (oldValue.IsNearlyEqual(_weight * _intensity)) return;
            
            UpdateState();
        }
        
        private void UpdateState() {
            float intensity = _weight * _intensity * enabled.AsFloat();
            
            for (int i = 0; i < _lights.Length; i++) {
                var light = _lights[i];
                
                light.intensity = Mathf.LerpUnclamped(0f, _originLightIntensities[i], intensity);
                light.color = _overrideLightColors[i];
            }

            switch (_mode) {
                case MaterialMode.MaterialPropertyBlock:
                    for (int i = 0; i < _renderers.Length; i++) {
                        _materialPropertyBlocks[i].SetVector(EmissiveColor, intensity * _overrideMaterialColors[i]);
                        _renderers[i].SetPropertyBlock(_materialPropertyBlocks[i]);
                    }
                    break;
                
                case MaterialMode.InstantiateMaterial:
                    for (int i = 0; i < _renderers.Length; i++) {
                        _renderers[i].material.SetColor(EmissiveColor, intensity * _overrideMaterialColors[i]);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FetchOriginalLightData() {
            if (_lights is not { Length: > 0 }) {
                _originLightIntensities = Array.Empty<float>();
                _originLightColors = Array.Empty<Color>();
                _overrideLightColors = Array.Empty<Color>();
                return;
            }
            
            int lightCount = _lights.Length;

            _originLightIntensities = new float[lightCount];
            _originLightColors = new Color[lightCount];
            _overrideLightColors = new Color[lightCount];
            
            for (int i = 0; i < lightCount; i++) {
                var light = _lights[i];

                _originLightIntensities[i] = light.intensity;
                _originLightColors[i] = light.color;
                _overrideLightColors[i] = light.color;
            }
        }

        private void FetchOriginalMaterialData() {
            if (_renderers is not { Length: > 0 }) {
                _originMaterialColors = Array.Empty<Color>();
                _overrideMaterialColors = Array.Empty<Color>();
                _materialPropertyBlocks = Array.Empty<MaterialPropertyBlock>();
                return;
            }
            
            _originMaterialColors = new Color[_renderers.Length];
            _overrideMaterialColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++) {
                var color = _renderers[i].sharedMaterial.GetColor(EmissiveColor);
                        
                _originMaterialColors[i] = color;
                _overrideMaterialColors[i] = color;
            }
            
            switch (_mode) {
                case MaterialMode.MaterialPropertyBlock:
                    _materialPropertyBlocks = new MaterialPropertyBlock[_renderers.Length];

                    for (int i = 0; i < _materialPropertyBlocks.Length; i++) {
                        _materialPropertyBlocks[i] = new MaterialPropertyBlock();
                    }
                    break;
                
                case MaterialMode.InstantiateMaterial:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _renderers = GetComponentsInChildren<Renderer>();
            _lights = GetComponentsInChildren<Light>();
        }

        private void OnValidate() {
            if (!Application.isPlaying || _originLightIntensities == null) return;
            
            UpdateState();
        }
#endif
    }
    
}