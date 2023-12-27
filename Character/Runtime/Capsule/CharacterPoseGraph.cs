using System;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    [CreateAssetMenu(fileName = nameof(CharacterPoseGraph), menuName = "MisterGames/Character/" + nameof(CharacterPoseGraph))]
    public sealed class CharacterPoseGraph : ScriptableObject {

        [Header("Poses")]
        [SerializeField] private CharacterPoseSettings _stand;
        [SerializeField] private CharacterPoseSettings _crouch;

        public CharacterPoseSettings GetPoseSettings(CharacterPoseType poseType) {
            return poseType switch {
                CharacterPoseType.Stand => _stand,
                CharacterPoseType.Crouch => _crouch,
                _ => throw new ArgumentOutOfRangeException(nameof(poseType), poseType, null)
            };
        }
    }

}
