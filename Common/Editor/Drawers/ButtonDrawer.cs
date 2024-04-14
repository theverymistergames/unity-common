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
            public readonly string DisplayName;
            public readonly MethodInfo Method;
        
            public ButtonData(MethodInfo method, string name)
            {
                DisplayName = string.IsNullOrEmpty(name) ? ObjectNames.NicifyVariableName(method.Name) : name;
                Method = method;
            }
        }
        
        public ButtonsDrawer(object target)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = target.GetType().GetMethods(flags);
            var buttons = new List<ButtonData>();
            
            foreach (var method in methods) {
                if (method.GetCustomAttribute<ButtonAttribute>() is { } attr) {
                    buttons.Add(new ButtonData(method, attr.name));
                }
            }

            _buttons = buttons.ToList();
        }

        public void DrawButtons(IReadOnlyList<object> targets) {
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i];
                if (!GUILayout.Button(button.DisplayName)) continue;

                for (int j = 0; j < targets.Count; j++) {
                    button.Method.Invoke(targets[j], null);
                }
            }
        }
    }
}