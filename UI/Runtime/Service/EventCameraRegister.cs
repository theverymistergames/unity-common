using UnityEngine;

namespace MisterGames.UI.Service {
    
    [RequireComponent(typeof(Camera))]
    public sealed class EventCameraRegister : MonoBehaviour {
        
        private Camera _camera;
        
        private void Awake() {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable() {
            CanvasRegistry.Instance.AddCanvasEventCamera(_camera);
        }
        
        private void OnDisable() {
            CanvasRegistry.Instance.RemoveCanvasEventCamera(_camera);
        }
    }
    
}