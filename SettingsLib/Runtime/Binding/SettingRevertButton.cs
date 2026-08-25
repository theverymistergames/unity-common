using System;
using System.Collections.Generic;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.UI.Components;
using MisterGames.UI.UiServices;
using UnityEngine;

namespace MisterGames.SettingsLib.Base {
    
    public sealed class SettingRevertButton : MonoBehaviour {

        [Header("Button")]
        [SerializeField] private UiButton _button;
        
        [Header("Reverting")]
        [SerializeField] private Mode _mode;
        [SerializeField] private Target _target;
        [SerializeField] private LabelArray<ISettingDesc>[] _arrays;
        [SerializeField] private LabelValue<ISettingDesc>[] _settings;
        
        [Header("Modal Dialog")]
        [SerializeField] private bool _showModalDialog = true;
        [SerializeField] private LocalizationKey _title;
        [SerializeField] private LocalizationKey _content;
        [SerializeField] private LocalizationKey _okButton;
        [SerializeField] private LocalizationKey _cancelButton;
        
        private enum Target {
            AllSettings,
            SettingArrays,
            SeparateSettings,
        }
        
        private enum Mode {
            RevertToDefault,
            RevertToLastSaved,
        }
            
        private void OnEnable() {
            _button.OnClicked += OnClicked;
        }

        private void OnDisable() {
            _button.OnClicked -= OnClicked;
        }

        private void OnClicked(UiButton obj) {
            if (!_showModalDialog) {
                ApplyRevert();
                return;
            }

            Debug.Log($"SettingRevertButton.OnClicked: f {Time.frameCount}, ");
            
            var parentCanvas = Services.Get<CanvasRegistry>().FindClosestParentCanvas(transform);
            Services.Get<IUiModalDialogService>().CreateModalDialogDefault(parentCanvas)
                .SetTitle(_title)
                .SetContent(_content)
                .AddButton(_okButton, ApplyRevert)
                .AddButton(_cancelButton, () => { })
                .SetBackNavigation(canCloseOnNavigateBack: true, callButton: 1)
                .Show();
        }

        private void ApplyRevert() {
            if (!Services.TryGet(out ISettingsService service)) return;

            Debug.Log($"SettingRevertButton.ApplyRevert: f {Time.frameCount}, ");
            
            var set = GetKeys(_target);
            
            switch (_mode) {
                case Mode.RevertToDefault:
                    if (set == null) service.RevertToDefaultSettings();
                    else service.RevertToDefaultSettings(set);
                    break;
                
                case Mode.RevertToLastSaved:
                    if (set == null) service.RevertToLastSavedSettings();
                    else service.RevertToLastSavedSettings(set);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private HashSet<string> GetKeys(Target target) {
            switch (target) {
                case Target.AllSettings:
                    return null;
                
                case Target.SettingArrays: {
                    var set = new HashSet<string>();
                    
                    for (int i = 0; i < _arrays.Length; i++) {
                        var array = _arrays[i];
                        int count = array.GetValuesCount();
                        for (int j = 0; j < count; j++) {
                            set.Add(array.GetLabelValue(j).GetFullLabel());
                        }
                    }
                    
                    return set;
                }

                case Target.SeparateSettings: {
                    var set = new HashSet<string>();

                    for (int i = 0; i < _settings.Length; i++) {
                        set.Add(_settings[i].GetFullLabel());
                    }

                    return set;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
    
}