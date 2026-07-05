using MisterGames.Common.Save;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    [DefaultExecutionOrder(-10_000)]
    public sealed class SettingServiceRunner : MonoBehaviour {

        [SerializeField] private SettingsStorage _settingsStorage;
        [SerializeField] private string _storageId = "GameSettings";
        
        private readonly SettingsService _settingsService = new();
        
        private void Awake() {
            _settingsService.Initialize(_settingsStorage, SaveSystem.Main, _storageId);
            Services.Register<ISettingsService>(_settingsService);
        }

        private void OnDestroy() {
            Services.Unregister(_settingsService);
            _settingsService.Dispose();
        }
    }
    
}