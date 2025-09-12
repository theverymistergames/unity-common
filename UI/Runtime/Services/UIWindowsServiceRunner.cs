using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.Service {
    
    [DefaultExecutionOrder(-9999)]
    public sealed class UIWindowsServiceRunner : MonoBehaviour {
        
        private readonly UIWindowsService _service = new();
        
        private void Awake() {
            Services.Register<IUIWindowService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
            _service.Dispose();
        }
    }
    
}