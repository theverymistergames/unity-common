using System;
using System.Text;
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
        [TextArea] [SerializeField] private string _greeting;

        [Header("Inputs")]
        [SerializeField] private InputActionKey _activationInput;

        public static ConsoleRunner Instance { get; private set; }

        public event Action OnShowConsole = delegate {  };
        public event Action OnHideConsole = delegate {  };
        public event Action<string> OnBeforeRunCommand = delegate {  };

        public Console Console { get; private set; }
        public string CurrentInput => _textInputField.text;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private void Awake() {
            Instance = this;

            Console = new Console();
            Console.Initialize();

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
            Console.DeInitialize();
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

            Console.Run(input);
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
