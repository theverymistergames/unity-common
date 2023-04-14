using System;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [Serializable]
    public struct InputActionKeyEvent {

        [SerializeField] private InputActionKeyEvents _mode;
        [SerializeField] private InputActionKey _key;

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
    }

}
