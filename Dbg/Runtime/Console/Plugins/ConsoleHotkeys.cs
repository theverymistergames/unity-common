using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;
using MisterGames.Input.Global;
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
            
            public void Process(ConsoleRunner consoleRunner) {
                if ((key.WasPerformedThisFrame() || modifiers.WasPerformedThisFrame()) && 
                    key.IsActive() && modifiers.IsActive()
                ) {
                    consoleRunner.RunCommand(command);
                }
            }
        }
        
        private ConsoleRunner _consoleRunner;

        private void Awake() {
            _consoleRunner = GetComponent<ConsoleRunner>();
        }

        private void Start() {
            FetchConsoleCommands();
        }

        private void Update() {
            for (int i = 0; i < _hotkeys.Count; i++) {
                _hotkeys[i].Process(_consoleRunner);
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
