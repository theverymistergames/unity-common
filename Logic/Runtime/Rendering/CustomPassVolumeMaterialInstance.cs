using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.Logic.Rendering {
    
    public sealed class CustomPassVolumeMaterialInstance : MonoBehaviour {
        
        [SerializeField] private CustomPassVolume _customPassVolume;
        [SerializeField] private Material _material;
        
        public Material Material => GetMaterial();
        
        private bool _instantiated;
        private bool _destroyed;
        private Material _runtimeMaterial;
        
        private void Awake() {
            InstantiateMaterial();
        }

        private void OnDestroy() {
            Destroy(_runtimeMaterial);
            
            _instantiated = false;
            _runtimeMaterial = null;
        }

        private Material GetMaterial() {
            if (_destroyed) return null;
            
            InstantiateMaterial();
            return _runtimeMaterial;
        }

        private void InstantiateMaterial() {
            if (_instantiated) return;

            _instantiated = true;
            
            _runtimeMaterial = new Material(_material);
            ((FullScreenCustomPass) _customPassVolume.customPasses[0]).fullscreenPassMaterial = _runtimeMaterial;
        }
    }
    
}