using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.UI.Service {
    
    public sealed class CursorService : ICursorService, IDisposable {

        private CancellationTokenSource _cts;
        
        public void Initialize() {
            AsyncExt.RecreateCts(ref _cts);
            
            Application.focusChanged += OnApplicationFocusChanged;
            
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged += OnDeviceChanged;
            if (Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged += OnWindowsChanged;

            UpdateCursorVisibilityNextFrame(_cts.Token).Forget();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            Application.focusChanged -= OnApplicationFocusChanged;
            
            if (Services.TryGet(out IDeviceService deviceService)) deviceService.OnDeviceChanged -= OnDeviceChanged;
            if (Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged -= OnWindowsChanged;
        }

        private async UniTask UpdateCursorVisibilityNextFrame(CancellationToken cancellationToken) {
            await UniTask.Yield();
            if (cancellationToken.IsCancellationRequested) return;
            
            SetCursorVisible(IsCursorVisible());
        }

        private void OnApplicationFocusChanged(bool isFocused) {
            SetCursorVisible(IsCursorVisible());
        }

        private void OnDeviceChanged(DeviceType device) {
            SetCursorVisible(IsCursorVisible());
        }

        private void OnWindowsChanged() {
            SetCursorVisible(IsCursorVisible());
        }

        private static bool IsCursorVisible() {
            return !Application.isFocused || 
                   (!Services.TryGet(out IDeviceService deviceService) || deviceService.CurrentDevice == DeviceType.KeyboardMouse) && 
                   (!Services.TryGet(out IUiWindowService windowService) || windowService.HasOpenedWindows());
        }
        
        private static void SetCursorVisible(bool visible) {
            Cursor.visible = visible;
            Cursor.lockState = visible 
                ? Application.isFocused ? CursorLockMode.Confined : CursorLockMode.None 
                : CursorLockMode.Locked;
        }
    }
    
}