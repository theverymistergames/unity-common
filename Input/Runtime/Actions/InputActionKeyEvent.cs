using System;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [Serializable]
    public struct InputActionKeyEvent : IEquatable<InputActionKeyEvent> {

        [SerializeField] private InputActionKeyEvents _mode;
        [SerializeField] private InputActionRef _key;

        public enum InputActionKeyEvents {
            OnPressed,
            OnReleased,
            OnPerformed,
        }
        
        public bool IsPressed => _key.Get().IsPressed();

        public bool WasFired => _mode switch {
            InputActionKeyEvents.OnPressed => _key.Get().WasPressedThisFrame(),
            InputActionKeyEvents.OnReleased => _key.Get().WasReleasedThisFrame(),
            InputActionKeyEvents.OnPerformed => _key.Get().WasPerformedThisFrame(),
            _ => throw new NotImplementedException($"{nameof(InputActionKeyEvent)}: mode {_mode} is not supported")
        };

        public bool Equals(InputActionKeyEvent other) => _mode == other._mode && _key == other._key;
        public override bool Equals(object obj) => obj is InputActionKeyEvent other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int) _mode, _key);

        public static bool operator ==(InputActionKeyEvent left, InputActionKeyEvent right) => left.Equals(right);
        public static bool operator !=(InputActionKeyEvent left, InputActionKeyEvent right) => !left.Equals(right);
    }

}
