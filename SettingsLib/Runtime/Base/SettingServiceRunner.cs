using MisterGames.Common.Save;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [DefaultExecutionOrder(-10_000)]
    public sealed class SettingServiceRunner : MonoBehaviour {

        [SerializeField] private SettingsConfig _settingsConfig;
        
        private readonly SettingsService _settingsService = new();
        
        private void Awake() {
            _settingsService.Initialize(_settingsConfig, SaveSystem.Main);
            Services.Register<ISettingsService>(_settingsService);
        }

        private void OnDestroy() {
            Services.Unregister(_settingsService);
            _settingsService.Dispose();
        }
    }
    
}