using System;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterInput : MonoBehaviour, ICharacterInput {

        [SerializeField] private InputActionVector2 _view;
        [SerializeField] private InputActionVector2 _move;
        [SerializeField] private InputActionKey _run;
        [SerializeField] private InputActionKey _runToggle;
        [SerializeField] private InputActionKey _crouch;
        [SerializeField] private InputActionKey _crouchToggle;
        [SerializeField] private InputActionKey _jump;

        public event Action<Vector2> View {
            add => _view.OnChanged += value;
            remove => _view.OnChanged -= value;
        }
        
        public event Action<Vector2> Move {
            add => _move.OnChanged += value;
            remove => _move.OnChanged -= value;
        }

        public event Action StartRun {
            add => _run.OnPress += value;
            remove => _run.OnPress -= value;
        }
        
        public event Action StopRun {
            add => _run.OnRelease += value;
            remove => _run.OnRelease -= value;
        }
         
        public event Action ToggleRun {
            add => _runToggle.OnPress += value;
            remove => _runToggle.OnPress -= value;
        }
        
        public event Action StartCrouch {
            add => _crouch.OnPress += value;
            remove => _crouch.OnPress -= value;
        }
        
        public event Action StopCrouch {
            add => _crouch.OnRelease += value;
            remove => _crouch.OnRelease -= value;
        }
        
        public event Action ToggleCrouch {
            add => _crouchToggle.OnPress += value;
            remove => _crouchToggle.OnPress -= value;
        }
        
        public event Action Jump {
            add => _jump.OnPress += value;
            remove => _jump.OnPress -= value;
        }
    }

}
