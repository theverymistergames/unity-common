using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using TMPro;
using UnityEngine;

namespace MisterGames.Dbg.Console.Plugins {

    [RequireComponent(typeof(ConsoleRunner))]
    public sealed class ConsoleSuggestions : MonoBehaviour {

        [Header("UI")]
        [SerializeField] private RectTransform _suggestionsRoot;
        [SerializeField] private TMP_Text _suggestionsText;
        [SerializeField] [Range(1, 5)] private int _maxLines = 5;
        [SerializeField] [Min(0f)] private float _verticalPadding = 8f;
        [SerializeField] private Color _selectedColor = new(1f, 0.92f, 0.016f, 1f);
        [SerializeField] private Color _hintColor = new(0.6f, 0.6f, 0.6f, 1f);

        [Header("Inputs")]
        [SerializeField] private KeyBinding _nextSuggestionInput = KeyBinding.Tab;
        [SerializeField] private ShortcutModifiers _previousSuggestionModifiers = ShortcutModifiers.Shift;

        [Header("Suggestions")]
        [SerializeField] [Min(1)] private int _maxSuggestions = 100;

        private struct Suggestion {
            // Text to put into the input field, null if suggestion is just a hint and cannot be selected.
            public string input;
            public string label;
            public string hint;
        }

        private readonly List<Suggestion> _suggestions = new();
        private readonly List<string> _argSuggestions = new();
        private readonly List<string> _args = new();
        private readonly List<int> _argStarts = new();
        private readonly HashSet<string> _addedInputs = new();
        private readonly HashSet<IConsoleSuggestionProvider> _providers = new();
        private readonly StringBuilder _sb = new();

        private ConsoleRunner _consoleRunner;
        private string _lastInput;
        private int _selected = -1;
        private bool _isConsoleShown;

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD

        private void Awake() {
            Destroy(this);
        }

#else

        private void Awake() {
            _consoleRunner = GetComponent<ConsoleRunner>();
        }

        private void OnEnable() {
            _consoleRunner.OnShowConsole -= OnShowConsole;
            _consoleRunner.OnShowConsole += OnShowConsole;

            _consoleRunner.OnHideConsole -= OnHideConsole;
            _consoleRunner.OnHideConsole += OnHideConsole;

            ClearSuggestions();
        }

        private void OnDisable() {
            _consoleRunner.OnShowConsole -= OnShowConsole;
            _consoleRunner.OnHideConsole -= OnHideConsole;

            OnHideConsole();
        }

        private void Update() {
            if (!_isConsoleShown) return;

            string input = _consoleRunner.CurrentInput ?? string.Empty;
            if (input == _lastInput) return;

            _lastInput = input;
            Rebuild(input);
        }

#endif

        private void OnShowConsole() {
            _isConsoleShown = true;
            _lastInput = null;

            _nextSuggestionInput.RemovePressCallback(OnNextSuggestion);
            _nextSuggestionInput.AddPressCallback(OnNextSuggestion);
        }

        private void OnHideConsole() {
            _isConsoleShown = false;
            _nextSuggestionInput.RemovePressCallback(OnNextSuggestion);

            ClearSuggestions();
        }

        private void OnNextSuggestion() {
            int count = _suggestions.Count;
            if (count == 0 || _suggestions[0].input == null) return;

            bool previous = _previousSuggestionModifiers != ShortcutModifiers.None &&
                            _previousSuggestionModifiers.ArePressed();

            _selected = previous
                ? _selected <= 0 ? count - 1 : _selected - 1
                : (_selected + 1) % count;

            string input = _suggestions[_selected].input;

            _consoleRunner.TypeIn(input);
            _lastInput = input;

            // The only suggestion is applied: show suggestions for the next part of the input right away.
            if (count == 1) {
                Rebuild(input);
                return;
            }

            Redraw();
        }

        private void ClearSuggestions() {
            _suggestions.Clear();
            _selected = -1;

            if (_suggestionsRoot != null) _suggestionsRoot.gameObject.SetActive(false);
        }

        private void Rebuild(string input) {
            _suggestions.Clear();
            _addedInputs.Clear();
            _selected = -1;

            input = input.TrimStart();

            if (input.Length > 0 && !TryCollectArgumentSuggestions(input)) {
                CollectCommandSuggestions(input);
            }

            Redraw();
        }

        private bool TryCollectArgumentSuggestions(string input) {
            var commands = _consoleRunner.Commands;
            string cmd = null;

            // Find the longest command followed by a space, commands can contain spaces.
            for (int i = 0; i < commands.Count; i++) {
                string c = commands[i].cmd;
                if (!IsCommandWithArgs(input, c) || cmd != null && cmd.Length >= c.Length) continue;

                cmd = c;
            }

            if (cmd == null) return false;

            ParseArgs(input, cmd, _args, _argStarts, out string inputBeforeLastArg);

            _providers.Clear();

            for (int i = 0; i < commands.Count; i++) {
                var command = commands[i];
                if (command.cmd != cmd || command.module is not IConsoleSuggestionProvider provider) continue;
                if (!_providers.Add(provider)) continue;

                _argSuggestions.Clear();
                provider.CollectSuggestions(cmd, _args, _argSuggestions);

                for (int j = 0; j < _argSuggestions.Count && _suggestions.Count < _maxSuggestions; j++) {
                    string arg = _argSuggestions[j];
                    if (string.IsNullOrEmpty(arg)) continue;

                    string text = inputBeforeLastArg + ConsoleArgs.QuoteIfNeeded(arg);
                    if (!_addedInputs.Add(text)) continue;

                    _suggestions.Add(new Suggestion { input = text, label = text });
                }
            }

            if (_suggestions.Count > 0) return true;

            // No argument suggestions: show command usage.
            for (int i = 0; i < commands.Count && _suggestions.Count < _maxSuggestions; i++) {
                var command = commands[i];
                if (command.cmd != cmd) continue;

                _suggestions.Add(new Suggestion {
                    label = $"{cmd} {GetParametersText(command.method)}".TrimEnd(),
                    hint = GetHelpText(command.method),
                });
            }

            return true;
        }

        private void CollectCommandSuggestions(string input) {
            var commands = _consoleRunner.Commands;
            string query = input.TrimEnd();

            for (int pass = 0; pass < 2; pass++) {
                for (int i = 0; i < commands.Count && _suggestions.Count < _maxSuggestions; i++) {
                    string cmd = commands[i].cmd;

                    bool match = pass == 0
                        ? cmd.StartsWith(query, StringComparison.OrdinalIgnoreCase)
                        : cmd.Contains(query, StringComparison.OrdinalIgnoreCase);

                    if (!match || _addedInputs.Contains(cmd)) continue;

                    _addedInputs.Add(cmd);
                    _suggestions.Add(new Suggestion {
                        input = AllOverloadsHaveParameters(cmd) ? $"{cmd} " : cmd,
                        label = cmd,
                        hint = GetCommandHelpText(cmd),
                    });
                }
            }
        }

        private void Redraw() {
            if (_suggestionsRoot == null || _suggestionsText == null) return;

            int count = _suggestions.Count;
            if (count == 0) {
                _suggestionsRoot.gameObject.SetActive(false);
                return;
            }

            int lines = Mathf.Min(_maxLines, count);
            int start = _selected < lines ? 0 : _selected - lines + 1;

            string selectedColor = ColorUtility.ToHtmlStringRGBA(_selectedColor);
            string hintColor = ColorUtility.ToHtmlStringRGBA(_hintColor);

            _sb.Clear();

            for (int i = start; i < start + lines; i++) {
                var suggestion = _suggestions[i];
                bool selected = i == _selected;

                if (i > start) _sb.Append('\n');

                if (selected) _sb.Append($"<color=#{selectedColor}>> ");
                else _sb.Append("  ");

                _sb.Append("<noparse>").Append(suggestion.label).Append("</noparse>");

                if (selected) _sb.Append("</color>");

                if (!string.IsNullOrEmpty(suggestion.hint)) {
                    _sb.Append($" <color=#{hintColor}>- <noparse>").Append(suggestion.hint).Append("</noparse></color>");
                }
            }

            if (count > lines) {
                string index = _selected < 0 ? "-" : $"{_selected + 1}";
                _sb.Append($"  <color=#{hintColor}>[{index}/{count}]</color>");
            }

            string text = _sb.ToString();
            _suggestionsText.text = text;
            _suggestionsRoot.gameObject.SetActive(true);

            float height = _suggestionsText.GetPreferredValues(text).y + 2f * _verticalPadding;
            _suggestionsRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private static bool IsCommandWithArgs(string input, string cmd) {
            return input.Length > cmd.Length &&
                   input[cmd.Length] == ' ' &&
                   input.StartsWith(cmd, StringComparison.OrdinalIgnoreCase);
        }

        private static void ParseArgs(string input, string cmd, List<string> args, List<int> argStarts, out string inputBeforeLastArg) {
            args.Clear();
            argStarts.Clear();

            bool inQuotes = ConsoleArgs.Parse(input, cmd.Length, args, argStarts);

            // Input ends with a space outside of quotes: a new argument is started.
            if (args.Count == 0 || !inQuotes && input[^1] == ConsoleArgs.Separator) {
                args.Add(string.Empty);
                argStarts.Add(input.Length);
            }

            inputBeforeLastArg = input[..argStarts[^1]];
        }

        private bool AllOverloadsHaveParameters(string cmd) {
            var commands = _consoleRunner.Commands;

            for (int i = 0; i < commands.Count; i++) {
                var command = commands[i];
                if (command.cmd == cmd && command.method.GetParameters().Length == 0) return false;
            }

            return true;
        }

        private string GetCommandHelpText(string cmd) {
            var commands = _consoleRunner.Commands;
            string help = null;
            int minParameters = int.MaxValue;

            for (int i = 0; i < commands.Count; i++) {
                var command = commands[i];
                if (command.cmd != cmd) continue;

                int parameters = command.method.GetParameters().Length;
                if (parameters >= minParameters) continue;

                minParameters = parameters;
                help = GetHelpText(command.method);
            }

            return help;
        }

        private static string GetHelpText(ICustomAttributeProvider method) {
            object[] attrs = method.GetCustomAttributes(typeof(ConsoleCommandHelpAttribute), false);
            return attrs.Length == 0 ? null : ((ConsoleCommandHelpAttribute) attrs[0]).text.Trim();
        }

        private static string GetParametersText(MethodBase method) {
            var parameters = method.GetParameters();
            var sb = new StringBuilder();

            for (int i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                if (i > 0) sb.Append(' ');
                sb.Append($"<{parameter.Name}: {parameter.ParameterType.Name}>");
            }

            return sb.ToString();
        }
    }

}
