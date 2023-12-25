using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Conditions {

    [CreateAssetMenu(fileName = nameof(CharacterConditionAsset), menuName = "MisterGames/Character/" + nameof(CharacterConditionAsset))]
    public sealed class CharacterConditionAsset : ScriptableObject, ICharacterCondition {

        [SerializeReference] [SubclassSelector] private ICharacterCondition _condition;

        public bool IsMatch(ICharacterAccess characterAccess) {
            return _condition.IsMatch(characterAccess);
        }
    }

}
