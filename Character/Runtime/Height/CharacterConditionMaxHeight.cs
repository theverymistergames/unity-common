using System;
using System.Collections.Generic;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterConditionMaxHeight : ICondition, IDynamicDataHost {

        [Min(0f)] public float maxHeight;

        public bool IsMatched => CheckCondition();

        private ICharacterHeightPipeline _heightPipeline;
        private IConditionCallback _callback;
        
        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _heightPipeline = provider.GetData<CharacterAccess>().GetPipeline<ICharacterHeightPipeline>();
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

        public void OnFired() { }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private bool CheckCondition() {
            return _heightPipeline.Height <= maxHeight;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionMaxHeight)}({maxHeight})";
        }
    }

}
