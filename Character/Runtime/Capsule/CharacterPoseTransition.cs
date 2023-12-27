using MisterGames.Character.Actions;
using MisterGames.Character.Conditions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseTransition), menuName = "MisterGames/Character/" + nameof(CharacterPoseTransition))]
    public sealed class CharacterPoseTransition : ScriptableObject {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] [Range(0f, 1f)] private float _setPoseAt;
        [SerializeReference] [SubclassSelector] private ICharacterCondition _condition;
        [SerializeReference] [SubclassSelector] private ICharacterAction _action;

        public float Duration => _duration;
        public float SetPoseAt => _setPoseAt;
        public ICharacterCondition Condition => _condition;
        public ICharacterAction Action => _action;
    }

}
