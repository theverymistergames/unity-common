using System;
using UnityEngine;

namespace MisterGames.Character.Pose {
    
    [Serializable]
    public struct CharacterCapsuleSize {
        [Min(0f)] public float colliderHeight;
        [Min(0f)] public float colliderRadius;
    }
    
}
