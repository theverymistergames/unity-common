using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

namespace MisterGames.Input.Icons {

    [CreateAssetMenu(fileName = nameof(InputIconsTable), menuName = "MisterGames/Input/" + nameof(InputIconsTable))]
    public sealed class InputIconsTable : ScriptableObject {

        [Header("Sprites")]
        [SerializeField] private SpriteAtlasData _keyboard;
        [SerializeField] private SpriteAtlasData _mouse;
        [SerializeField] private SpriteAtlasData _gamepadDefault;
        [SerializeField] private SerializedDictionary<GamepadType, SpriteAtlasData> _gamepadPerType;
        [SerializeField] private Sprite _fallbackSprite;
        
        [Serializable]
        private struct SpriteAtlasData {
            public SpriteAtlas spriteAtlas;
            public string pathPattern;
            public string digitsPattern;
            public PathOptions pathOptions;
            public SerializedDictionary<string, string> replaceNames;
        }

        [Flags]
        private enum PathOptions {
            None = 0,
            AlphabetToUppercase = 1,
            F1toF24ToUppercase = 2,
            OtherKeysToUppercase = 4,
        }

        private const string Keyboard = "Keyboard"; 
        private const string Mouse = "Mouse"; 
        private const string DigitPattern = "^[0-9]$"; 
        private const string LetterPattern = "^[a-z]$"; 
        private const string FunctionPattern = "^f(?:[1-9]|1[0-9]|2[0-4])$";

        private Dictionary<(string, string), string> _spritePathMap;
        
        public Sprite GetFallbackSprite() {
            return _fallbackSprite;
        }
        
        public void GetIcons(
            List<Sprite> buffer,
            InputAction inputAction,
            InputDeviceType deviceType,
            GamepadType gamepadType = GamepadType.Default) 
        {
            GetInputActionSprites(inputAction, deviceType, gamepadType, buffer);
        }

        public Sprite GetIcon(InputBinding inputBinding, GamepadType gamepadType = GamepadType.Default) {
            inputBinding.ToDisplayString(out string deviceLayoutName, out string controlPath);
            return GetInputBindingSprite(deviceLayoutName, controlPath, gamepadType);
        }
        
        public Sprite GetIcon(KeyBinding key, GamepadType gamepadType = GamepadType.Default) {
            (string deviceLayoutName, string controlPath) = key.GetBindingPath();
            return GetInputBindingSprite(deviceLayoutName, controlPath, gamepadType);
        }
        
        public Sprite GetIcon(AxisBinding axis, AxisBingingDirection dir = AxisBingingDirection.Default, GamepadType gamepadType = GamepadType.Default) {
            (string deviceLayoutName, string controlPath) = axis.GetBindingPath(dir);
            return GetInputBindingSprite(deviceLayoutName, controlPath, gamepadType);
        }

        private void GetInputActionSprites(
            InputAction inputAction,
            InputDeviceType deviceType,
            GamepadType gamepadType,
            List<Sprite> dest) 
        {
            for (int i = 0; i < inputAction.bindings.Count; i++) {
                var binding = inputAction.bindings[i];
                if (binding.isComposite) continue;
                
                binding.ToDisplayString(out string deviceLayoutName, out string controlPath);
                if (GetDeviceType(deviceLayoutName) != deviceType) continue;
                
                dest.Add(GetInputBindingSprite(deviceLayoutName, controlPath, gamepadType));
            }
        }
        
        private Sprite GetInputBindingSprite(string deviceLayoutName, string controlPath, GamepadType gamepadType) {
            var atlasData = GetAtlasData(deviceLayoutName, gamepadType);

            if (_spritePathMap == null || 
                !_spritePathMap.TryGetValue((deviceLayoutName, controlPath), out string spritePath)) 
            {
                _spritePathMap ??= new Dictionary<(string, string), string>();
                spritePath = GetSpritePath(controlPath, ref atlasData);
                _spritePathMap[(deviceLayoutName, controlPath)] = spritePath;
            }
            
            return atlasData.spriteAtlas.GetSprite(spritePath);
        }

        private static InputDeviceType GetDeviceType(string deviceLayoutName) {
            return deviceLayoutName switch {
                Keyboard or Mouse => InputDeviceType.KeyboardMouse,
                _ => InputDeviceType.Gamepad,
            };
        }
        
        private SpriteAtlasData GetAtlasData(string deviceLayoutName, GamepadType gamepadType) {
            return deviceLayoutName switch {
                Keyboard => _keyboard,
                Mouse => _mouse,
                _ => _gamepadPerType.GetValueOrDefault(gamepadType, _gamepadDefault),
            };
        }
        
        private static string GetSpritePath(string controlPath, ref SpriteAtlasData atlasData) {
            if (string.IsNullOrWhiteSpace(controlPath)) return null;
            
            if (atlasData.replaceNames.TryGetValue(controlPath, out string path)) {
                controlPath = path;
            }
            else if (Regex.IsMatch(controlPath, DigitPattern) && !string.IsNullOrWhiteSpace(atlasData.digitsPattern)) {
                controlPath = string.Format(atlasData.digitsPattern, controlPath);
            }
            else if (Regex.IsMatch(controlPath, LetterPattern) && (atlasData.pathOptions & PathOptions.AlphabetToUppercase) != 0) {
                controlPath = controlPath.ToUpper();
            }
            else if (Regex.IsMatch(controlPath, FunctionPattern) && (atlasData.pathOptions & PathOptions.F1toF24ToUppercase) != 0) {
                controlPath = controlPath.ToUpper();
            }
            else if ((atlasData.pathOptions & PathOptions.OtherKeysToUppercase) != 0) {
                controlPath = controlPath.Length == 1 ? controlPath.ToUpper() : $"{char.ToUpper(controlPath[0])}{controlPath[1..]}";
            }

            if (!string.IsNullOrWhiteSpace(atlasData.pathPattern)) {
                controlPath = string.Format(atlasData.pathPattern, controlPath);
            } 

            return controlPath;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            _spritePathMap = null;
        }
#endif
    }
    
}