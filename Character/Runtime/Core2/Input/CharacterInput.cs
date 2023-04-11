using System;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    public sealed class CharacterInput : MonoBehaviour, ICharacterInput {

        [SerializeField] private InputActionVector2 _view;
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionKey _runToggle;
        [SerializeField] private InputActionKey _crouch;
        [SerializeField] private InputActionKey _crouchToggle;
        [SerializeField] private InputActionKey _jump;

        public event Action<Vector2> OnViewVectorChanged {
            add => _view.OnChanged += value;
            remove => _view.OnChanged -= value;
        }
        public event Action<Vector2> OnMotionVectorChanged {
            add => _move.OnChanged += value;
            remove => _move.OnChanged -= value;
        }

        public event Action RunToggled {
            add => _runToggle.OnPress += value;
            remove => _runToggle.OnPress -= value;
        }

        public event Action CrouchPressed {
            add => _crouch.OnPress += value;
            remove => _crouch.OnPress -= value;
        }
        public event Action CrouchReleased {
            add => _crouch.OnRelease += value;
            remove => _crouch.OnRelease -= value;
        }
        public event Action CrouchToggled {
            add => _crouchToggle.OnPress += value;
            remove => _crouchToggle.OnPress -= value;
        }
        public bool IsCrouchPressed => _crouch.IsPressed;

        public event Action JumpPressed {
            add => _jump.OnPress += value;
            remove => _jump.OnPress -= value;
        }
        public bool IsJumpPressed => _jump.IsPressed;
    }

}
