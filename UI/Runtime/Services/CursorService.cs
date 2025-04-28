using System.Collections;
using MisterGames.Common.Inputs;
using UnityEngine;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.UI.Services {
    
    [DefaultExecutionOrder(-9990)]
    public sealed class CursorService : MonoBehaviour, ICursorService {
        
        public static ICursorService Instance { get; private set; }

        private void Awake() {
            Instance = this;
        }

        private void OnDestroy() {
            Instance = null;
        }

        private void OnEnable() {
            Application.focusChanged += OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged += OnDeviceChanged;
            UIWindowsService.Instance.OnWindowsChanged += OnWindowsChanged;
        }

        private void OnDisable() {
            Application.focusChanged -= OnApplicationFocusChanged;
            
            DeviceService.Instance.OnDeviceChanged -= OnDeviceChanged;
            UIWindowsService.Instance.OnWindowsChanged -= OnWindowsChanged;
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
            return DeviceService.Instance.CurrentDevice == DeviceType.KeyboardMouse && UIWindowsService.Instance.HasOpenedWindows() ||
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