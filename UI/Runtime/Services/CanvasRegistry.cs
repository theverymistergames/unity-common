using System.Collections.Generic;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    public sealed class CanvasRegistry {

        private sealed class CameraDepthComparer : IComparer<Camera> {
            public int Compare(Camera x, Camera y) => y!.depth.CompareTo(x!.depth);
        }
        
        public static readonly CanvasRegistry Instance = new();
        
        private readonly HashSet<Canvas> _canvases = new();
        private readonly SortedSet<Camera> _cameraSet = new(new CameraDepthComparer());

        public void AddCanvas(Canvas canvas) {
            if (TryGetCurrentEventCamera(out var camera)) canvas.worldCamera = camera;
            
            _canvases.Add(canvas);
        }

        public void RemoveCanvas(Canvas canvas) {
            _canvases.Remove(canvas);
        }

        public void AddCanvasEventCamera(Camera eventCamera) {
            _cameraSet.Add(eventCamera);
            
            if (TryGetCurrentEventCamera(out var camera)) {
                UpdateEventCamera(camera);
            }
        }

        public void RemoveCanvasEventCamera(Camera eventCamera) {
            _cameraSet.Remove(eventCamera);

            if (TryGetCurrentEventCamera(out var camera)) {
                UpdateEventCamera(camera);
            }
        }
        
        private void UpdateEventCamera(Camera eventCamera) {
            Debug.Log($"CanvasRegistry.UpdateEventCamera: f {Time.frameCount}, {eventCamera.GetPathInScene()}");
            foreach (var canvas in _canvases) {
                if (canvas != null) canvas.worldCamera = eventCamera;
            }
        }

        private bool TryGetCurrentEventCamera(out Camera camera) {
            foreach (var c in _cameraSet) {
                camera = c;
                return true;
            }

            camera = null;
            return false;
        }
    }
    
}
