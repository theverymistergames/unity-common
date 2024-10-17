using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseGraph), menuName = "MisterGames/Character/" + nameof(CharacterPoseGraph))]
    public sealed class CharacterPoseGraph : ScriptableObject {

        [Header("Poses")]
        public CharacterPose initialPose;
        public CharacterPose crouchPose;
        public CharacterPose standPose;
        
        [Header("Transitions")]
        public float retryDelay = 0.1f;
        public CharacterPoseTransition[] transitions;
    }

}
