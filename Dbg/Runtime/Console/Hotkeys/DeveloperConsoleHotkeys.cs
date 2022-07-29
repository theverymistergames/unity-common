using System;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Dbg.Console.Hotkeys {

    public class DeveloperConsoleHotkeys : MonoBehaviour {

        [SerializeField] private ConsoleHotkey[] _hotkeys;

        private HotkeyHandler[] _hotkeyHandlers;

        [Serializable]
        private struct ConsoleHotkey {
            public InputActionKey input;
            public string command;
        }

        private void Awake() {
            _hotkeyHandlers = new HotkeyHandler[_hotkeys.Length];
            for (int i = 0; i < _hotkeys.Length; i++) {
                _hotkeyHandlers[i] = new HotkeyHandler(_hotkeys[i]);
            }
        }

        private void OnEnable() {
            for (int i = 0; i < _hotkeyHandlers.Length; i++) {
                _hotkeyHandlers[i].SubscribeOnInput();
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _hotkeyHandlers.Length; i++) {
                _hotkeyHandlers[i].UnsubscribeFromInput();
            }
        }

        private class HotkeyHandler {

            private readonly ConsoleHotkey _hotkey;

            public HotkeyHandler(ConsoleHotkey hotkey) {
                _hotkey = hotkey;
            }

            public void SubscribeOnInput() {
                _hotkey.input.OnPress -= OnUseHotkey;
                _hotkey.input.OnPress += OnUseHotkey;
            }

            public void UnsubscribeFromInput() {
                _hotkey.input.OnPress -= OnUseHotkey;
            }

            private void OnUseHotkey() {
                DeveloperConsoleRunner.Instance.ProcessCommand(_hotkey.command);
            }
        }

    }

}
