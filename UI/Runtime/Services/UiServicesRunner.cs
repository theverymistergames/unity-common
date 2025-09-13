using MisterGames.Common.Service;
using MisterGames.UI.Data;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UiServicesRunner : MonoBehaviour {
        
        [SerializeField] private UiNavigationSettings _uiNavigationSettings;
        
        private readonly UiWindowsService _windowService = new();
        private readonly UiNavigationService _navigationService = new();
        
        private void Awake() {
            _navigationService.Initialize(_windowService, _uiNavigationSettings);

            Services.Register<IUiNavigationService>(_navigationService);
            Services.Register<IUiWindowService>(_windowService);
        }

        private void OnDestroy() {
            Services.Unregister(_windowService);
            Services.Unregister(_navigationService);
            
            _windowService.Dispose();
            _navigationService.Dispose();
        }
    }
    
}