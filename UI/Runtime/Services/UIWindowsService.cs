using System;
using System.Collections.Generic;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UIWindowsService : MonoBehaviour, IUIWindowService {
        
        public event Action OnWindowsChanged = delegate { };

        private readonly HashSet<int> _openedWindows = new();

        private void Awake() {
            Services.Register<IUIWindowService>(this);
        }

        private void OnDestroy() {
            Services.Unregister(this);
        }

        public bool HasOpenedWindows() {
            return _openedWindows.Count > 0;
        }

        public void NotifyOpenedWindow(object source, bool opened) {
            bool changed = opened 
                ? _openedWindows.Add(source.GetHashCode()) 
                : _openedWindows.Remove(source.GetHashCode());
            
            if (!changed) return;
            
            OnWindowsChanged.Invoke();
        }
    }
    
}