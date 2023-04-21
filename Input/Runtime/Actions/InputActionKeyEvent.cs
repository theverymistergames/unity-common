using System;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [Serializable]
    public struct InputActionKeyEvent : IEquatable<InputActionKeyEvent> {

        [SerializeField] private InputActionKeyEvents _mode;
        [SerializeField] private InputActionKey _key;

        public bool IsPressed => _key.IsPressed;

        public bool WasFired => _mode switch {
            InputActionKeyEvents.OnPressed => _key.WasPressed,
            InputActionKeyEvents.OnReleased => _key.WasReleased,
            InputActionKeyEvents.OnUsed => _key.WasUsed,
            _ => throw new NotImplementedException($"{nameof(InputActionKeyEvent)}: mode {_mode} is not supported")
        };

        public event Action OnFired {
            add {
                switch (_mode) {
                    case InputActionKeyEvents.OnPressed:
                        _key.OnPress -= value;
                        _key.OnPress += value;
                        break;

                    case InputActionKeyEvents.OnReleased:
                        _key.OnRelease -= value;
                        _key.OnRelease += value;
                        break;

                    case InputActionKeyEvents.OnUsed:
                        _key.OnUse -= value;
                        _key.OnUse += value;
                        break;

                    default:
                        throw new NotImplementedException($"{nameof(InputActionKeyEvent)}: mode {_mode} is not supported");
                }
            }
            remove {
                switch (_mode) {
                    case InputActionKeyEvents.OnPressed:
                        _key.OnPress -= value;
                        break;

                    case InputActionKeyEvents.OnReleased:
                        _key.OnRelease -= value;
                        break;

                    case InputActionKeyEvents.OnUsed:
                        _key.OnUse -= value;
                        break;

                    default:
                        throw new NotImplementedException($"{nameof(InputActionKeyEvent)}: mode {_mode} is not supported");
                }
            }
        }

        public bool Equals(InputActionKeyEvent other) {
            return _mode == other._mode && _key == other._key;
        }

        public override bool Equals(object obj) {
            return obj is InputActionKeyEvent other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine((int) _mode, _key);
        }

        public static bool operator ==(InputActionKeyEvent left, InputActionKeyEvent right) {
            return left.Equals(right);
        }

        public static bool operator !=(InputActionKeyEvent left, InputActionKeyEvent right) {
            return !left.Equals(right);
        }
    }

}
