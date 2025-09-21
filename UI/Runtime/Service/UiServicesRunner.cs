using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UiServicesRunner : MonoBehaviour {
        
        [SerializeField] private UiNavigationSettings _uiNavigationSettings;
        
        private readonly UiWindowsService _windowService = new();
        private readonly UiNavigationService _navigationService = new();
        private readonly CursorService _cursorService = new();
        
        private void Awake() {
            Services.Register<IUiNavigationService>(_navigationService);
            Services.Register<IUiWindowService>(_windowService);
            Services.Register<ICursorService>(_cursorService);
            
            _navigationService.Initialize(_windowService, _uiNavigationSettings);
            _cursorService.Initialize();
        }

        private void OnDestroy() {
            Services.Unregister(_windowService);
            Services.Unregister(_navigationService);
            Services.Unregister(_cursorService);
            
            _windowService.Dispose();
            _navigationService.Dispose();
            _cursorService.Dispose();
        }
    }
    
}