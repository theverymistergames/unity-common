using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    internal sealed class ButtonsDrawer
    {
        private readonly List<ButtonData> _buttons;

        private readonly struct ButtonData
        {
            public readonly string displayName;
            public readonly ButtonAttribute.Mode mode;
            public readonly MethodInfo method;
        
            public ButtonData(MethodInfo method, ButtonAttribute attribute)
            {
                displayName = string.IsNullOrEmpty(attribute.name) 
                    ? ObjectNames.NicifyVariableName(method.Name) 
                    : attribute.name;

                mode = attribute.mode; 
                this.method = method;
            }
        }
        
        public ButtonsDrawer(object target)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = target.GetType().GetMethods(flags);
            var buttons = new List<ButtonData>();
            
            foreach (var method in methods) {
                if (method.GetCustomAttribute<ButtonAttribute>() is { } attr) {
                    buttons.Add(new ButtonData(method, attr));
                }
            }

            _buttons = buttons.ToList();
        }

        public void DrawButtons(IReadOnlyList<object> targets) {
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i];
                if (!CanDrawButton(button.mode) || !GUILayout.Button(button.displayName)) continue;

                for (int j = 0; j < targets.Count; j++) {
                    button.method.Invoke(targets[j], null);
                }
            }
        }

        private static bool CanDrawButton(ButtonAttribute.Mode mode) {
            return mode switch {
                ButtonAttribute.Mode.Always => true,
                ButtonAttribute.Mode.Runtime => Application.isPlaying,
                ButtonAttribute.Mode.Editor => !Application.isPlaying,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
    }
    
}