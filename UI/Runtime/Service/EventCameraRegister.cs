using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    [RequireComponent(typeof(Camera))]
    public sealed class EventCameraRegister : MonoBehaviour {
        
        private Camera _camera;
        
        private void Awake() {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable() {
            Services.Get<CanvasRegistry>()?.AddCanvasEventCamera(_camera);
        }
        
        private void OnDisable() {
            Services.Get<CanvasRegistry>()?.RemoveCanvasEventCamera(_camera);
        }
    }
    
}