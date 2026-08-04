using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.UI.UiServices {
    
    public sealed class CursorService : ICursorService, IDisposable, IUpdate {

        private readonly HashSet<int> _visibilityBlockers = new();
        private IDeviceService _deviceService;
        private IUiWindowService _windowService;
        
        public void Initialize(IDeviceService deviceService, IUiWindowService windowService) {
            _deviceService = deviceService;
            _windowService = windowService;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            _visibilityBlockers.Clear();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public void BlockCursor(object source, bool block) {
            if (block) _visibilityBlockers.Add(source.GetHashCode());
            else _visibilityBlockers.Remove(source.GetHashCode());
        }

        void IUpdate.OnUpdate(float dt) {
            SetCursorVisible(IsCursorVisible());
        }

        private bool IsCursorVisible() {
            return !Application.isFocused || 
                   _visibilityBlockers.Count == 0 &&
                   _deviceService.CurrentDevice == InputDeviceType.KeyboardMouse && 
                   _windowService.IsCursorRequired();
        }
        
        private static void SetCursorVisible(bool visible) {
            Cursor.visible = visible;
            Cursor.lockState = visible 
                ? Application.isFocused ? CursorLockMode.Confined : CursorLockMode.None 
                : CursorLockMode.Locked;

#if UNITY_EDITOR
            Cursor.lockState = visible 
                ? CursorLockMode.None 
                : CursorLockMode.Locked;
#endif
        }
    }
    
}