using System.Collections;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9990)]
    public sealed class CursorService : MonoBehaviour, ICursorService {
        
        private void Awake() {
            Common.Service.Services.Register<ICursorService>(this);
        }

        private void OnDestroy() {
            Common.Service.Services.Unregister(this);
        }

        private void OnEnable() {
            Application.focusChanged += OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged += OnDeviceChanged;
            if (Common.Service.Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged += OnWindowsChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged -= OnDeviceChanged;
            if (Common.Service.Services.TryGet(out IUiWindowService windowService)) windowService.OnWindowsHierarchyChanged -= OnWindowsChanged;
        }

        private IEnumerator Start() {
            yield return null;
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

        private bool IsCursorVisible() {
            return DeviceService.Instance.CurrentDevice == DeviceType.KeyboardMouse && 
                   Common.Service.Services.TryGet(out IUiWindowService windowService) && windowService.HasOpenedWindows() ||
                   !Application.isFocused;
        }
        
        private void SetCursorVisible(bool visible) {
            Cursor.visible = visible;
            Cursor.lockState = visible 
                ? Application.isFocused ? CursorLockMode.Confined : CursorLockMode.None 
                : CursorLockMode.Locked;
        }
    }
    
}