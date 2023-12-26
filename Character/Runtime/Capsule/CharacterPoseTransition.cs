using MisterGames.Character.Actions;
using MisterGames.Character.Conditions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseTransition), menuName = "MisterGames/Character/" + nameof(CharacterPoseTransition))]
    public sealed class CharacterPoseTransition : ScriptableObject {

        public CharacterPoseType sourcePose;
        public CharacterPoseType targetPose;

        [Min(0f)] public float duration;
        [Range(0f, 1f)] public float setPoseAt;

        [SerializeReference] [SubclassSelector] public ICharacterCondition condition;
        [SerializeReference] [SubclassSelector] public ICharacterAction action;
    }

}
