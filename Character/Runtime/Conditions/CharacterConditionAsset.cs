using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Conditions {

    [CreateAssetMenu(fileName = nameof(CharacterConditionAsset), menuName = "MisterGames/Character/" + nameof(CharacterConditionAsset))]
    public sealed class CharacterConditionAsset : ScriptableObject {

        [SerializeReference] [SubclassSelector] private ICharacterCondition _condition;

        public bool IsMatched(ICharacterAccess characterAccess) {
            return _condition.IsMatch(characterAccess);
        }
    }

}
