﻿using System;
using System.Collections.Generic;
using MisterGames.Character.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Input {

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
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        public void OnFired() { }

        private bool CheckCondition() {
            var motionInput = _characterAccess.MotionPipeline.MotionInput;

            return _isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   _isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionMotionInput)}(active {_isMotionInputActive}, moving forward {_isMovingForward})";
        }
    }

}
