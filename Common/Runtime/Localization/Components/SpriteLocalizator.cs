using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Localization.Components {
    
    public sealed class SpriteLocalizator : MonoBehaviour {
        
        [SerializeField] private Image _image;
        [SerializeField] private LocalizationKey<Sprite> _key;
        [SerializeField] private bool _dontSetNull = true;

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
            var sprite = _service.GetLocalizedAsset(_key);
            if (!_dontSetNull || sprite != null) _image.sprite = sprite;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInRuntime = false;
        [SerializeField] private Locale _defaultLocale = LocaleId.en.ToLocale();
        [HideInInspector] 
        [SerializeField] private LocalizationKey<Sprite> _lastKey;
        
        private void Reset() {
            _image = GetComponentInChildren<Image>();
        }

        private void OnValidate() {
            if (enabled && (!Application.isPlaying || _updateInRuntime) && _lastKey != _key) {
                FetchValueForDefaultLocale();
            }

            _lastKey = _key;
        }

        [Attributes.Button]
        private void FetchValueForDefaultLocale() {
            if (_key.IsNull() || _image == null) return;

            var sprite = _key.GetValue(_defaultLocale);
            if (sprite == _image.sprite) return;
            
            _image.sprite = sprite;
            EditorUtility.SetDirty(_image);
        }
#endif
    }
    
}