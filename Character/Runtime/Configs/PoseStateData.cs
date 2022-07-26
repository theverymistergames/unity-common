using UnityEngine;

namespace MisterGames.Character.Configs {
    
    [CreateAssetMenu(fileName = nameof(PoseStateData), menuName = "MisterGames/Character/" + nameof(PoseStateData))]
    public class PoseStateData : ScriptableObject {

        public bool isCrouchState;
        public bool isJumpAllowed;
        public float colliderHeight;
        
    }

}