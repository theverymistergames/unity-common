using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterConditionMaxHeight : ICondition {

        [Min(0f)] public float maxHeight;
        
        public bool IsMatched => CharacterAccessProvider.CharacterAccess.HeightPipeline.Height <= maxHeight;

        private IConditionCallback _callback;
        
        public void Arm(IConditionCallback callback) {
            _callback = callback;
            CharacterAccessProvider.CharacterAccess.HeightPipeline.OnHeightChanged += OnHeightChanged;
        }

        public void Disarm() {
            CharacterAccessProvider.CharacterAccess.HeightPipeline.OnHeightChanged -= OnHeightChanged;
            _callback = null;
        }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnConditionMatch();
        }
    }

}
