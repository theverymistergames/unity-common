using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public sealed class UiWindowsService : IUiWindowService, IDisposable {
        
        public event Action OnWindowsHierarchyChanged = delegate { };

        private readonly Dictionary<int, IUiWindow> _gameObjectIdToWindowMap = new();
        private readonly Dictionary<int, int> _childToParentMap = new();
        private readonly MultiValueDictionary<int, int> _relationTree = new();
        private readonly Dictionary<int, int> _layerToOpenedWindowIdMap = new();

        public void Dispose() {
            _gameObjectIdToWindowMap.Clear();
            _childToParentMap.Clear();
            _relationTree.Clear();
            _layerToOpenedWindowIdMap.Clear();
        }

        public void RegisterWindow(IUiWindow window, UiWindowState state) {
            _gameObjectIdToWindowMap[GetWindowId(window)] = window;
            
            UpdateHierarchy(window.GameObject);
            
            // Prevent setting initial window state if it has a parent window
            var firstState = GetParentWindow(window) != null
                ? UiWindowState.Closed
                : state;

            switch (firstState) {
                case UiWindowState.Closed:
                    SetWindowBranchState(window, UiWindowState.Closed);
                    break;
                
                case UiWindowState.Opened:
                    OpenWindow(window);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UnregisterWindow(IUiWindow window) {
            if (window.State == UiWindowState.Opened) {
                CloseWindow(window);
            }
            
            _gameObjectIdToWindowMap.Remove(GetWindowId(window));
            
            UpdateHierarchy(window.GameObject);
        }

        private void UpdateHierarchy(GameObject root) {
            if (root == null) return;
            
            var rootTrf = root.transform;
            
            foreach (var window in _gameObjectIdToWindowMap.Values) {
                if (!window.GameObject.transform.IsChildOf(rootTrf)) continue;
                
                BindWindowHierarchy(window);
            }
        }
        
        private void BindWindowHierarchy(IUiWindow window) {
            if (window?.GameObject == null) return;
            
            var parentWindow = FindClosestParentWindow(window.GameObject, includeSelf: false);
            
            if (parentWindow == null) {
                UnbindWindowHierarchy(window);
                return;
            }
            
            int id = GetWindowId(window);
            int parentId = GetWindowId(parentWindow);
            
            if (_childToParentMap.TryGetValue(id, out int existentParentId) && existentParentId != parentId) {
                UnbindWindowHierarchy(window);
            }
            
            _childToParentMap[id] = parentId;
            
            if (!_relationTree.ContainsValue(parentId, id)) {
                _relationTree.AddValue(parentId, id);
            }
        }
        
        private void UnbindWindowHierarchy(IUiWindow window) {
            if (window?.GameObject == null || !_childToParentMap.Remove(GetWindowId(window), out int parentId)) {
                return;
            }
            
            _relationTree.RemoveValue(parentId, GetWindowId(window));
        }

        public IUiWindow GetFocusedWindow() {
            return TryGetTopOpenedLayer(out int layer) && 
                   _gameObjectIdToWindowMap.TryGetValue(_layerToOpenedWindowIdMap[layer], out var window)
                ? window 
                : null;
        }

        public IUiWindow GetParentWindow(IUiWindow child) {
            return _gameObjectIdToWindowMap.GetValueOrDefault(_childToParentMap.GetValueOrDefault(GetWindowId(child)));
        }
        
        public IUiWindow GetRootWindow(IUiWindow window) {
            if (window == null) return null;
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out int parentId)) {
                window = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);
            }
            
            return window;
        }
        
        public IUiWindow FindClosestParentWindow(GameObject gameObject, bool includeSelf = true) {
            if (!includeSelf) gameObject = gameObject?.transform.parent?.gameObject;
            
            while (gameObject != null) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(gameObject.GetHashCode());
                if (window != null) return window;
                
                gameObject = gameObject.transform.parent?.gameObject;
            }

            return null;
        }

        public bool IsChildWindow(IUiWindow window, IUiWindow child) {
            return child?.GameObject != null && window?.GameObject != null && 
                   child.GameObject.transform.IsChildOf(window.GameObject.transform);
        }

        public bool HasOpenedWindows() {
            return _layerToOpenedWindowIdMap.Count > 0;
        }

        public bool IsInOpenedBranch(IUiWindow window) {
            if (window == null || !TryGetWindowRootLayer(window, out int layer)) return false;

            var frontWindow = _gameObjectIdToWindowMap.GetValueOrDefault(_layerToOpenedWindowIdMap.GetValueOrDefault(layer), window);
            
            while (frontWindow?.GameObject != null) 
            {
                if (frontWindow.State == UiWindowState.Opened) return true;
                if (frontWindow == window) return false;
                
                frontWindow = GetParentWindow(frontWindow);
            }
            
            return false;
        }

        public bool SetWindowState(IUiWindow window, UiWindowState state) {
            return state switch {
                UiWindowState.Closed => CloseWindow(window),
                UiWindowState.Opened => OpenWindow(window),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private bool TryGetTopOpenedLayer(out int layer) {
            bool hasLayers = false;
            layer = 0;
            
            foreach (int l in _layerToOpenedWindowIdMap.Keys) {
                if (hasLayers && l < layer) continue;
                
                layer = l;
                hasLayers = true;
            }
            
            return hasLayers;
        }
        
        private bool OpenWindow(IUiWindow window) {
            if (!TryGetWindowRootLayer(window, out int layer)) return false;
            
            bool changed = OpenWindowBranch(window);
            
            _layerToOpenedWindowIdMap[layer] = GetWindowId(window);
            
            var lastParentState = UiWindowState.Opened;
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out int parentId)) {
                int siblingsCount = _relationTree.GetCount(parentId);
                
                for (int i = 0; i < siblingsCount; i++) {
                    var sibling = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(parentId, i));
                    if (sibling != window) changed |= SetWindowBranchState(sibling, UiWindowState.Closed);
                }

                lastParentState = lastParentState switch {
                    UiWindowState.Closed => UiWindowState.Closed,
                    UiWindowState.Opened => window.OpenMode switch {
                        UiWindowOpenMode.Full => UiWindowState.Closed,
                        UiWindowOpenMode.Embedded => UiWindowState.Opened,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                var parent = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);
                changed |= parent != null && parent.State != lastParentState;
                parent?.NotifyWindowState(lastParentState);
                
                window = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);
            }
            
            OnWindowsHierarchyChanged.Invoke();
            return changed;
        }

        private bool CloseWindow(IUiWindow window) {
            if (window is null or { CloseMode: UiWindowCloseMode.NoExit }) return false;

            int id = GetWindowId(window);
            
            if (_childToParentMap.TryGetValue(id, out int parentId)) {
                return OpenWindow(_gameObjectIdToWindowMap.GetValueOrDefault(parentId));
            }

            if (_layerToOpenedWindowIdMap.TryGetValue(window.Layer, out int frontWindowId) && frontWindowId == id) {
                _layerToOpenedWindowIdMap.Remove(window.Layer);
            }
            
            bool changed = SetWindowBranchState(window, UiWindowState.Closed);
            
            OnWindowsHierarchyChanged.Invoke();
            
            return changed;
        }

        private bool TryGetWindowRootLayer(IUiWindow window, out int layer) {
            if (GetRootWindow(window) is { } root) {
                layer = root.Layer;
                return true;
            }
            
            layer = 0;
            return false;
        }
        
        private bool OpenWindowBranch(IUiWindow root) {
            if (root == null) return false;

            bool changed = root.State == UiWindowState.Closed;
            root.NotifyWindowState(UiWindowState.Opened);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                changed |= SetWindowBranchState(window, UiWindowState.Closed);
            }

            return changed;
        }

        private bool SetWindowBranchState(IUiWindow root, UiWindowState state) {
            if (root == null) return false;
            
            bool changed = root.State != state;
            root.NotifyWindowState(state);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                changed |= SetWindowBranchState(window, state);
            }

            return changed;
        }

        private static int GetWindowId(IUiWindow window) {
            return window.GameObject.GetHashCode();
        }
    }
    
}