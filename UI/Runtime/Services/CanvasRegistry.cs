using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.UI.Services {
    
    public sealed class CanvasRegistry {

        public static readonly CanvasRegistry Instance = new();
        
        private readonly HashSet<Canvas> _canvases = new();
        private readonly List<Camera> _cameraList = new();

        public void AddCanvas(Canvas canvas) {
            if (TryGetCurrentEventCamera(out var camera)) canvas.worldCamera = camera;
            
            _canvases.Add(canvas);
        }

        public void RemoveCanvas(Canvas canvas) {
            _canvases.Remove(canvas);
        }

        public void AddCanvasEventCamera(Camera eventCamera) {
            _cameraList.Add(eventCamera);

            _cameraList.Sort((x, y) => y.depth.CompareTo(x.depth));
            
            if (TryGetCurrentEventCamera(out var camera)) {
                UpdateEventCamera(camera);
            }
        }

        public void RemoveCanvasEventCamera(Camera eventCamera) {
            _cameraList.Remove(eventCamera);

            if (TryGetCurrentEventCamera(out var camera)) {
                UpdateEventCamera(camera);
            }
        }
        
        private void UpdateEventCamera(Camera eventCamera) {
            foreach (var canvas in _canvases) {
                if (canvas != null) canvas.worldCamera = eventCamera;
            }
        }

        private bool TryGetCurrentEventCamera(out Camera camera) {
            if (_cameraList.Count > 0) {
                camera = _cameraList[^1];
                return true;
            }

            camera = null;
            return false;
        }
    }
    
}
