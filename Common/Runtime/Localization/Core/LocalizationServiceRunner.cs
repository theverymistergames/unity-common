using MisterGames.Common.Attributes;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class LocalizationServiceRunner : MonoBehaviour {

        [SerializeField] private LocalizationSettings _localizationSettings;
        
        private readonly LocalizationService _localizationService = new();
        
        private void Awake() {
            _localizationService.Initialize(_localizationSettings);
            Services.Register<ILocalizationService>(_localizationService);
        }

        private void OnDestroy() {
            Services.Unregister(_localizationService);
            _localizationService.Dispose();
        }

#if UNITY_EDITOR
        [SerializeField] private Locale _debugLocale;
        
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        private void ApplyDebugLocale() {
            _localizationService.Locale = _debugLocale;
        }
#endif
    }
    
}