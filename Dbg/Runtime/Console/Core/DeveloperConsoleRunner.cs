using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Common.Routines;
using MisterGames.Input.Actions;
using TMPro;
using UnityEngine;

namespace MisterGames.Dbg.Console.Core {

    public sealed class DeveloperConsoleRunner : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        
        [Header("UI")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private int _textFieldMaxCharacters = 10000;
        [SerializeField] private int _textFieldFontSize = 20;
        [SerializeField] private TMP_InputField _textInputField;
        [SerializeField] private int _textInputFieldFontSize = 18;
        [TextArea] [SerializeField] private string _greeting;

        [Header("Inputs")]
        [SerializeField] private InputActionKey _activationInput;
        [SerializeField] private InputActionKey _historyUpInput;
        [SerializeField] private InputActionKey _historyDownInput;

        [Header("Commands")]
        [SerializeField] private int _maxCommandHistorySize = 20;

        [BeginReadOnlyGroup]
        [SerializeReference] [SubclassSelector] private IConsoleCommand[] _consoleCommands = Array.Empty<IConsoleCommand>();

        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        private readonly List<string> _commandHistory = new List<string>();
        private string _historyCurrentInput;
        private int _historyPointer;
        
        private DeveloperConsole _console;
        private IConsoleCommandResult _currentResult;
        private IConsoleCommandResult _lastResult;

        private string _lastOutput;
        private bool IsShowingConsole => _canvas.activeSelf;

        private void Awake() {
            _console = new DeveloperConsole(this, _consoleCommands);
            
            OnHideConsole();
            AppendText(_greeting);
            SetTextFieldFontSize(_textFieldFontSize);
            SetTextInputFieldFontSize(_textInputFieldFontSize);
            UpdateTextField();
        }

        private void OnEnable() {
            _activationInput.OnPress -= OnPressActivationInput;
            _activationInput.OnPress += OnPressActivationInput;
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _activationInput.OnPress -= OnPressActivationInput;
            _timeDomain.UnsubscribeUpdate(this);
        }

        public void OnUpdate(float dt) {
            if (_currentResult == null) return;

            if (_lastResult == _currentResult) RemoveLastOutput();
            AppendText(_currentResult.Output);
            UpdateTextField();

            if (_currentResult.IsCompleted) {
                _currentResult = null;
                _lastResult = null;
                return;
            }
            
            _lastResult = _currentResult;
        }

        public void SetConsoleCommands(IConsoleCommand[] commands) {
            _consoleCommands = commands;
        }

        public void SetTextFieldFontSize(float size) {
            _textField.fontSize = size;
        }
        
        public void SetTextInputFieldFontSize(float size) {
            _textInputField.textComponent.fontSize = size;
        }
        
        public void ClearConsole() {
            _stringBuilder.Clear();
            _commandHistory.Clear();
            _historyPointer = 0;
            UpdateTextField();
        }
        
        private void OnPressActivationInput() {
            if (IsShowingConsole) OnHideConsole();
            else OnShowConsole();
        }

        private void OnShowConsole() {
            _canvas.SetActive(true);
            ResetTextInputField();
            
            _textInputField.onSubmit.RemoveListener(ProcessCommand);
            _textInputField.onSubmit.AddListener(ProcessCommand);
            
            _historyUpInput.OnPress -= OnHistoryUp;
            _historyUpInput.OnPress += OnHistoryUp;
            
            _historyDownInput.OnPress -= OnHistoryDown;
            _historyDownInput.OnPress += OnHistoryDown;

            _historyPointer = _commandHistory.Count;
        }
        
        private void OnHideConsole() {
            _canvas.SetActive(false);
            
            _textInputField.onSubmit.RemoveListener(ProcessCommand);
            
            _historyUpInput.OnPress -= OnHistoryUp;
            _historyDownInput.OnPress -= OnHistoryDown;
        }

        private void ProcessCommand(string input) {
            if (_currentResult is { IsCompleted: false }) return;
            
            AddCommandToHistory(input);
            _historyPointer = _commandHistory.Count;

            AppendText($"<color=yellow>> {input}</color>");
            
            _currentResult = _console.ProcessCommand(input);
            _lastResult = null;
            
            ResetTextInputField();
        }

        private void AddCommandToHistory(string input) {
            _commandHistory.Add(input);
            
            int length = _commandHistory.Count;
            if (length <= _maxCommandHistorySize) return;
            
            int lengthShouldBe = Mathf.FloorToInt(_maxCommandHistorySize * 0.7f);
            int toRemoveCount = length - lengthShouldBe;
            _commandHistory.RemoveRange(0, toRemoveCount);
        }
        
        private void OnHistoryUp() {
            if (_historyPointer == _commandHistory.Count) _historyCurrentInput = _textInputField.text;
            _historyPointer = Mathf.Max(_historyPointer - 1, 0);
            SetTextInputFieldFromHistory();
        }
        
        private void OnHistoryDown() {
            _historyPointer = Mathf.Min(_historyPointer + 1, _commandHistory.Count);
            SetTextInputFieldFromHistory();
        }
        
        private void AppendText(string text) {
            _lastOutput = $"{text}\n";
            _stringBuilder.Append(_lastOutput);
            CheckTextFieldCapacity();
        }

        private void RemoveLastOutput() {
            if (string.IsNullOrEmpty(_lastOutput) || _stringBuilder.Length < _lastOutput.Length) return;

            int start = _stringBuilder.Length - _lastOutput.Length;
            _stringBuilder.Remove(start, _lastOutput.Length);
        }
        
        private void UpdateTextField() {
            _textField.text = _stringBuilder.ToString();
        }

        private void SetTextInputFieldFromHistory() {
            string text = _commandHistory.IsEmpty() || _historyPointer == _commandHistory.Count
                ? _historyCurrentInput
                : _commandHistory[_historyPointer];
            
            _textInputField.text = text;
            _textInputField.caretPosition = text.Length;
        }
        
        private void ResetTextInputField() {
            _textInputField.text = string.Empty;
            _textInputField.ActivateInputField();
        }

        private void CheckTextFieldCapacity() {
            int length = _stringBuilder.Length;
            if (length <= _textFieldMaxCharacters) return;

            int lengthShouldBe = Mathf.FloorToInt(_textFieldMaxCharacters * 0.7f);
            int toRemoveCount = length - lengthShouldBe;
            _stringBuilder.Remove(0, toRemoveCount);
        }
    }
    
}
