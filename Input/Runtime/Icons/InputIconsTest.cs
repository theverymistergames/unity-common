using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Inputs;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Input.Icons {
    
    internal sealed class InputIconsTest : MonoBehaviour {
        
        public InputIconsTable inputIconsTable;
        public Image[] images;
        public Mode mode;
        [VisibleIf(nameof(mode), 0)] public KeyBinding keyBinding;
        [VisibleIf(nameof(mode), 1)] public AxisBinding axisBinding;
        [VisibleIf(nameof(mode), 1)] public AxisBingingDirection axisIconType;
        [VisibleIf(nameof(mode), 2)] public InputDeviceType deviceType;
        [VisibleIf(nameof(mode), 2)] public InputActionRef inputActionRef;

        public enum Mode {
            KeyBinding,
            AxisBinding,
            InputAction,
        }
        
        [Button]
        private void UpdateIcon() {
            if (images == null || inputIconsTable == null || images.Length < 1) return;

            switch (mode) {
                case Mode.KeyBinding:
                    images[0].sprite = inputIconsTable.GetIcon(keyBinding);
                    break;
                
                case Mode.AxisBinding:
                    images[0].sprite = inputIconsTable.GetIcon(axisBinding, axisIconType);
                    break;
                
                case Mode.InputAction:
                    InputServices.EnableInputInEditModeForSource(this, true);
                    
                    if (inputActionRef.Get() is { } inputAction) {
                        var buffer = new List<Sprite>();
                        inputIconsTable.GetIcons(buffer, inputAction, deviceType);
                        int l = Mathf.Min(buffer.Count, images.Length);
                        for (int i = 0; i < l; i++) {
                            images[i].sprite = buffer[i];   
                        }
                    }
                    
                    InputServices.EnableInputInEditModeForSource(this, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnValidate() {
            UpdateIcon();
        }
    }
    
}