using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Logic.Interactives {
    
    public sealed class LampBehaviour : MonoBehaviour, IActorComponent {
        
        [SerializeField] private Light[] _lights;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] [Min(0f)] private float _weight = 1f;
        [SerializeField] [Min(0f)] private float _intensity = 1f;

        public float Weight { get => _weight; set => SetWeight(value); }
        public float Intensity { get => _intensity; set => SetIntensity(value); }

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        
        private float[] _lightIntensities;
        private Color[] _materialColors;
        private int[] _paramIds;
        
        private void Awake() {
            _lightIntensities = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++) {
                _lightIntensities[i] = _lights[i].intensity;
            }
            
            _materialColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++) {
                _materialColors[i] = _renderers[i].material.GetColor(EmissiveColor) * 1f;
            }
        }

        private void OnEnable() {
            SetWeight(_weight);
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
                _lights[i].intensity = Mathf.LerpUnclamped(0f, _lightIntensities[i], _weight * _intensity);
            }
            
            for (int i = 0; i < _renderers.Length; i++) {
                _renderers[i].material.SetColor(EmissiveColor, _weight * _intensity * _materialColors[i]);
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