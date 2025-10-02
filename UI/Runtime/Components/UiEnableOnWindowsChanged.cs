using System;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.UI.Components {
    
    public sealed class UiEnableOnWindowsChanged : MonoBehaviour {
        
        [SerializeField] private int _layer;
        [SerializeField] private Mode _mode;
        [SerializeField] private Object[] _enableObjects;

        private enum Mode {
            EnableIfHasOpenedWindowsOfHigherOrEqualLayers,
            DisableIfHasOpenedWindowsOfHigherOrEqualLayers,
        }
        
        private void Awake() {
            if (Services.TryGet(out IUiWindowService service)) service.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            
            OnWindowsHierarchyChanged();
        }

        private void OnDestroy() {
            if (Services.TryGet(out IUiWindowService service)) service.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
        }

        private void OnWindowsHierarchyChanged() {
            if (!Services.TryGet(out IUiWindowService service)) return;

            bool hasOpenedWindows = service.HasOpenedWindows(out int topLayer);
            
            bool enable = _mode switch {
                Mode.EnableIfHasOpenedWindowsOfHigherOrEqualLayers => hasOpenedWindows && _layer <= topLayer,
                Mode.DisableIfHasOpenedWindowsOfHigherOrEqualLayers => !hasOpenedWindows || _layer > topLayer,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            _enableObjects.SetEnabled(enable);
        }

#if UNITY_EDITOR
        private void Reset() {
            _enableObjects = new Object[] { gameObject };
        }
#endif
    }
    
}