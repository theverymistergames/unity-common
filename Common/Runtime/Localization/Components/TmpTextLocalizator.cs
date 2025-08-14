using MisterGames.Common.Service;
using TMPro;
using UnityEngine;

namespace MisterGames.Common.Localization.Components {
    
    public sealed class TmpTextLocalizator : MonoBehaviour {
        
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private LocalizationKey _key;

        private ILocalizationService _service;
        
        private void Awake() {
            _service = Services.Get<ILocalizationService>();
        }

        private void OnEnable() {
            _service.OnLocaleChanged += OnLocaleChanged;
            
            SetupText();
        }

        private void OnDisable() {
            _service.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) {
            SetupText();
        }

        private void SetupText() {
            _textField.text = _service.GetLocalizedString(_key);
        }

#if UNITY_EDITOR
        private void Reset() {
            _textField = GetComponentInChildren<TMP_Text>();
        }
#endif
    }
    
}