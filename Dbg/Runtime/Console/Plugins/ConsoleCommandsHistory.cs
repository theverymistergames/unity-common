using System.Collections.Generic;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using UnityEngine;

namespace MisterGames.Dbg.Console.Plugins {

    public class ConsoleCommandsHistory : MonoBehaviour {

        [SerializeField] private ConsoleRunner _consoleRunner;

        [Header("Inputs")]
        [SerializeField] private KeyBinding _historyUpInput = KeyBinding.ArrowUp;
        [SerializeField] private KeyBinding _historyDownInput = KeyBinding.ArrowDown;

        [Header("Commands")]
        [SerializeField] private int _maxCommandHistorySize = 20;

        private readonly List<string> _commandHistory = new List<string>();
        private string _historyCurrentInput;
        private int _historyPointer;

        private void OnEnable() {
            SubscribeShowHideConsole();

            _historyPointer = _commandHistory.Count;
        }

        private void OnDisable() {
            UnsubscribeShowHideConsole();
            UnsubscribeHistoryInput();
            UnsubscribeConsoleInput();

            ClearHistory();
        }

        private void OnShowConsole() {
            SubscribeHistoryInput();
            SubscribeConsoleInput();
        }

        private void OnHideConsole() {
            UnsubscribeHistoryInput();
            UnsubscribeConsoleInput();
        }

        private void OnBeforeRunCommand(string input) {
            _commandHistory.Add(input);
            ValidateHistorySize();
            _historyPointer = _commandHistory.Count;
        }

        private void OnHistoryUp() {
            if (_historyPointer == _commandHistory.Count) {
                _historyCurrentInput = _consoleRunner.CurrentInput;
            }

            _historyPointer = Mathf.Max(_historyPointer - 1, 0);
            SetTextInputFieldFromHistory();
        }

        private void OnHistoryDown() {
            _historyPointer = Mathf.Min(_historyPointer + 1, _commandHistory.Count);
            SetTextInputFieldFromHistory();
        }

        private void ValidateHistorySize() {
            int length = _commandHistory.Count;
            if (length <= _maxCommandHistorySize) return;

            int lengthShouldBe = Mathf.FloorToInt(_maxCommandHistorySize * 0.7f);
            int toRemoveCount = length - lengthShouldBe;
            _commandHistory.RemoveRange(0, toRemoveCount);
        }

        private void ClearHistory() {
            _commandHistory.Clear();
            _historyPointer = 0;
        }

        private void SetTextInputFieldFromHistory() {
            string text = _commandHistory.Count == 0 || _historyPointer == _commandHistory.Count
                ? _historyCurrentInput
                : _commandHistory[_historyPointer];

            _consoleRunner.TypeIn(text);
        }

        private void SubscribeConsoleInput() {
            if (_consoleRunner == null) return;

            _consoleRunner.OnBeforeRunCommand -= OnBeforeRunCommand;
            _consoleRunner.OnBeforeRunCommand += OnBeforeRunCommand;
        }

        private void UnsubscribeConsoleInput() {
            if (_consoleRunner == null) return;

            _consoleRunner.OnBeforeRunCommand -= OnBeforeRunCommand;
        }

        private void SubscribeShowHideConsole() {
            _consoleRunner.OnShowConsole -= OnShowConsole;
            _consoleRunner.OnShowConsole += OnShowConsole;

            _consoleRunner.OnHideConsole -= OnHideConsole;
            _consoleRunner.OnHideConsole += OnHideConsole;
        }

        private void UnsubscribeShowHideConsole() {
            _consoleRunner.OnShowConsole -= OnShowConsole;
            _consoleRunner.OnHideConsole -= OnHideConsole;
        }

        private void SubscribeHistoryInput() {
            UnsubscribeHistoryInput();
            
            _historyUpInput.AddPressCallback(OnHistoryUp);
            _historyDownInput.AddPressCallback(OnHistoryDown);
        }

        private void UnsubscribeHistoryInput() {
            _historyUpInput.RemovePressCallback(OnHistoryUp);
            _historyDownInput.RemovePressCallback(OnHistoryDown);
        }
    }

}
