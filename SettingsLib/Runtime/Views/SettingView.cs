using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.SettingsLib.Base {

    public sealed class SettingView : MonoBehaviour {

        [SerializeField] private TMP_Text _title;
        [SerializeField] private LabelValue<ISettingDesc> _settingKey;
        [SerializeReference] [SubclassSelector] private ISettingBinder _binder;
        
        private void OnEnable() {
            _binder.Bind(Services.Get<ISettingsService>(), _settingKey.GetData(), _settingKey.GetLabel());
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged += OnLocaleChanged;
            }
            
            SetupBinder();
        }

        private void OnDisable() {
            _binder.Unbind();
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged -= OnLocaleChanged;
            }
        }

        private void OnLocaleChanged(Locale locale) {
            SetupBinder();
        }

        private void SetupBinder() {
            var desc = _settingKey.GetData();
            
            if (_title != null) {
                string title = desc?.GetName().GetValue() ?? "<unknown>";
                string oldTitle = _title.text;

                if (oldTitle != title) {
                    _title.SetText(title);
#if UNITY_EDITOR
                    if (!Application.isPlaying) EditorUtility.SetDirty(_title);
#endif
                }
            }
            
            if (_binder != null) {
                _binder.SetupView(desc);
                _binder.SetupValue();
            }
        }

#if UNITY_EDITOR
        [Button]
        private void ForceUpdateView() {
            SetupBinder();
        }
#endif
    }

}