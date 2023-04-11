using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionMotionInput : ICondition {

        public Optional<bool> _isMotionInputActive;
        public Optional<bool> _isMovingForward;

        public bool IsMatched => CheckCondition(CharacterAccessProvider.CharacterAccess.MotionPipeline.MotionInput);

        private IConditionCallback _callback;

        public void Arm(IConditionCallback callback) {
            _callback = callback;
            CharacterAccessProvider.CharacterAccess.Input.OnMotionVectorChanged += OnMotionVectorChanged;
        }

        public void Disarm() {
            CharacterAccessProvider.CharacterAccess.Input.OnMotionVectorChanged -= OnMotionVectorChanged;
            _callback = null;
        }

        private void OnMotionVectorChanged(Vector2 motion) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition(Vector2 motionInput) {
            return _isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   _isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }
    }

}
