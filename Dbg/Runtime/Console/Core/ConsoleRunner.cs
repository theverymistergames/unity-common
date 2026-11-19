using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MisterGames.Common.Attributes;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Common.Steam;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [SerializeField] private InputActionRef _activationInput;
        [Tooltip("Closes console on release, so actions bound to the same key and blocked while console is opened " +
                 "are not triggered by the initial state check when console is closed and they are unblocked.")]
        [SerializeField] private InputActionRef _closeInput;
        [SerializeReference] [SubclassSelector] private IConsoleModule[] _consoleModules;

        private const string Editor = "editor";  
        
        public event Action OnShowConsole = delegate {  };
        public event Action OnHideConsole = delegate {  };
        public event Action<string> OnBeforeRunCommand = delegate {  };

        public string CurrentInput => _textInputField.text;
        internal IReadOnlyList<Command> Commands => _console.Commands;
        internal IReadOnlyList<IConsoleModule> ConsoleModules => _consoleModules;

        public static ConsoleRunner Instance { get; private set; }
        
        private readonly Console _console = new();
        private readonly StringBuilder _stringBuilder = new();
        private CursorLockMode _lastCursorLockMode;
        private bool _lastCursorVisibility;
        private bool _isFloatingKeyboardShown;

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD

        private void Awake() {
            Destroy(gameObject);
        }

#else

        private void Awake() {
            Instance = this;
            
            FetchConsoleModules();
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
            InputServices.Blocks?.SetInputActionBlockOverride(this, _activationInput, blocked: false);
            InputServices.Blocks?.SetInputActionBlockOverride(this, _closeInput, blocked: false);
            
            if (_activationInput.Get() is not { } inputAction) return;
            
            inputAction.performed -= OnPressActivationInput;
            inputAction.performed += OnPressActivationInput;
        }

        private void OnDisable() {
            if (_activationInput.Get() is { } inputAction) inputAction.performed -= OnPressActivationInput;
            
            InputServices.Blocks?.RemoveInputActionBlockOverride(this, _activationInput);
            InputServices.Blocks?.RemoveInputActionBlockOverride(this, _closeInput);
            
            ClearConsole();
            HideConsole();
        }

#endif

        public void RunCommand(string input) {
            input = input.Trim();

            OnBeforeRunCommand.Invoke(input);

            ResetTextInputField();
            AppendLine($"<color=yellow>> {input}</color>");

            if (input.Length > 0 && !_console.Run(input)) {
                AppendLine($"Command {input} is not found or has invalid arguments. " +
                           $"Arguments with spaces must be in quotes: {ConsoleArgs.Quote}some argument{ConsoleArgs.Quote}. Type help to see list of all commands.");
            }
        }

        public void AppendLine(string text) {
            _stringBuilder.Append($"{text}\n");
            CheckTextFieldCapacity();
            UpdateTextField();
        }

        public void TypeIn(string input) {
            _textInputField.text = input;
            _textInputField.caretPosition = input?.Length ?? 0;
        }

        public void MoveCaretToEnd() {
            _textInputField.caretPosition = _textInputField.text?.Length ?? 0;
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

        private void FetchConsoleModules() {
            var types =

#if UNITY_EDITOR
                TypeCache.GetTypesDerivedFrom<IConsoleModule>()
#else
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(assembly => !assembly.FullName.Contains(Editor, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => typeof(IConsoleModule).IsAssignableFrom(t))
#endif
                .Where(t =>
                    typeof(IConsoleModule).IsAssignableFrom(t) &&
                    (t.IsPublic || t.IsNestedPublic) &&
                    t.IsVisible &&
                    !t.IsAbstract &&
                    !t.IsGenericType
                )
                .ToArray();

            var map = DictionaryPool<Type, IConsoleModule>.Get();

            for (int i = 0; i < _consoleModules.Length; i++) {
                if (_consoleModules[i] is {} module) map[module.GetType()] = module;
            }

            for (int i = 0; i < types.Length; i++) {
                var type = types[i];
                if (!map.ContainsKey(type)) map[type] = Activator.CreateInstance(type) as IConsoleModule; 
            }
            
            _consoleModules = map.Values.ToArray();
            DictionaryPool<Type, IConsoleModule>.Release(map);

            for (int i = 0; i < _consoleModules.Length; i++) {
                var module = _consoleModules[i];
                module.ConsoleRunner = this;

                _console.AddModule(module);
            }
        }
        
        private void OnPressActivationInput(InputAction.CallbackContext context) {
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

            if (_closeInput.Get() is { } inputAction) {
                inputAction.canceled -= OnReleaseCloseInput;
                inputAction.canceled += OnReleaseCloseInput;
            }
            
            if (IsSteamDeckGamepadInput()) ShowFloatingKeyboard();
            
            OnShowConsole.Invoke();
        }
        
        public bool ShowFloatingKeyboard() {
            _isFloatingKeyboardShown = SteamExt.ShowFloatingKeyboard(GetInputFieldScreenRect());
            return _isFloatingKeyboardShown;
        }

        private static bool IsSteamDeckGamepadInput() {
            return Services.TryGet(out IDeviceService deviceService) && 
                   deviceService.CurrentDevice == InputDeviceType.Gamepad && 
                   deviceService.GamepadType == GamepadType.SteamDeck;
        }

        private Rect GetInputFieldScreenRect() {
            Canvas.ForceUpdateCanvases();
            
            var rectTransform = (RectTransform) _textInputField.transform;
            var canvas = _textInputField.GetComponentInParent<Canvas>();
            var camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            var min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        
        private void OnReleaseCloseInput(InputAction.CallbackContext context) {
            HideConsole();
        }
        
        private void HideConsole() {
            _canvas.SetActive(false);
            
            if (_isFloatingKeyboardShown) SteamExt.DismissFloatingKeyboard();
            _isFloatingKeyboardShown = false;
            
            _textInputField.onSubmit.RemoveListener(RunCommand);
            
            if (_closeInput.Get() is { } inputAction) inputAction.canceled -= OnReleaseCloseInput;

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
    }
    
}
