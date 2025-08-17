using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    [DefaultExecutionOrder(-99_999)]
    public sealed class LoadingServiceRunner : MonoBehaviour {
        
        [SerializeField] private LoadingService _loadingService;
        
        private void Awake() {
            Services.Register<ILoadingService>(_loadingService);
            _loadingService.Initialize();
        }

        private void OnDestroy() {
            Services.Unregister(_loadingService);
        }
    }
    
}