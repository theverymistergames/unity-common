using System.Collections.Generic;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Dbg.Console.Plugins {

    public class ConsoleCommandsHistory : MonoBehaviour {

        [SerializeField] private ConsoleRunner _consoleRunner;

        [Header("Inputs")]
        [SerializeField] private InputActionKey _historyUpInput;
        [SerializeField] private InputActionKey _historyDownInput;

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
                _historyCurrentInput = ConsoleRunner.Instance.CurrentInput;
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
            string text = _commandHistory.IsEmpty() || _historyPointer == _commandHistory.Count
                ? _historyCurrentInput
                : _commandHistory[_historyPointer];

            ConsoleRunner.Instance.TypeIn(text);
        }

        private void SubscribeConsoleInput() {
            var runner = ConsoleRunner.Instance;
            if (runner == null) return;

            runner.OnBeforeRunCommand -= OnBeforeRunCommand;
            runner.OnBeforeRunCommand += OnBeforeRunCommand;
        }

        private void UnsubscribeConsoleInput() {
            var runner = ConsoleRunner.Instance;
            if (runner == null) return;

            runner.OnBeforeRunCommand -= OnBeforeRunCommand;
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
            _historyUpInput.OnPress -= OnHistoryUp;
            _historyUpInput.OnPress += OnHistoryUp;

            _historyDownInput.OnPress -= OnHistoryDown;
            _historyDownInput.OnPress += OnHistoryDown;
        }

        private void UnsubscribeHistoryInput() {
            _historyUpInput.OnPress -= OnHistoryUp;
            _historyDownInput.OnPress -= OnHistoryDown;
        }
    }

}
