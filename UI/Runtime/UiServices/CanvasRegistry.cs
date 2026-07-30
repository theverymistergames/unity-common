using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MisterGames.UI.UiServices {
    
    public sealed class CanvasRegistry : IDisposable {

        private sealed class CameraDepthComparer : IComparer<Camera> {
            public int Compare(Camera x, Camera y) => y!.depth.CompareTo(x!.depth);
        }

        private readonly HashSet<Canvas> _canvases = new();
        private readonly SortedSet<Camera> _cameraSet = new(new CameraDepthComparer());

        public void Dispose() {
            _canvases.Clear();
            _cameraSet.Clear();
        }

        public Canvas GetClosestParentCanvas(Transform transform) {
            List<Canvas> candidates = null;
            
            foreach (var c in _canvases) {
                if (!transform.IsChildOf(c.transform) && transform != c.transform) continue;

                candidates ??= ListPool<Canvas>.Get();
                candidates.Add(c);
            }
            
            if (candidates == null) return null;

            Canvas canvas = null;

            for (int i = 0; i < candidates.Count; i++) {
                var trf = candidates[i].transform;
                bool isChildForAll = true;
                
                for (int j = 0; j < candidates.Count; j++) {
                    if (i == j || trf.IsChildOf(candidates[j].transform)) continue;
                    
                    isChildForAll = false;
                    break;
                }
                
                if (!isChildForAll) continue;
                
                canvas = candidates[i];
                break;
            }

            ListPool<Canvas>.Release(candidates);

            return canvas;
        }

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
            foreach (var canvas in _canvases) {
                if (canvas != null) canvas.worldCamera = eventCamera;
            }
        }

        public bool TryGetCurrentEventCamera(out Camera camera) {
            foreach (var c in _cameraSet) {
                camera = c;
                return true;
            }

            camera = null;
            return false;
        }
    }
    
}
