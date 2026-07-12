using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    public sealed class ButtonsDrawer
    {
        private readonly List<ButtonData> _buttons;

        private readonly struct ButtonData
        {
            public readonly string displayName;
            public readonly ButtonAttribute.Mode mode;
            public readonly MethodInfo method;
            public readonly MethodInfo validationMethod;
        
            public ButtonData(MethodInfo method, ButtonAttribute attribute, MethodInfo validationMethod)
            {
                displayName = string.IsNullOrEmpty(attribute.name) 
                    ? ObjectNames.NicifyVariableName(method.Name) 
                    : attribute.name;

                mode = attribute.mode; 
                this.method = method;
                this.validationMethod = validationMethod;
            }
        }
        
        public ButtonsDrawer(object target)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var targetType = target.GetType();
            var methods = targetType.GetMethods(flags);
            var buttons = new List<ButtonData>();
            
            foreach (var method in methods) {
                if (method.GetCustomAttribute<ButtonAttribute>() is { } attr) {
                    var validationMethod = attr.showIf == null ? null : targetType.GetMethod(attr.showIf, flags);
                    buttons.Add(new ButtonData(method, attr, validationMethod));
                }
            }

            _buttons = buttons.ToList();
        }

        public void DrawButtons(IReadOnlyList<object> targets) {
            for (int i = 0; i < _buttons.Count; i++) {
                var button = _buttons[i];
                
                for (int j = 0; j < targets.Count; j++) {
                    if (!CanDrawButton(button.mode, button.validationMethod, targets[j]) || !GUILayout.Button(button.displayName)) continue;

                    button.method.Invoke(targets[j], null);
                }
            }
        }

        private static bool CanDrawButton(ButtonAttribute.Mode mode, MethodInfo validationMethod, object target) {
            return mode switch {
                ButtonAttribute.Mode.Always => true,
                ButtonAttribute.Mode.Runtime => Application.isPlaying,
                ButtonAttribute.Mode.Editor => !Application.isPlaying,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            } && 
                   (validationMethod == null || validationMethod.ReturnType != typeof(bool) || (bool) validationMethod.Invoke(target, null));
        }
    }
    
}