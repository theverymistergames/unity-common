using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class UiListLocalizator : MonoBehaviour {
        
        [SerializeField] private UiList _uiList;
        [SerializeField] private LocalizationKey[] _keys;

        private ILocalizationService _service;
        
        private void Awake() {
            _service = Services.Get<ILocalizationService>();
        }
        
        private void OnEnable() {
            _service.OnLocaleChanged += OnLocaleChanged;
            
            SetupValues(_service.Locale);
        }

        private void OnDisable() {
            _service.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) {
            SetupValues(locale);
        }

        private void SetupValues(Locale locale) {
            int count = _keys?.Length ?? 0;
            _uiList.SetElementsCount(count);
            
            for (int i = 0; i < count; i++) {
                _uiList.SetElement(i, _service.GetLocalizedString(_keys![i], locale));
            }
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private Locale _defaultLocale = LocaleId.en.ToLocale();
        
        private void Reset() {
            _uiList = GetComponentInChildren<UiList>();
        }

        [Button]
        private void FetchValuesForDefaultLocale() {
            SetupValues(_defaultLocale);
        }
#endif
    }
    
}