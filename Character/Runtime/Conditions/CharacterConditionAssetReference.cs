using System;
using MisterGames.Character.Core;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionAssetReference : ICharacterCondition {

        public CharacterConditionAsset condition;
        public bool defaultCondition;

        public bool IsMatch(ICharacterAccess context) {
            return condition == null ? defaultCondition : condition.IsMatch(context);
        }
    }

}
