using System;
using System.Text;
using MisterGames.Dbg.Console2.Attributes;
using MisterGames.Input.Actions;
using TMPro;
using UnityEngine;

namespace MisterGames.Dbg.Console2.Core {

    public class TestConsoleModule : IConsoleModule {

        [ConsoleCommand("logab")]
        public void LogIntBool(int a, bool b) {
            Debug.Log($"<color=yellow>{Time.realtimeSinceStartup} : {Time.frameCount}</color> :: " +
                      "Console2Runner.LogIntAndBool: " +
                      $"int [{a}], bool [{b}]");
        }

        [ConsoleCommand("loh")]
        public void Loh(string who) {
            Debug.Log($"{who} ЛОХ");
        }
    }

    public sealed class Console2Runner : MonoBehaviour {

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

        public static Console2Runner Instance { get; private set; }

        public event Action OnShowConsole = delegate {  };
        public event Action OnHideConsole = delegate {  };
        public event Action OnClearConsole = delegate {  };
        public event Action<string> OnBeforeRunCommand = delegate {  };

        public string CurrentInput => _textInputField.text;

        private Console _console;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private void Awake() {
            Instance = this;

            _console = new Console();
            _console.Initialize();

            SetTextFieldFontSize(_textFieldFontSize);
            SetTextInputFieldFontSize(_textInputFieldFontSize);
        }

        private void Start() {
            HideConsole();
            AppendText(_greeting);
            UpdateTextField();
        }

        private void OnDestroy() {
            ClearConsole();
            HideConsole();
            _console.DeInitialize();
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
            OnBeforeRunCommand.Invoke(input);

            AppendText($"<color=yellow>> {input}</color>");

            _console.Run(input);

            ResetTextInputField();
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

            OnClearConsole.Invoke();
        }
        
        private void OnPressActivationInput() {
            if (_canvas.activeSelf) HideConsole();
            else ShowConsole();
        }

        private void ShowConsole() {
            _canvas.SetActive(true);
            ResetTextInputField();
            
            _textInputField.onSubmit.RemoveListener(RunCommand);
            _textInputField.onSubmit.AddListener(RunCommand);

            OnShowConsole.Invoke();
        }
        
        private void HideConsole() {
            _canvas.SetActive(false);
            
            _textInputField.onSubmit.RemoveListener(RunCommand);

            OnHideConsole.Invoke();
        }

        private void AppendText(string text) {
            _stringBuilder.Append($"{text}\n");
            CheckTextFieldCapacity();
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
    }
    
}
