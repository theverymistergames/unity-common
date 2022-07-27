using System;
using MisterGames.Input.Core;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    public interface IInputBindingKey : IInputBinding {
        KeyBinding[] GetBindings();
        bool IsActive();
    }

    [Serializable]
    public struct InputBindingKey : IInputBindingKey {

        [SerializeField] private KeyBinding _key;

        private KeyBinding[] _keys;

        public void Init() { }

        public void Terminate() { }

        public KeyBinding[] GetBindings() {
            _keys ??= new[] { _key };
            return _keys;
        }
        
        public bool IsActive() {
            return _key.IsActive();
        }
    }

    [Serializable]
    public struct InputBindingKeyCombo : IInputBindingKey {

        [SerializeField] private KeyBinding[] _keys;

        public void Init() { }

        public void Terminate() { }

        public KeyBinding[] GetBindings() {
            return _keys;
        }

        public bool IsActive() {
            for (int i = 0; i < _keys.Length; i++) {
                if (!_keys[i].IsActive()) return false;
            }

            return true;
        }
    }

}
