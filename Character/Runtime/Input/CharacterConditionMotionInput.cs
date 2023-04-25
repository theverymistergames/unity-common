using System;
using System.Collections.Generic;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionMotionInput : ICondition, IDynamicDataHost {

        public Optional<bool> isMotionInputActive;
        public Optional<bool> isMovingForward;

        public bool IsMatched => CheckCondition();

        private IConditionCallback _callback;
        private ICharacterInputPipeline _input;
        private ICharacterMotionPipeline _motion;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            var characterAccess = provider.GetData<CharacterAccess>();

            _input = characterAccess.GetPipeline<ICharacterInputPipeline>();
            _motion = characterAccess.GetPipeline<ICharacterMotionPipeline>();
        }

        public void Arm(IConditionCallback callback) {
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
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        public void OnFired() { }

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
