using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Motion;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Jump {

    public class CharacterJumpPipeline : MonoBehaviour, ICharacterJumpPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private float _force = 1f;

        public event Action<Vector3> OnJump = delegate {  };

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public float Force { get => _force; set => _force = value; }
        public float ForceMultiplier { get; private set; } = 1f;

        private readonly Dictionary<object, float> _forceMultipliers = new Dictionary<object, float>();

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.JumpPressed -= HandleJumpPressedInput;
                _characterAccess.Input.JumpPressed += HandleJumpPressedInput;
                return;
            }

            _characterAccess.Input.JumpPressed -= HandleJumpPressedInput;
        }

        public void SetForceMultiplier(object source, float multiplier) {
            _forceMultipliers[source] = Mathf.Max(0f, multiplier);
            InvalidateForceMultiplier();
        }

        public void ResetForceMultiplier(object source) {
            if (!_forceMultipliers.ContainsKey(source)) return;

            _forceMultipliers.Remove(source);
            InvalidateForceMultiplier();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleJumpPressedInput() {
            var impulse = ForceMultiplier * _force * _direction;
            if (impulse.IsNearlyZero()) return;

            _characterAccess.MotionPipeline.GetProcessor<CharacterProcessorMass>()?.ApplyImpulse(impulse);
            OnJump.Invoke(impulse);
        }

        private void InvalidateForceMultiplier() {
            float forceMultiplier = 1f;
            foreach (float m in _forceMultipliers.Values) {
                forceMultiplier *= m;
            }

            ForceMultiplier = forceMultiplier;
        }
    }

}
