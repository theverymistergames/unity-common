using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionMotionInput : ICondition, IDynamicDataHost {

        public Optional<bool> _isMotionInputActive;
        public Optional<bool> _isMovingForward;

        public bool IsMatched => CheckCondition();

        private IConditionCallback _callback;
        private ICharacterAccess _characterAccess;
        
        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (_isMotionInputActive.HasValue || _isMovingForward.HasValue) {
                var input = _characterAccess.Input;

                input.OnMotionVectorChanged -= OnMotionVectorChanged;
                input.OnMotionVectorChanged += OnMotionVectorChanged;
            }
        }

        public void Disarm() {
            if (_isMotionInputActive.HasValue || _isMovingForward.HasValue) {
                _characterAccess.Input.OnMotionVectorChanged -= OnMotionVectorChanged;
            }

            _callback = null;
        }

        private void OnMotionVectorChanged(Vector2 motion) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            var motionInput = _characterAccess.MotionPipeline.MotionInput;

            return _isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   _isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }
    }

}
