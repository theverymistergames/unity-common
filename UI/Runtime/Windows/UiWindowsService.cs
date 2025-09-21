using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public sealed class UiWindowsService : IUiWindowService, IDisposable {
        
        private readonly struct RelationData {
            
            public readonly int parentId;
            public readonly UiWindowMode mode;
            
            public RelationData(int parentId, UiWindowMode mode) {
                this.parentId = parentId;
                this.mode = mode;
            }
        }
        
        public event Action OnWindowsHierarchyChanged = delegate { };

        private readonly Dictionary<int, IUiWindow> _windowMap = new();
        private readonly Dictionary<int, RelationData> _childToParentMap = new();
        private readonly MultiValueDictionary<int, int> _relationTree = new();
        private readonly Dictionary<int, int> _layerToOpenedWindowIdMap = new();

        public void Dispose() {
            _windowMap.Clear();
            _childToParentMap.Clear();
            _relationTree.Clear();
            _layerToOpenedWindowIdMap.Clear();
        }

        public void RegisterWindow(IUiWindow window) {
            _windowMap[GetWindowId(window)] = window;
        }

        public void UnregisterWindow(IUiWindow window) {
            _windowMap.Remove(GetWindowId(window));
        }

        public void RegisterRelation(IUiWindow parent, IUiWindow child, UiWindowMode mode) {
            _relationTree.AddValue(GetWindowId(parent), GetWindowId(child));
            _childToParentMap[GetWindowId(child)] = new RelationData(GetWindowId(parent), mode);
        }
        
        public void UnregisterRelation(IUiWindow parent, IUiWindow child) {
            _relationTree.RemoveValue(GetWindowId(parent), GetWindowId(child));
            
            if (_childToParentMap.TryGetValue(GetWindowId(child), out var data) && data.parentId == GetWindowId(parent)) {
                _childToParentMap.Remove(GetWindowId(child));
            }
        }

        public IUiWindow GetFrontWindow() {
            return TryGetTopOpenedLayer(out int layer) && 
                   _windowMap.TryGetValue(_layerToOpenedWindowIdMap[layer], out var window)
                ? window 
                : null;
        }

        public IUiWindow GetParentWindow(IUiWindow child) {
            return _windowMap.GetValueOrDefault(_childToParentMap.GetValueOrDefault(GetWindowId(child)).parentId);
        }

        public IUiWindow GetClosestParentWindow(GameObject gameObject) {
            while (gameObject != null) {
                var window = _windowMap.GetValueOrDefault(gameObject.GetHashCode());
                if (window != null) return window;
                
                gameObject = gameObject.transform.parent?.gameObject;
            }

            return null;
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
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out var data)) {
                int siblingsCount = _relationTree.GetCount(data.parentId);
                
                for (int i = 0; i < siblingsCount; i++) {
                    var sibling = _windowMap.GetValueOrDefault(_relationTree.GetValueAt(data.parentId, i));
                    if (sibling != window) SetWindowBranchState(sibling, UiWindowState.Closed, focused: false);
                }

                lastParentState = lastParentState switch {
                    UiWindowState.Closed => UiWindowState.Closed,
                    UiWindowState.Opened => data.mode switch {
                        UiWindowMode.Full => UiWindowState.Closed,
                        UiWindowMode.Embedded => UiWindowState.Opened,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                _windowMap.GetValueOrDefault(data.parentId)?.NotifyWindowState(lastParentState, focused: false);
                
                window = _windowMap.GetValueOrDefault(data.parentId);
            }
            
            OnWindowsHierarchyChanged.Invoke();
        }

        private void CloseWindow(IUiWindow window) {
            if (window == null) return;

            int id = GetWindowId(window);
            
            if (_childToParentMap.TryGetValue(id, out var data)) {
                OpenWindow(_windowMap.GetValueOrDefault(data.parentId));
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
            
            while (_childToParentMap.TryGetValue(GetWindowId(window), out var data)) {
                window = _windowMap.GetValueOrDefault(data.parentId);
            }
            
            return window;
        }
        
        private void OpenWindowBranch(IUiWindow root) {
            if (root == null) return;
            
            root.NotifyWindowState(UiWindowState.Opened, focused: true);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _windowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                SetWindowBranchState(window, UiWindowState.Closed, focused: false);
            }
        }

        private void SetWindowBranchState(IUiWindow root, UiWindowState state, bool focused) {
            if (root == null) return;
            
            root.NotifyWindowState(state, focused);
            
            int id = GetWindowId(root);
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _windowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                SetWindowBranchState(window, state, focused);
            }
        }

        private static int GetWindowId(IUiWindow window) {
            return window.GameObject.GetHashCode();
        }
    }
    
}