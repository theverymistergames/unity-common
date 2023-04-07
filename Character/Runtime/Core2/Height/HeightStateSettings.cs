using UnityEngine;

namespace MisterGames.Character.Core2.Height {
    
    [CreateAssetMenu(fileName = nameof(HeightStateSettings), menuName = "MisterGames/Character/" + nameof(HeightStateSettings))]
    public class HeightStateSettings : ScriptableObject {

        [Min(0f)] public float jumpForceMultiplier;
        public float colliderHeight;
        
    }

}
