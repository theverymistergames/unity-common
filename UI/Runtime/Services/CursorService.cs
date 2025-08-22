using System.Collections;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using UnityEngine;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9990)]
    public sealed class CursorService : MonoBehaviour, ICursorService {
        
        private void Awake() {
            Services.Register<ICursorService>(this);
        }

        private void OnDestroy() {
            Services.Unregister(this);
        }

        private void OnEnable() {
            Application.focusChanged += OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged += OnDeviceChanged;
            if (Services.TryGet(out IUIWindowService windowService)) windowService.OnWindowsChanged += OnWindowsChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged -= OnDeviceChanged;
            if (Services.TryGet(out IUIWindowService windowService)) windowService.OnWindowsChanged -= OnWindowsChanged;
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
                   Services.TryGet(out IUIWindowService windowService) && windowService.HasOpenedWindows() ||
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