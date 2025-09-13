using System;
using MisterGames.Input.Actions;
using MisterGames.UI.Data;
using UnityEngine.InputSystem;

namespace MisterGames.UI.Service {
    
    public sealed class UiNavigationService : IUiNavigationService, IDisposable {

        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _uiNavigationSettings;
        
        public void Initialize(IUiWindowService uiWindowService, UiNavigationSettings settings) {
            _uiWindowService = uiWindowService;
            _uiNavigationSettings = settings;
            
            _uiNavigationSettings.cancelInput.Get().performed += OnCancelInputPerformed;
        }

        public void Dispose() {
            _uiNavigationSettings.cancelInput.Get().performed -= OnCancelInputPerformed;
        }

        public void PerformCancel() {
            _uiWindowService.SetWindowState(_uiWindowService.GetFrontWindow(), UiWindowState.Closed);
        }

        private void OnCancelInputPerformed(InputAction.CallbackContext obj) {
            PerformCancel();
        }
    }
    
}