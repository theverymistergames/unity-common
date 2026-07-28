using System;
using System.Collections.Generic;
using MisterGames.Common.Labels;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Descs;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MisterGames.SettingsLib.Base {
    
    public sealed class InputBindingSchemeView : MonoBehaviour {

        [SerializeField] private InputViewData[] _inputViews;
        [SerializeField] private LabelValue<ISettingDesc>[] _gamepadBindingSettings;
        
        [Serializable]
        private struct InputViewData {
            public string controlPath;
            public TMP_Text textField;
            public Image lineImage;
        }

        private readonly Dictionary<string, KeyBindingSetting> _controlPathToBindingMap = new();
        private bool _subscribed;

        private void Awake() {
            FetchBindingMap();
        }

        private void OnEnable() {
            SubscribeBindings();

            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged += OnLocaleChanged;
            }
            
            UpdateInputViews();
        }

        private void OnDisable() {
            UnsubscribeBindings();
            
            if (Services.TryGet(out ILocalizationService localizationService)) {
                localizationService.OnLocaleChanged -= OnLocaleChanged;
            }
        }

        private void OnLocaleChanged(Locale obj) {
            UpdateInputViews();
        }

        private void OnNotifyBinding(string id, InputAction action, int bindingIndex, string path) {
            FetchBindingMap();
            UpdateInputViews();
        }

        private void UpdateInputViews() {
            for (int i = 0; i < _inputViews.Length; i++) {
                ref var inputView = ref _inputViews[i];
                
                if (!_controlPathToBindingMap.TryGetValue(inputView.controlPath, out var setting)) {
                    inputView.textField.SetText((string) null);
                    inputView.lineImage.enabled = false;
                    continue;
                }

                inputView.textField.SetText(setting.GetName().GetValue());
                inputView.lineImage.enabled = true;
            }
        }

        private void SubscribeBindings() {
            _subscribed = true;
            foreach (var setting in _controlPathToBindingMap.Values) {
                setting.AddBindingListener(OnNotifyBinding);
            }
        }

        private void UnsubscribeBindings() {
            foreach (var setting in _controlPathToBindingMap.Values) {
                setting.RemoveBindingListener(OnNotifyBinding);
            }
            _subscribed = false;
        }

        private void FetchBindingMap() {
            _controlPathToBindingMap.Clear();
            
            for (int i = 0; i < _gamepadBindingSettings.Length; i++) {
                var label = _gamepadBindingSettings[i];
                if (label.GetData() is not KeyBindingSetting setting) {
                    Debug.LogError($"{nameof(InputBindingSchemeView)} label value #{i} [{label}] is not a {nameof(KeyBindingSetting)}. " +
                                   $"Required setting desc of type {nameof(KeyBindingSetting)}.");
                    continue;
                }
                
                if (!setting.TryGetBinding(out _, out var binding, out int _)) continue;
                
                _controlPathToBindingMap[binding.effectivePath] = setting;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            if (_subscribed) UnsubscribeBindings();
            FetchBindingMap();
            if (_subscribed) SubscribeBindings();
        }
#endif
    }
    
}