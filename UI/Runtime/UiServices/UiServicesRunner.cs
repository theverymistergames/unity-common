using MisterGames.Common.Service;
using MisterGames.UI.Components;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;

namespace MisterGames.UI.UiServices {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UiServicesRunner : MonoBehaviour {
        
        [SerializeField] private UiNavigationSettings _uiNavigationSettings;
        [SerializeField] private UiModalDialog _defaultModalDialogPrefab;
        
        private readonly UiWindowsService _windowService = new();
        private readonly UiNavigationService _navigationService = new();
        private readonly CursorService _cursorService = new();
        private readonly CanvasRegistry _canvasRegistry = new();
        private readonly UiModalDialogService _uiModalDialogService = new();
        
        private void Awake() {
            Services.Register<IUiNavigationService>(_navigationService);
            Services.Register<IUiWindowService>(_windowService);
            Services.Register<ICursorService>(_cursorService);
            Services.Register<CanvasRegistry>(_canvasRegistry);
            Services.Register<IUiModalDialogService>(_uiModalDialogService);
            
            _uiModalDialogService.Initialize(_defaultModalDialogPrefab);
            _navigationService.Initialize(_windowService, _uiNavigationSettings);
            _cursorService.Initialize();
        }

        private void OnDestroy() {
            Services.Unregister(_windowService);
            Services.Unregister(_navigationService);
            Services.Unregister(_cursorService);
            Services.Unregister(_uiModalDialogService);
            
            _uiModalDialogService.Dispose();
            _windowService.Dispose();
            _navigationService.Dispose();
            _cursorService.Dispose();
            _canvasRegistry.Dispose();
        }
    }
    
}