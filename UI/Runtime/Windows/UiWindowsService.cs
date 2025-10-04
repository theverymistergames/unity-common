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
        private readonly Dictionary<int, int> _layerToFrontOpenedWindowIdMap = new();
        private readonly Dictionary<int, int> _layerToFocusedWindowIdMap = new();
        private readonly HashSet<int> _openedWindowIdsSet = new();
        private readonly HashSet<int> _openedWindowIdsWithCursorSet = new();

        public void Dispose() {
            _gameObjectIdToWindowMap.Clear();
            _childToParentMap.Clear();
            _relationTree.Clear();
            _layerToFrontOpenedWindowIdMap.Clear();
            _layerToFocusedWindowIdMap.Clear();
            _openedWindowIdsSet.Clear();
            _openedWindowIdsWithCursorSet.Clear();
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
                    OpenWindow(window, notify: true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UnregisterWindow(IUiWindow window) {
            int id = GetWindowId(window);

            if (_openedWindowIdsSet.Contains(id)) {
                CloseWindow(window, canOpenParent: false, forceClose: true, notify: true);
            }
            
            _gameObjectIdToWindowMap.Remove(id);
            
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
            return TryGetTopOpenedLayer(onlyNavigable: true, out int layer) ? GetFocusedWindow(layer) : null;
        }

        public IUiWindow GetFocusedWindow(int layer) {
            return _gameObjectIdToWindowMap.GetValueOrDefault(_layerToFocusedWindowIdMap.GetValueOrDefault(layer));
        }

        public IUiWindow GetFrontOpenedWindow() {
            return TryGetTopOpenedLayer(onlyNavigable: false, out int layer) ? GetFrontOpenedWindow(layer) : null;
        }

        public IUiWindow GetFrontOpenedWindow(int layer) {
            return _gameObjectIdToWindowMap.GetValueOrDefault(_layerToFrontOpenedWindowIdMap.GetValueOrDefault(layer));
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
            return _openedWindowIdsSet.Count > 0;
        }

        public bool HasOpenedWindows(out int topLayer) {
            return TryGetTopOpenedLayer(onlyNavigable: false, out topLayer);
        }

        public bool HasFocusedWindows() {
            return TryGetTopOpenedLayer(onlyNavigable: true, out _);
        }

        public bool HasFocusedWindows(out int topLayer) {
            return TryGetTopOpenedLayer(onlyNavigable: true, out topLayer);
        }

        public bool IsInOpenedBranch(IUiWindow window) {
            if (window == null || !TryGetWindowRootLayer(window, out int layer)) return false;
            
            var frontWindow = GetFrontOpenedWindow(layer);
            
            while (frontWindow?.GameObject != null) 
            {
                if (_openedWindowIdsSet.Contains(GetWindowId(frontWindow))) {
                    return true;
                }

                if (frontWindow == window) {
                    return false;
                }
                
                frontWindow = GetParentWindow(frontWindow);
            }
            
            return false;
        }

        public bool SetWindowState(IUiWindow window, UiWindowState state) {
            return state switch {
                UiWindowState.Closed => CloseWindow(window, canOpenParent: true, forceClose: false, notify: true),
                UiWindowState.Opened => OpenWindow(window, notify: true),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public UiWindowState GetWindowState(IUiWindow window) {
            return _openedWindowIdsSet.Contains(GetWindowId(window)) ? UiWindowState.Opened : UiWindowState.Closed;
        }

        public void NotifyWindowEnabled(IUiWindow window, bool enabled) {
            if (window == null) return;

            if (!enabled) {
                CloseWindow(window, canOpenParent: false, forceClose: true, notify: true);
                return;
            }

            var root = GetRootWindow(window);

            if (root == window && window.CloseMode == UiWindowCloseMode.NoExit) {
                OpenWindow(window, notify: true);
                return;
            }
            
            if (root != null && GetFrontOpenedWindow(root.Layer) is { } frontWindow) {
                OpenWindow(frontWindow, notify: true);
            }
        }

        public bool IsCursorRequired() {
            return _openedWindowIdsWithCursorSet.Count > 0;
        }

        private bool TryGetTopOpenedLayer(bool onlyNavigable, out int layer) {
            bool hasLayers = false;
            layer = 0;

            var map = onlyNavigable ? _layerToFocusedWindowIdMap : _layerToFrontOpenedWindowIdMap;
            
            foreach (int l in map.Keys) {
                if (hasLayers && l < layer) continue;
                
                layer = l;
                hasLayers = true;
            }

            return hasLayers;
        }
        
        private bool OpenWindow(IUiWindow window, bool notify) {
            if (!TryGetWindowRootLayer(window, out int layer)) return false;
            
            _layerToFrontOpenedWindowIdMap[layer] = GetWindowId(window);
            
            if ((window.Options & UiWindowOptions.IgnoreNavigation) == 0) {
                _layerToFocusedWindowIdMap[layer] = GetWindowId(window);
            }

            bool changed = OpenWindowBranch(window);
            var parentState = UiWindowState.Opened;
            
            while (window != null && _childToParentMap.TryGetValue(GetWindowId(window), out int parentId)) {
                int siblingsCount = _relationTree.GetCount(parentId);
                
                for (int i = 0; i < siblingsCount; i++) {
                    var sibling = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(parentId, i));
                    if (sibling != window) changed |= SetWindowBranchState(sibling, UiWindowState.Closed);
                }

                parentState = parentState switch {
                    UiWindowState.Closed => UiWindowState.Closed,
                    UiWindowState.Opened => window.OpenMode switch {
                        UiWindowOpenMode.Full => UiWindowState.Closed,
                        UiWindowOpenMode.Embedded => UiWindowState.Opened,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                var parent = _gameObjectIdToWindowMap.GetValueOrDefault(parentId);

                changed |= parent != null && parentState switch {
                    UiWindowState.Closed => _openedWindowIdsSet.Contains(parentId),
                    UiWindowState.Opened => !_openedWindowIdsSet.Contains(parentId),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (parent != null) {
                    WriteWindowState(parentId, parentState, parent.Options);
                    parent.NotifyWindowState(parentState);
                }
                
                window = parent;
            }
            
            if (changed && notify) OnHierarchyChanged();
            
            return changed;
        }

        private bool CloseWindow(IUiWindow window, bool canOpenParent, bool forceClose, bool notify) {
            if (window == null || !forceClose && window is { CloseMode: UiWindowCloseMode.NoExit }) return false;

            int id = GetWindowId(window);
            
            if (canOpenParent && _childToParentMap.TryGetValue(id, out int parentId)) {
                return OpenWindow(_gameObjectIdToWindowMap.GetValueOrDefault(parentId), notify);    
            }
            
            if (TryGetWindowRootLayer(window, out int layer) && 
                _layerToFrontOpenedWindowIdMap.TryGetValue(layer, out int frontWindowId) && frontWindowId == id) 
            {
                _layerToFrontOpenedWindowIdMap.Remove(window.Layer);
                _layerToFocusedWindowIdMap.Remove(window.Layer);
            }
            
            bool changed = SetWindowBranchState(window, UiWindowState.Closed);
            if (changed && notify) OnHierarchyChanged();
            
            return changed;
        }
        
        private void OnHierarchyChanged() {
            OnWindowsHierarchyChanged.Invoke();
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

            int id = GetWindowId(root);
            bool changed = GetWindowState(root) == UiWindowState.Closed;

            WriteWindowState(id, UiWindowState.Opened, root.Options);
            root.NotifyWindowState(UiWindowState.Opened);
            
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                changed |= SetWindowBranchState(window, UiWindowState.Closed);
            }

            return changed;
        }

        private bool SetWindowBranchState(IUiWindow root, UiWindowState state) {
            if (root == null) return false;
            
            int id = GetWindowId(root);
            bool changed = state != GetWindowState(root);
            
            WriteWindowState(id, state, root.Options);
            root.NotifyWindowState(state);
            
            int childCount = _relationTree.GetCount(id);
            
            for (int i = 0; i < childCount; i++) {
                var window = _gameObjectIdToWindowMap.GetValueOrDefault(_relationTree.GetValueAt(id, i));
                changed |= SetWindowBranchState(window, state);
            }

            return changed;
        }

        private void WriteWindowState(int id, UiWindowState state, UiWindowOptions options) {
            switch (state) {
                case UiWindowState.Closed:
                    _openedWindowIdsSet.Remove(id);
                    _openedWindowIdsWithCursorSet.Remove(id);
                    break;
                
                case UiWindowState.Opened:
                    _openedWindowIdsSet.Add(id);
                    
                    if ((options & UiWindowOptions.HideCursor) == 0) {
                        _openedWindowIdsWithCursorSet.Add(id);
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static int GetWindowId(IUiWindow window) {
            return window.GameObject.GetHashCode();
        }
    }
    
}