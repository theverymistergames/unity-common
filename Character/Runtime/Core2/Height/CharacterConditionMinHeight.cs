using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterConditionMinHeight : ICondition, IDynamicDataHost {

        [Min(0f)] public float minHeight;

        public bool IsMatched => CheckCondition();

        private ICharacterAccess _characterAccess;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;
            if (_characterAccess != null) _characterAccess.HeightPipeline.OnHeightChanged += OnHeightChanged;

            if (IsMatched) _callback?.OnConditionMatch();
        }

        public void Disarm() {
            if (_characterAccess != null) _characterAccess.HeightPipeline.OnHeightChanged -= OnHeightChanged;
            _callback = null;
        }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            if (_characterAccess == null) return minHeight <= 0f;

            return _characterAccess.HeightPipeline.Height >= minHeight;
        }
    }

}
