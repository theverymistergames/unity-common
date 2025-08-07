using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPose), menuName = "MisterGames/Character/" + nameof(CharacterPose))]
    public sealed class CharacterPose : ScriptableObject {

        [SerializeField] private CapsuleSize _capsuleSize;

        public CapsuleSize CapsuleSize => _capsuleSize;
    }

}
