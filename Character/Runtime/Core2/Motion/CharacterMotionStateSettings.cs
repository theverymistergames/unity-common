using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    [CreateAssetMenu(fileName = nameof(CharacterMotionStateSettings), menuName = "MisterGames/Character/" + nameof(CharacterMotionStateSettings))]
    public class CharacterMotionStateSettings : ScriptableObject {

        [Header("Speed")]
        [Min(0f)] public float speed;
        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        [Header("Jump")]
        [Min(0f)] public float jumpForceMultiplier = 1f;
    }

}
