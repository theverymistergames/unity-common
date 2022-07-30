using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.UI.Initialization {
    
    public class CanvasRegistry : MonoBehaviour {
        
        public static CanvasRegistry Instance { get; private set; }

        private readonly List<Canvas> _canvases = new List<Canvas>();
        private Camera _eventCamera;

        private void Awake() {
            Instance = this;
        }

        public void AddCanvas(Canvas canvas) {
            canvas.worldCamera = _eventCamera;
            _canvases.Add(canvas);
        }

        public void SetCanvasEventCamera(Camera eventCamera) {
            _eventCamera = eventCamera;

            for (int i = 0; i < _canvases.Count; i++) {
                _canvases[i].worldCamera = eventCamera;
            }
        }
    }
    
}
