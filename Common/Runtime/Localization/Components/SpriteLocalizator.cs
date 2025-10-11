using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.UI;

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
        private void Reset() {
            _image = GetComponentInChildren<Image>();
        }
#endif
    }
    
}