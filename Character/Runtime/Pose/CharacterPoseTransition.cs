using MisterGames.Character.Actions;
using MisterGames.Character.Conditions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Pose {

    [CreateAssetMenu(fileName = nameof(CharacterPoseTransition), menuName = "MisterGames/Character/" + nameof(CharacterPoseTransition))]
    public sealed class CharacterPoseTransition : ScriptableObject {

        [Min(0f)] public float duration;
        [Range(0f, 1f)] public float changePoseAt;
        public CharacterPoseType targetPose;
        [SerializeReference] [SubclassSelector] public ICharacterCondition condition;
        [SerializeReference] [SubclassSelector] public ICharacterAction action;
    }

}
