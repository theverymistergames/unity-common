using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using UnityEngine;

namespace MisterGames.Dbg.Console.Plugins {

    [RequireComponent(typeof(ConsoleRunner))]
    public sealed class ConsoleHotkeys : MonoBehaviour {
        
        [SerializeField] private List<ConsoleHotkey> _hotkeys;
        
        [Serializable]
        private struct ConsoleHotkey {
            
            public KeyBinding key;
            public ShortcutModifiers modifiers;
            public string command;
        }
        
        private ConsoleRunner _consoleRunner;
        private bool[] _inputActiveMap;

        private void Awake() {
            _consoleRunner = GetComponent<ConsoleRunner>();
        }

        private void Start() {
            FetchConsoleCommands();
        }

        private void Update() {
            for (int i = 0; i < _hotkeys.Count; i++) {
                var hotkey = _hotkeys[i];

                bool wasActive = _inputActiveMap[i];
                bool active = hotkey.key.IsPressed() && hotkey.modifiers.ArePressed();
                
                if (active && !wasActive) _consoleRunner.RunCommand(hotkey.command);

                _inputActiveMap[i] = active;
            }
        }

        private void FetchConsoleCommands() {
            var modules = _consoleRunner.ConsoleModules;
            
            for (int i = 0; i < modules.Count; i++) {
                var methods = modules[i].GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                
                for (int j = 0; j < methods.Length; j++) {
                    if (!TryGetAttrFromMethod(methods[j], out var attr)) continue;

                    _hotkeys.Add(new ConsoleHotkey { command = attr.cmd, key = attr.key, modifiers = attr.mod });
                }
            }

            _inputActiveMap = new bool[_hotkeys.Count];
        }

        private static bool TryGetAttrFromMethod(ICustomAttributeProvider methodInfo, out ConsoleHotkeyAttribute attr) {
            attr = null;

            object[] cmdAttrs = methodInfo.GetCustomAttributes(typeof(ConsoleHotkeyAttribute), false);
            if (cmdAttrs.Length == 0) return false;

            attr = (ConsoleHotkeyAttribute) cmdAttrs[0];
            return true;
        }
    }

}
