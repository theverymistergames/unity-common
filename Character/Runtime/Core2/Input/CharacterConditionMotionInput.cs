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

        public bool IsMatched => CheckCondition(_characterAccess.MotionPipeline.MotionInput);

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
            _characterAccess.Input.OnMotionVectorChanged += OnMotionVectorChanged;
        }

        public void Disarm() {
            _characterAccess.Input.OnMotionVectorChanged -= OnMotionVectorChanged;
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
