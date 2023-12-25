using System;
using MisterGames.Character.Conditions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpPipeline : CharacterPipelineBase, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        [EmbeddedInspector]
        [SerializeField] private CharacterConditionAsset _jumpCondition;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 LastJumpImpulse { get; private set; }
        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterInputPipeline _input;
        private CharacterProcessorMass _mass;

        private void Awake() {
            _input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            _mass = _characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();
        }

        private void OnEnable() {
            _input.JumpPressed -= HandleJumpPressedInput;
            _input.JumpPressed += HandleJumpPressedInput;
        }

        private void OnDisable() {
            _input.JumpPressed -= HandleJumpPressedInput;
        }

        private void HandleJumpPressedInput() {
            if (_jumpCondition != null && !_jumpCondition.IsMatch(_characterAccess)) return;

            LastJumpImpulse = _force * _direction;
            if (LastJumpImpulse.IsNearlyZero()) return;

            _mass.ApplyImpulse(LastJumpImpulse);
            OnJump.Invoke(LastJumpImpulse);
        }
    }

}
