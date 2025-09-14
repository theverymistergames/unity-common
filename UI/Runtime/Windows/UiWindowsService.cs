using System;
using System.Collections.Generic;
using MisterGames.Common.Data;

namespace MisterGames.UI.Windows {
    
    public sealed class UiWindowsService : IUiWindowService, IDisposable {

        private readonly struct WindowData {
            
            public readonly IUiWindow parent;
            public readonly UiWindowMode mode;
            
            public WindowData(IUiWindow parent, UiWindowMode mode) {
                this.parent = parent;
                this.mode = mode;
            }
        }
        
        public event Action OnWindowsHierarchyChanged = delegate { };

        private readonly Dictionary<IUiWindow, int> _windowToLayerMap = new();
        private readonly Dictionary<IUiWindow, WindowData> _childToParentMap = new();
        private readonly MultiValueDictionary<IUiWindow, IUiWindow> _relationTree = new();
        private readonly Dictionary<int, IUiWindow> _layerToOpenedWindowMap = new();

        public void Dispose() {
            _windowToLayerMap.Clear();
            _childToParentMap.Clear();
            _relationTree.Clear();
            _layerToOpenedWindowMap.Clear();
        }

        public void RegisterWindow(IUiWindow window, int layer) {
            _windowToLayerMap.Add(window, layer);
        }

        public void UnregisterWindow(IUiWindow window) {
            _windowToLayerMap.Remove(window);
        }

        public void RegisterRelation(IUiWindow parent, IUiWindow child, UiWindowMode mode) {
            _relationTree.AddValue(parent, child);
            _childToParentMap[child] = new WindowData(parent, mode);
        }
        
        public void UnregisterRelation(IUiWindow parent, IUiWindow child) {
            _relationTree.RemoveValue(parent, child);
            
            if (_childToParentMap.TryGetValue(child, out var data) && data.parent == parent) {
                _childToParentMap.Remove(child);
            }
        }

        public IUiWindow GetFrontWindow() {
            return TryGetTopOpenedLayer(out int layer) ? _layerToOpenedWindowMap[layer] : null;
        }

        public IUiWindow GetParentWindow(IUiWindow child) {
            return _childToParentMap.GetValueOrDefault(child).parent;
        }

        public bool HasOpenedWindows() {
            return _layerToOpenedWindowMap.Count > 0;
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
            
            foreach (int l in _layerToOpenedWindowMap.Keys) {
                if (hasLayers && l < layer) continue;
                
                layer = l;
                hasLayers = true;
            }
            
            return hasLayers;
        }
        
        private void OpenWindow(IUiWindow window) {
            if (!TryGetWindowRootLayer(window, out int layer)) return;
            
            SetWindowBranchStates(window, rootState: UiWindowState.Opened, childStates: UiWindowState.Closed);
            _layerToOpenedWindowMap[layer] = window;
            
            var lastParentState = UiWindowState.Opened;
            
            while (_childToParentMap.TryGetValue(window, out var data)) {
                int siblingsCount = _relationTree.GetCount(data.parent);
                
                for (int i = 0; i < siblingsCount; i++) {
                    var sibling = _relationTree.GetValueAt(data.parent, i);
                    if (sibling != window) SetWindowBranchState(sibling, UiWindowState.Closed);
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
                
                data.parent.NotifyWindowState(lastParentState);
                
                window = data.parent;
            }
            
            OnWindowsHierarchyChanged.Invoke();
        }

        private void CloseWindow(IUiWindow window) {
            if (window == null) return;

            if (_childToParentMap.TryGetValue(window, out var data)) {
                OpenWindow(data.parent);
                return;
            }

            var root = GetWindowRoot(window);
            if (root == window && root.IsRoot) return;
            
            if (_windowToLayerMap.TryGetValue(root, out int layer)) {
                _layerToOpenedWindowMap.Remove(layer);
            }
            
            SetWindowBranchState(window, UiWindowState.Closed);
            
            OnWindowsHierarchyChanged.Invoke();
        }

        private bool TryGetWindowRootLayer(IUiWindow window, out int layer) {
            if (GetWindowRoot(window) is { } root && 
                _windowToLayerMap.TryGetValue(root, out layer)) 
            {
                return true;
            }
            
            layer = 0;
            return false;
        }

        private IUiWindow GetWindowRoot(IUiWindow window) {
            if (window == null) return null;
            
            while (_childToParentMap.TryGetValue(window, out var data)) {
                window = data.parent;
            }
            
            return window;
        }
        
        private void SetWindowBranchStates(IUiWindow root, UiWindowState rootState, UiWindowState childStates) {
            root.NotifyWindowState(rootState);
            
            int childCount = _relationTree.GetCount(root);
            
            for (int i = 0; i < childCount; i++) {
                SetWindowBranchState(_relationTree.GetValueAt(root, i), childStates);
            }
        }

        private void SetWindowBranchState(IUiWindow root, UiWindowState state) {
            root.NotifyWindowState(state);
            
            int childCount = _relationTree.GetCount(root);
            
            for (int i = 0; i < childCount; i++) {
                SetWindowBranchState(_relationTree.GetValueAt(root, i), state);
            }
        }
    }
    
}