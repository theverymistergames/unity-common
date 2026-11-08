using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class TmpTextLocalizator : MonoBehaviour {
        
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private LocalizationKey _key;
        [SerializeReference] [SubclassSelector] private IArgumentResolver _formatter;
        
        private ILocalizationService _service;
        
        private void Awake() {
            _service = Services.Get<ILocalizationService>();
        }

        private void OnEnable() {
            _service.OnLocaleChanged += OnLocaleChanged;
            
            SetupValue();
        }

        private void OnDisable() {
            _service.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) {
            SetupValue();
        }

        private void SetupValue() {
            _textField.SetText(GetResultText(_service.Locale));
        }

        private string GetResultText(Locale locale) {
            string value = _key.GetValue(locale);
            _formatter?.Resolve(_key, locale, ref value);
            return value;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private Locale _defaultLocale = LocaleId.en.ToLocale();
        
        private void Reset() {
            _textField = GetComponentInChildren<TMP_Text>();
        }

        [Button]
        private void FetchValueForDefaultLocale() {
            if (_key.IsNull() || _textField == null) return;

            string text = GetResultText(_defaultLocale);
            if (text == _textField.text) return;
            
            _textField.SetText(text);
            EditorUtility.SetDirty(_textField);
        }
#endif
    }
    
}