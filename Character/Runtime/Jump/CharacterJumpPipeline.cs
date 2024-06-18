using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        [EmbeddedInspector]
        [SerializeField] private ActorCondition _jumpCondition;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 LastJumpImpulse { get; private set; }
        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }

        private IActor _actor;
        private CharacterInputPipeline _input;
        private Rigidbody _rigidbody;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _input = actor.GetComponent<CharacterInputPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
        }

        private void OnEnable() {
            _input.JumpPressed -= HandleJumpPressedInput;
            _input.JumpPressed += HandleJumpPressedInput;
        }

        private void OnDisable() {
            _input.JumpPressed -= HandleJumpPressedInput;
        }

        private void HandleJumpPressedInput() {
            if (_jumpCondition != null && !_jumpCondition.IsMatch(_actor)) return;

            LastJumpImpulse = _force * _direction;
            if (LastJumpImpulse.IsNearlyZero()) return;

            _rigidbody.AddForce(LastJumpImpulse, ForceMode.VelocityChange);
            OnJump.Invoke(LastJumpImpulse);
        }
    }

}
