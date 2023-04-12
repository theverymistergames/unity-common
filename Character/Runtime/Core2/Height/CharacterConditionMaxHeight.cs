using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterConditionMaxHeight : ICondition, IDynamicDataHost {

        [Min(0f)] public float maxHeight;
        
        public bool IsMatched => _characterAccess.HeightPipeline.Height <= maxHeight;

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
            if (IsMatched) _callback?.OnConditionMatch();

            _characterAccess.HeightPipeline.OnHeightChanged += OnHeightChanged;
        }

        public void Disarm() {
            _characterAccess.HeightPipeline.OnHeightChanged -= OnHeightChanged;
            _callback = null;
        }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnConditionMatch();
        }
    }

}
