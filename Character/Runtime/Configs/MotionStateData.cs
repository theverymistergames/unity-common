using UnityEngine;

namespace MisterGames.Character.Configs {

    [CreateAssetMenu(fileName = nameof(MotionStateData), menuName = "MisterGames/Character/" + nameof(MotionStateData))]
    public class MotionStateData : ScriptableObject {

        [Header("Speed")]
        [Min(0f)] public float speed;
        
        [Header("Speed correction")]
        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;
        
        [Header("Jump")]
        [Min(0f)] public float jumpForce;

        [Header("Constraints")]
        public bool isRunState;
        public bool isJumpAllowedGrounded;

    }

}