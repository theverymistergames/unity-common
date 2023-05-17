using System;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionMotionInput : ITransition, IDependency {

        public Optional<bool> isMotionInputActive;
        public Optional<bool> isMovingForward;

        public bool IsMatched => CheckCondition();

        private ITransitionCallback _callback;
        private ICharacterInputPipeline _input;
        private ICharacterMotionPipeline _motion;

        public void OnAddDependencies(IDependencyResolver resolver) {
            resolver.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            var characterAccess = resolver.ResolveDependency<CharacterAccess>();

            _input = characterAccess.GetPipeline<ICharacterInputPipeline>();
            _motion = characterAccess.GetPipeline<ICharacterMotionPipeline>();
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            if (isMotionInputActive.HasValue || isMovingForward.HasValue) {
                _input.OnMotionVectorChanged -= OnMotionVectorChanged;
                _input.OnMotionVectorChanged += OnMotionVectorChanged;
            }
        }

        public void Disarm() {
            if (isMotionInputActive.HasValue || isMovingForward.HasValue) {
                _input.OnMotionVectorChanged -= OnMotionVectorChanged;
            }

            _callback = null;
        }

        private void OnMotionVectorChanged(Vector2 motion) {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            var motionInput = _motion.MotionInput;

            return isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionMotionInput)}(active {isMotionInputActive}, moving forward {isMovingForward})";
        }
    }

}
