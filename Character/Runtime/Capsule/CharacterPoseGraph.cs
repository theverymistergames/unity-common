using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseGraph), menuName = "MisterGames/Character/" + nameof(CharacterPoseGraph))]
    public sealed class CharacterPoseGraph : ScriptableObject {

        [Header("Poses")]
        [SerializeField] private CharacterPose _initialPose;

        [Header("Transitions")]
        [SerializeField] private CharacterPoseTransition[] _transitions;

        public CharacterPose InitialPose => _initialPose;
        public IReadOnlyList<CharacterPoseTransition> Transitions => _transitions;
    }

}
