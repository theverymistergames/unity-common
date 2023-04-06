using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    [CreateAssetMenu(fileName = nameof(CharacterMotionSettings), menuName = "MisterGames/Character/" + nameof(CharacterMotionSettings))]
    public class CharacterMotionSettings : ScriptableObject {

        [Header("Speed")]
        [Min(0f)] public float speed;
        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        [Header("Jump")]
        [Min(0f)] public float jumpForce;

        [Header("Run")]
        public bool isCharacterRunning;
    }

}
