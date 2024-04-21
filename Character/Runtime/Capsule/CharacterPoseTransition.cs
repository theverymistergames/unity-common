using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseTransition), menuName = "MisterGames/Character/" + nameof(CharacterPoseTransition))]
    public sealed class CharacterPoseTransition : ScriptableObject {

        [SerializeField] private CharacterPose _sourcePose;
        [SerializeField] private CharacterPose _targetPose;

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] [Range(0f, 1f)] private float _setPoseAt;

        [SerializeReference] [SubclassSelector] private IActorCondition _condition;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        public CharacterPose SourcePose => _sourcePose;
        public CharacterPose TargetPose => _targetPose;

        public float Duration => _duration;
        public float SetPoseAt => _setPoseAt;

        public IActorCondition Condition => _condition;
        public IActorAction Action => _action;
    }

}
