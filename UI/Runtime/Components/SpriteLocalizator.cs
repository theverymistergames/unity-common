using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class SpriteLocalizator : MonoBehaviour {
        
        [SerializeField] private Image _image;
        [SerializeField] private LocalizationKey<Sprite> _key;
        [SerializeField] private bool _dontSetNull = true;

        private ILocalizationService _service;
        private Disposable<Sprite> _spriteRef;
        
        private void Awake() {
            _service = Services.Get<ILocalizationService>();
        }

        private void OnEnable() {
            _service.OnLocaleChanged += OnLocaleChanged;
            
            SetupValue();
        }

        private void OnDisable() {
            _service.OnLocaleChanged -= OnLocaleChanged;
            
            _spriteRef.Dispose();
        }

        private void OnLocaleChanged(Locale locale) {
            SetupValue();
        }

        private void SetupValue() {
            _spriteRef.Dispose();
            _spriteRef = _key.GetValue();
            
            if (!_dontSetNull || _spriteRef.value != null) _image.sprite = _spriteRef.value;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private Locale _defaultLocale = LocaleId.en.ToLocale();
        
        private void Reset() {
            _image = GetComponentInChildren<Image>();
        }

        [Button]
        private void FetchValueForDefaultLocale() {
            if (_key.IsNull() || _image == null) return;

            _spriteRef.Dispose();
            _spriteRef = _key.GetValue(_defaultLocale);
            
            if (_spriteRef.value == _image.sprite) return;
            
            _image.sprite = _spriteRef.value;
            EditorUtility.SetDirty(_image);
        }
#endif
    }
    
}