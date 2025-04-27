using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace MisterGames.UI.Services {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UIWindowsService : MonoBehaviour, IUIWindowService {
        
        public static IUIWindowService Instance { get; private set; }
        
        public event Action OnWindowsChanged = delegate { };

        private readonly HashSet<int> _openedWindows = new();

        private void Awake() {
            Instance = this;
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