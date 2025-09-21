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

        public void RegisterWindow(IUiWindow window) {
            _gameObjectIdToWindowMap[GetWindowId(window)] = window;
            
            UpdateHierarchy(window.GameObject);
        }

        public void UnregisterWindow(IUiWindow window) {
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

        public void SetWindowState(IUiWindow window, UiWindowState state) {
            switch (state) {
                case UiWindowState.Closed:
                    CloseWindow(window);
                    break;
                
                case UiWindowState.Opened:
                    OpenWindow(window);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
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
        
        private void OpenWindow(IUiWindow window) {
            if (!TryGetWindowRootLayer(window, out int layer)) return;
            
            OpenWindowBranch(window);
            
            _layerToOpenedWindowIdMap[layer] = GetWindowId(window);
            
            var lastParentState = UiWindowState.Opened;
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out int parentId)) {
                int siblingsCount = _relationTree.GetCount(parentId);
                
                for (int i = 0; i < siblingsCount; i++) {
                    var sibling = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(parentId, i));
                    if (sibling != window) SetWindowBranchState(sibling, UiWindowState.Closed, focused: false);
                }

                lastParentState = lastParentState switch {
                    UiWindowState.Closed => UiWindowState.Closed,
                    UiWindowState.Opened => window.Mode switch {
                        UiWindowMode.Full => UiWindowState.Closed,
                        UiWindowMode.Embedded => UiWindowState.Opened,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                _gameObjectIdToWindowMap.GetValueOrDefault(parentId)?.NotifyWindowState(lastParentState, focused: false);
                
                window = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);
            }
            
            OnWindowsHierarchyChanged.Invoke();
        }

        private void CloseWindow(IUiWindow window) {
            if (window == null) return;

            int id = GetWindowId(window);
            
            if (_childToParentMap.TryGetValue(id, out int parentId)) {
                OpenWindow(_gameObjectIdToWindowMap.GetValueOrDefault(parentId));
                return;
            }

            var root = GetWindowRoot(window);
            if (root == window && root.IsRoot) return;

            if (_layerToOpenedWindowIdMap.TryGetValue(id, out int frontWindowId) && frontWindowId == id) {
                _layerToOpenedWindowIdMap.Remove(window.Layer);
            }
            
            SetWindowBranchState(window, UiWindowState.Closed, focused: false);
            
            OnWindowsHierarchyChanged.Invoke();
        }

        private bool TryGetWindowRootLayer(IUiWindow window, out int layer) {
            if (GetWindowRoot(window) is { } root) {
                layer = root.Layer;
                return true;
            }
            
            layer = 0;
            return false;
        }

        private IUiWindow GetWindowRoot(IUiWindow window) {
            if (window == null) return null;
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out int parentId)) {
                window = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);
            }
            
            return window;
        }
        
        private void OpenWindowBranch(IUiWindow root) {
            if (root == null) return;
            
            root.NotifyWindowState(UiWindowState.Opened, focused: true);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                SetWindowBranchState(window, UiWindowState.Closed, focused: false);
            }
        }

        private void SetWindowBranchState(IUiWindow root, UiWindowState state, bool focused) {
            if (root == null) return;
            
            root.NotifyWindowState(state, focused);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                SetWindowBranchState(window, state, focused);
            }
        }

        private static int GetWindowId(IUiWindow window) {
            return window.GameObject.GetHashCode();
        }
    }
    
}