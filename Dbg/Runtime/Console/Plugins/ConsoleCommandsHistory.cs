using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
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
        [SerializeField] private string _playerPrefsId = "ConsoleCommandsHistory";
        [SerializeField] private int _maxCommandHistorySize = 20;

        [Serializable]
        private sealed class JsonDto {
            public List<string> history;
        }
        
        private readonly List<string> _commandHistory = new();
        private JsonDto _dto = new();
        
        private string _historyCurrentInput;
        private int _historyPointer;

        private void OnEnable() {
            SubscribeShowHideConsole();
            RestoreHistoryFromPlayerPrefs();
        }

        private void OnDisable() {
            UnsubscribeShowHideConsole();
            UnsubscribeHistoryInput();
            UnsubscribeConsoleInput();
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
            
            SaveHistoryToPlayerPrefs();
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

        private void SaveHistoryToPlayerPrefs() {
            _dto ??= new JsonDto();
            _dto.history = _commandHistory;
            
            PlayerPrefs.SetString(_playerPrefsId, JsonUtility.ToJson(_dto));
        }

        private void RestoreHistoryFromPlayerPrefs() {
            string json = PlayerPrefs.GetString(_playerPrefsId);
            _dto = JsonUtility.FromJson<JsonDto>(json) ?? new JsonDto();

            if (_dto.history == null || _dto.history.Count == 0) {
                ClearHistory();
                return;
            }

            _commandHistory.Clear();
            _commandHistory.AddRange(_dto.history);
            _historyPointer = _commandHistory.Count;
        }
        
        private void ValidateHistorySize() {
            int length = _commandHistory.Count;
            if (length <= _maxCommandHistorySize) return;

            int lengthShouldBe = Mathf.FloorToInt(_maxCommandHistorySize * 0.7f);
            int toRemoveCount = length - lengthShouldBe;
            _commandHistory.RemoveRange(0, toRemoveCount);
        }

        [Button]
        private void ClearHistory() {
            _commandHistory.Clear();
            _historyPointer = 0;
            
            PlayerPrefs.DeleteKey(_playerPrefsId);
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
