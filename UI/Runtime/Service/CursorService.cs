using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.SceneManagement;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.UI.Service {
    
    public sealed class CursorService : ICursorService, IDisposable {

        private readonly HashSet<int> _visibilityBlockers = new();
        private CancellationTokenSource _cts;
        
        public void Initialize() {
            AsyncExt.RecreateCts(ref _cts);
            
            Application.focusChanged += OnApplicationFocusChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged += OnDeviceChanged;
            if (Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged += OnWindowsChanged;

            UpdateCursorVisibilityNextFrame(_cts.Token).Forget();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            Application.focusChanged -= OnApplicationFocusChanged;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged -= OnDeviceChanged;
            if (Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged -= OnWindowsChanged;
            
            _visibilityBlockers.Clear();
        }

        public void BlockCursor(object source, bool block) {
            if (block) _visibilityBlockers.Add(source.GetHashCode());
            else _visibilityBlockers.Remove(source.GetHashCode());
            
            UpdateCursorVisibility();
        }

        public void UpdateCursorVisibility() {
            SetCursorVisible(IsCursorVisible());
        }

        private async UniTask UpdateCursorVisibilityNextFrame(CancellationToken cancellationToken) {
            await UniTask.Yield();
            if (cancellationToken.IsCancellationRequested) return;
            
            UpdateCursorVisibility();
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1) {
            UpdateCursorVisibility();
        }

        private void OnApplicationFocusChanged(bool isFocused) {
            UpdateCursorVisibility();
        }

        private void OnDeviceChanged(DeviceType device) {
            UpdateCursorVisibility();
        }

        private void OnWindowsChanged() {
            UpdateCursorVisibility();
        }

        private bool IsCursorVisible() {
            return !Application.isFocused || 
                   _visibilityBlockers.Count == 0 &&
                   (!Services.TryGet(out IDeviceService deviceService) || deviceService.CurrentDevice == DeviceType.KeyboardMouse) && 
                   (!Services.TryGet(out IUiWindowService windowService) || windowService.IsCursorRequired());
        }
        
        private static void SetCursorVisible(bool visible) {
            Cursor.visible = visible;
            Cursor.lockState = visible 
                ? Application.isFocused ? CursorLockMode.Confined : CursorLockMode.None 
                : CursorLockMode.Locked;
        }
    }
    
}