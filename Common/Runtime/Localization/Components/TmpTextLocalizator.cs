using MisterGames.Common.Service;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
            
            SetupValue();
        }

        private void OnDisable() {
            _service.OnLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged(Locale locale) {
            SetupValue();
        }

        private void SetupValue() {
            _textField.SetText(_service.GetLocalizedString(_key));
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInRuntime = false;
        [SerializeField] private Locale _defaultLocale = LocaleId.en.ToLocale();
        [HideInInspector] 
        [SerializeField] private LocalizationKey _lastKey;
        
        private void Reset() {
            _textField = GetComponentInChildren<TMP_Text>();
        }

        private void OnValidate() {
            if (enabled && (!Application.isPlaying || _updateInRuntime) && _lastKey != _key) {
                FetchValueForDefaultLocale();
                _lastKey = _key;
            }
        }

        [Attributes.Button]
        private void FetchValueForDefaultLocale() {
            if (_key.IsNull() || _textField == null) return;

            string text = _key.GetValue(_defaultLocale);
            if (text == _textField.text) return;
            
            _textField.SetText(text);
            EditorUtility.SetDirty(_textField);
        }
#endif
    }
    
}