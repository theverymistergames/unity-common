using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Configs {

    [CreateAssetMenu(fileName = nameof(CharacterMotionSettings), menuName = "MisterGames/Character/" + nameof(CharacterMotionSettings))]
    public class CharacterMotionSettings : ScriptableObject {

        [Header("Speed")]
        [Min(0f)] public float speed;
        
        [Header("Speed correction")]
        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        [Header("Jump")]
        public bool canJumpOnGround;
        [VisibleIf(nameof(canJumpOnGround))] [Min(0f)] public float jumpForceOnGround;

        public bool canJumpInAir;
        [VisibleIf(nameof(canJumpInAir))] [Min(0f)] public float jumpForceInAir;
    }

}
