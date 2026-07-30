using System;
using System.Collections.Generic;
using MisterGames.Common.Localization;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using TMPro;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class UiModalDialog : MonoBehaviour, IUiModalDialog, IUiNavigationCallback {
        
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _content;
        [SerializeField] private Transform _buttonsParent;
        [SerializeField] private UiButton _buttonPrefab;
        [SerializeField] private UiWindow _window;

        private readonly struct ButtonData {

            public readonly UiButton button;
            public readonly LocalizationKey key;
            public readonly Action action;
            public readonly bool closeOnClick;
            
            public ButtonData(LocalizationKey key, Action action, bool closeOnClick, UiButton button) {
                this.key = key;
                this.action = action;
                this.closeOnClick = closeOnClick;
                this.button = button;
            }
        }
        
        private ILocalizationService _localizationService;
        
        private readonly List<ButtonData> _buttons = new();
        private readonly Dictionary<(LocalizationKey, int), ILocalizationFormatter.Lambda> _formatMap = new();
        
        private LocalizationKey _titleKey;
        private LocalizationKey _contentKey;
        private bool _canCloseOnNavigateBack;
        private int _navigateBackCallIndex = -1;
        
        private void OnEnable() {
            GetLocalizationService().OnLocaleChanged += OnLocaleChanged;

            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i].button;
                button.OnClicked -= OnClicked;
                button.OnClicked += OnClicked;
            }
        }

        private void OnDisable() {
            GetLocalizationService().OnLocaleChanged -= OnLocaleChanged;
            
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i].button;
                button.OnClicked -= OnClicked;
            }
        }

        private void OnLocaleChanged(Locale locale) {
            _title.SetText(Format(_titleKey, 0, locale));
            _content.SetText(Format(_contentKey, 0, locale));
            
            for (int i = 0; i < _buttons.Count; i++) {
                var data = _buttons[i];
                data.button.ButtonText.SetText(Format(data.key, i, locale));
            }
        }

        public IUiModalDialog SetTitle(LocalizationKey title, ILocalizationFormatter.Lambda format = null) {
            _titleKey = title;
            _formatMap[(title, 0)] = format;
            _title.SetText(Format(_titleKey, 0, GetLocalizationService().Locale));
            return this;
        }

        public IUiModalDialog SetContent(LocalizationKey content, ILocalizationFormatter.Lambda format = null) {
            _contentKey = content;
            _formatMap[(content, 0)] = format;
            _content.SetText(Format(_contentKey, 0, GetLocalizationService().Locale));
            return this;
        }

        public IUiModalDialog SetBackNavigation(bool canCloseOnNavigateBack, int callButton = -1) {
            _canCloseOnNavigateBack = canCloseOnNavigateBack;
            _navigateBackCallIndex = callButton;
            return this;
        }

        public IUiModalDialog AddButton(
            LocalizationKey text,
            Action onClick,
            bool closeOnClick = true,
            bool firstSelected = true,
            ILocalizationFormatter.Lambda format = null) 
        {
            var button = PrefabPool.Main.Get(_buttonPrefab, _buttonsParent);
            button.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            button.transform.localScale = Vector3.one;
            
            button.ButtonText.SetText(text.GetValue());
            button.OnClicked -= OnClicked;
            button.OnClicked += OnClicked;

            int index = _buttons.Count;
            _buttons.Add(new ButtonData(text, onClick, closeOnClick, button));
            _formatMap[(text, index)] = format;
            button.ButtonText.SetText(Format(text, index, GetLocalizationService().Locale));

            if (firstSelected) _window.FirstSelected = button.Selectable; 
            
            return this;
        }

        public void Show() {
            gameObject.SetActive(true);
            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.AddTopLayerNavigationCallback(this);
            }
        }

        public void Close() {
            ClearButtons();
            _formatMap.Clear();
            _canCloseOnNavigateBack = false;
            _navigateBackCallIndex = -1;
            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.RemoveTopLayerNavigationCallback(this);
            }
            PrefabPool.Main.Release(gameObject);
        }

        public bool CanNavigateBack() {
            return _canCloseOnNavigateBack;
        }

        public void OnNavigateBack() {
            if (_navigateBackCallIndex >= 0 && _navigateBackCallIndex < _buttons.Count) {
                var data = _buttons[_navigateBackCallIndex];
                data.action?.Invoke();
                Close();
            }
        }

        private ILocalizationService GetLocalizationService() {
            return _localizationService ??= Services.Get<ILocalizationService>();
        }
        
        private string Format(LocalizationKey key, int index, Locale locale) {
            string text = key.GetValue();
            
            if (_formatMap.TryGetValue((key, index), out var f) && f != null) {
                f.Invoke(key, locale, ref text);
            }
            
            return text;
        }
        
        private void ClearButtons() {
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i].button;
                button.OnClicked -= OnClicked;
                PrefabPool.Main.Release(button);
            }
            
            _buttons.Clear();
        }

        private void OnClicked(UiButton button) {
            for (int i = 0; i < _buttons.Count; i++) {
                var data = _buttons[i];
                if (data.button != button) continue;
                
                data.action?.Invoke();
                if (data.closeOnClick) Close();
                return;
            }
        }
    }
    
}