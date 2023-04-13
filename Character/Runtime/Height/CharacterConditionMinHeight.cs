using System;
using System.Collections.Generic;
using MisterGames.Character.Access;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterConditionMinHeight : ICondition, IDynamicDataHost {

        [Min(0f)] public float minHeight;

        public bool IsMatched => CheckCondition();

        private ICharacterHeightPipeline _heightPipeline;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _heightPipeline = provider.GetData<CharacterAccess>().HeightPipeline;
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            _heightPipeline.OnHeightChanged -= OnHeightChanged;
            _heightPipeline.OnHeightChanged += OnHeightChanged;
        }

        public void Disarm() {
            _heightPipeline.OnHeightChanged -= OnHeightChanged;

            _callback = null;
        }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            return _heightPipeline.Height >= minHeight;
        }
    }

}
