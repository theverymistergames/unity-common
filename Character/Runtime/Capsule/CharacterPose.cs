using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPose), menuName = "MisterGames/Character/" + nameof(CharacterPose))]
    public sealed class CharacterPose : ScriptableObject {

        [SerializeField] private CharacterCapsuleSize _capsuleSize;

        public CharacterCapsuleSize CapsuleSize => _capsuleSize;
    }

}
