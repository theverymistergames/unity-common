using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class CameraTargetTexture : MonoBehaviour {
        
        [SerializeField] private Camera _camera;
        [SerializeField] private RenderTexture _renderTexture;

        private void OnEnable() {
            _camera.targetTexture = _renderTexture;
        }
        
        private void OnDisable() {
            _camera.targetTexture = null;
        }
    }
    
}