using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Localization;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.Input.Actions;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        private readonly struct InputBinding {
            
            public readonly InputAction input;
            public readonly int buttonIndex;
            
            public InputBinding(InputAction input, int buttonIndex) {
                this.input = input;
                this.buttonIndex = buttonIndex;
            }
        }
        
        private ILocalizationService _localizationService;
        
        private readonly List<ButtonData> _buttons = new();
        private readonly List<InputBinding> _inputBindings = new();
        private readonly Dictionary<(LocalizationKey, int), ILocalizationFormatter.Lambda> _formatMap = new();

        private LocalizationKey _titleKey;
        private LocalizationKey _contentKey;
        private bool _canCloseOnNavigateBack;
        private int _navigateBackCallIndex = -1;
        private UniTaskCompletionSource _closeSource;

        private void OnEnable() {
            GetLocalizationService().OnLocaleChanged += OnLocaleChanged;

            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i].button;
                button.OnClicked -= OnClicked;
                button.OnClicked += OnClicked;
            }
            
            SubscribeInputs();
        }

        private void OnDisable() {
            // Dialog is disabled not by the close: complete the awaiters as well.
            CompleteClose();
            
            GetLocalizationService().OnLocaleChanged -= OnLocaleChanged;
            
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i].button;
                button.OnClicked -= OnClicked;
            }
            
            UnsubscribeInputs();
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

            if (firstSelected) _window.Node.SetCurrentSelectable(button.Selectable); 
            
            return this;
        }

        public IUiModalDialog BindButtonToInputs(int buttonIndex, IReadOnlyList<InputActionRef> inputs) {
            if (buttonIndex < 0 || buttonIndex >= _buttons.Count) {
                Debug.LogWarning($"{nameof(UiModalDialog)}: trying to bind inputs to button {buttonIndex}, " +
                                 $"but there are only {_buttons.Count} buttons, skip.");
                return this;
            }
            
            for (int i = 0; i < inputs?.Count; i++) {
                if (inputs[i].Get() is not { } input) continue;
                
                // Every input is subscribed once: an input bound to several buttons clicks the first one only.
                if (!IsInputBound(input)) input.performed += OnPerformInput;
                
                _inputBindings.Add(new InputBinding(input, buttonIndex));
            }
            
            return this;
        }

        public void Show() {
            // Show without the close: the awaiters of the previous show are completed.
            CompleteClose();
            _closeSource = new UniTaskCompletionSource();
            
            gameObject.SetActive(true);
            
            Services.Get<IUiWindowService>()?.SetWindowState(_window, UiWindowState.Opened);
            Services.Get<IUiNavigationService>()?.AddTopLayerNavigationCallback(this);
        }

        public void Close() {
            // Awaiters are completed only after the dialog is released to the pool,
            // so a continuation that shows a dialog again gets a clean one.
            var closeSource = _closeSource;
            _closeSource = null;
            
            ClearInputBindings();
            ClearButtons();
            
            _formatMap.Clear();
            _canCloseOnNavigateBack = false;
            _navigateBackCallIndex = -1;
            
            Services.Get<IUiWindowService>()?.SetWindowState(_window, UiWindowState.Closed);
            Services.Get<IUiNavigationService>()?.RemoveTopLayerNavigationCallback(this);
            
            PrefabPool.Main.Release(gameObject);
            
            closeSource?.TrySetResult();
        }

        public async UniTask AwaitClose(CancellationToken cancellationToken = default) {
            if (_closeSource == null) return;
            
            await _closeSource.Task.AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();
        }

        private void CompleteClose() {
            var closeSource = _closeSource;
            _closeSource = null;
            
            closeSource?.TrySetResult();
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
        
        private bool IsInputBound(InputAction input) {
            for (int i = 0; i < _inputBindings.Count; i++) {
                if (_inputBindings[i].input == input) return true;
            }
            
            return false;
        }

        private void SubscribeInputs() {
            for (int i = 0; i < _inputBindings.Count; i++) {
                var input = _inputBindings[i].input;
                input.performed -= OnPerformInput;
                input.performed += OnPerformInput;
            }
        }

        private void UnsubscribeInputs() {
            for (int i = 0; i < _inputBindings.Count; i++) {
                _inputBindings[i].input.performed -= OnPerformInput;
            }
        }
        
        private void ClearInputBindings() {
            UnsubscribeInputs();
            _inputBindings.Clear();
        }

        private void OnPerformInput(InputAction.CallbackContext context) {
            for (int i = 0; i < _inputBindings.Count; i++) {
                var binding = _inputBindings[i];
                if (binding.input != context.action) continue;

                if (binding.buttonIndex < _buttons.Count) _buttons[binding.buttonIndex].button.ClickManual();
                return;
            }
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