using MisterGames.Common.Colors;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class ParticleAlphaController : MonoBehaviour {
        
		[SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] [Range(0f, 1f)] private float _alpha;
        
        private static readonly int Color = Shader.PropertyToID("_Color"); 
        
        private ParticleSystemRenderer _renderer;
        private Color _colorMain;
        private Color _colorTrail;
        private bool _hasTrails;
        
        private void Awake() {
            _renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            _hasTrails = _renderer.trailMaterial != null;
            
            _renderer.material = new Material(_renderer.sharedMaterial);
            if (_hasTrails) _renderer.trailMaterial = new Material(_renderer.trailMaterial);
            
            _colorMain = _renderer.sharedMaterial.GetColor(Color);
            _colorTrail = _hasTrails ? _renderer.sharedMaterial.GetColor(Color) : default;
        }

        private void OnEnable() {
            SetAlpha(_alpha);
        }

        public void SetAlpha(float alpha) {
            _alpha = Mathf.Clamp01(alpha);
            
            _renderer.material.SetColor(Color, _colorMain.WithAlpha(_alpha));
            if (_hasTrails) _renderer.trailMaterial.SetColor(Color, _colorTrail.WithAlpha(_alpha));
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying || _renderer == null) return;

            SetAlpha(_alpha);
        }
#endif
    }
    
}