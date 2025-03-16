using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class LampBehaviour : MonoBehaviour, IActorComponent {
        
        [SerializeField] private Light[] _lights;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] [Min(0f)] private float _weight = 1f;
        [SerializeField] [Min(0f)] private float _intensity = 1f;

        public float Weight { get => _weight; set => SetWeight(value); }
        public float Intensity { get => _intensity; set => SetIntensity(value); }

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        
        private float[] _lightIntensities;
        private Color[] _originLightColors;
        private Color[] _originMaterialColors;
        private Color[] _overrideLightColors;
        private Color[] _overrideMaterialColors;

        private void Awake() {
            _lightIntensities = new float[_lights.Length];
            
            _originLightColors = new Color[_lights.Length];
            _originMaterialColors = new Color[_renderers.Length];
            
            _overrideLightColors = new Color[_lights.Length];
            _overrideMaterialColors = new Color[_renderers.Length];
            
            for (int i = 0; i < _lights.Length; i++) {
                var light = _lights[i];
                
                _lightIntensities[i] = light.intensity;
                _originLightColors[i] = light.color;
                _overrideLightColors[i] = light.color;
            }
            
            for (int i = 0; i < _renderers.Length; i++) {
                var color = _renderers[i].material.GetColor(EmissiveColor);
                
                _originMaterialColors[i] = color;
                _overrideMaterialColors[i] = color;
            }
        }

        private void OnEnable() {
            SetWeight(_weight);
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
            for (int i = 0; i < _originMaterialColors.Length; i++) {
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
            _weight = Mathf.Max(0f, weight);
            UpdateState();
        }

        private void SetIntensity(float intensity) {
            _intensity = Mathf.Max(0f, intensity);
            UpdateState();
        }
        
        private void UpdateState() {
            for (int i = 0; i < _lights.Length; i++) {
                var light = _lights[i];
                
                light.intensity = Mathf.LerpUnclamped(0f, _lightIntensities[i], _weight * _intensity);
                light.color = _overrideLightColors[i];
            }
            
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].material.SetColor(EmissiveColor, _weight * _intensity * _overrideMaterialColors[i]);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying || _lightIntensities == null) return;
            
            SetWeight(_weight);
            SetIntensity(_intensity);
        }
#endif
    }
    
}