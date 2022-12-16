using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.UI.Initialization {
    
    public class CanvasRegistry {

        public static readonly CanvasRegistry Instance = new CanvasRegistry();

        private readonly List<Canvas> _canvases = new List<Canvas>();
        private Camera _eventCamera;

        public void AddCanvas(Canvas canvas) {
            canvas.worldCamera = _eventCamera;
            _canvases.Add(canvas);
        }

        public void RemoveCanvas(Canvas canvas) {
            _canvases.Remove(canvas);
        }

        public void SetCanvasEventCamera(Camera eventCamera) {
            _eventCamera = eventCamera;

            for (int i = 0; i < _canvases.Count; i++) {
                var canvas = _canvases[i];
                if (canvas == null) continue;

                canvas.worldCamera = eventCamera;
            }
        }
    }
    
}
