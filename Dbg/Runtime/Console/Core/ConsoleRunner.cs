using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Attributes;
using MisterGames.Input.Actions;
using TMPro;
using UnityEngine;

namespace MisterGames.Dbg.Console.Core {

    public sealed class ConsoleRunner : MonoBehaviour {

        [Header("UI")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private int _textFieldMaxCharacters = 10000;
        [SerializeField] private int _textFieldFontSize = 20;
        [SerializeField] private TMP_InputField _textInputField;
        [SerializeField] private int _textInputFieldFontSize = 18;
        [TextArea] [SerializeField] private string _greeting = "MisterGames Debug Console";

        [Header("Inputs")]
        [SerializeField] private InputActionKey _activationInput;

        [SerializeReference] [SubclassSelector] private IConsoleModule[] _consoleModules;

        public event Action OnShowConsole = delegate {  };
        public event Action OnHideConsole = delegate {  };
        public event Action<string> OnBeforeRunCommand = delegate {  };

        public string CurrentInput => _textInputField.text;
        internal IReadOnlyList<Command> Commands => _console.Commands;
        internal IReadOnlyList<IConsoleModule> ConsoleModules => _consoleModules;

        private readonly Console _console = new Console();
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private CursorLockMode _lastCursorLockMode;
        private bool _lastCursorVisibility;

        private void Awake() {
            for (int i = 0; i < _consoleModules.Length; i++) {
                var module = _consoleModules[i];
                module.ConsoleRunner = this;

                _console.AddModule(module);
            }

            SetTextFieldFontSize(_textFieldFontSize);
            SetTextInputFieldFontSize(_textInputFieldFontSize);
        }

        private void Start() {
            HideConsole();
            AppendLine(_greeting);
        }

        private void OnDestroy() {
            ClearConsole();
            HideConsole();

            _console.ClearModules();
        }

        private void OnEnable() {
            _activationInput.OnPress -= OnPressActivationInput;
            _activationInput.OnPress += OnPressActivationInput;
        }

        private void OnDisable() {
            ClearConsole();
            HideConsole();
            _activationInput.OnPress -= OnPressActivationInput;
        }

        public void RunCommand(string input) {
            input = input.Trim();

            OnBeforeRunCommand.Invoke(input);

            ResetTextInputField();
            AppendLine($"<color=yellow>> {input}</color>");

            _console.Run(input);
        }

        public void AppendLine(string text) {
            _stringBuilder.Append($"{text}\n");
            CheckTextFieldCapacity();
            UpdateTextField();
        }

        public void TypeIn(string input) {
            _textInputField.text = input;
            _textInputField.caretPosition = input.Length;
        }

        public void SetTextFieldFontSize(float size) {
            _textField.fontSize = size;
        }
        
        public void SetTextInputFieldFontSize(float size) {
            _textInputField.textComponent.fontSize = size;
        }
        
        public void ClearConsole() {
            _stringBuilder.Clear();
            UpdateTextField();
        }
        
        private void OnPressActivationInput() {
            if (_canvas.activeSelf) HideConsole();
            else ShowConsole();
        }

        private void ShowConsole() {
            _lastCursorLockMode = Cursor.lockState;
            _lastCursorVisibility = Cursor.visible;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _canvas.SetActive(true);
            ResetTextInputField();
            
            _textInputField.onSubmit.RemoveListener(RunCommand);
            _textInputField.onSubmit.AddListener(RunCommand);

            OnShowConsole.Invoke();
        }
        
        private void HideConsole() {
            _canvas.SetActive(false);
            
            _textInputField.onSubmit.RemoveListener(RunCommand);

            Cursor.visible = _lastCursorVisibility;
            Cursor.lockState = _lastCursorLockMode;

            OnHideConsole.Invoke();
        }

        private void UpdateTextField() {
            _textField.text = _stringBuilder.ToString();
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

#if UNITY_EDITOR
        public void RefreshModules(IReadOnlyList<IConsoleModule> newModules) {
            var existentConsoleModulesTypeMap = new Dictionary<Type, IConsoleModule>(_consoleModules.Length);
            for (int i = 0; i < _consoleModules.Length; i++) {
                var consoleModule = _consoleModules[i];
                if (consoleModule == null) continue;

                existentConsoleModulesTypeMap.Add(consoleModule.GetType(), consoleModule);
            }

            var targetConsoleModules = new List<IConsoleModule>();

            for (int i = 0; i < newModules.Count; i++) {
                var newModule = newModules[i];

                var moduleToAdd = existentConsoleModulesTypeMap.TryGetValue(newModule.GetType(), out var existentModule)
                    ? existentModule
                    : newModule;

                targetConsoleModules.Add(moduleToAdd);
            }

            _consoleModules = targetConsoleModules.ToArray();
        }
#endif
    }
    
}
