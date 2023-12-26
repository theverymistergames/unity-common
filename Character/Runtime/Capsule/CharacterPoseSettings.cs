using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseSettings), menuName = "MisterGames/Character/" + nameof(CharacterPoseSettings))]
    public sealed class CharacterPoseSettings : ScriptableObject {

        [Header("Poses")]
        public CharacterCapsuleSize stand;
        public CharacterCapsuleSize crouch;

        [Header("Transitions")]
        public CharacterPoseTransition[] transitions;
    }

}
