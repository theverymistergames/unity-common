using System;
using UnityEngine;

namespace MisterGames.Character.Capsule {
    
    [Serializable]
    public struct CharacterCapsuleSize {

        [Min(0f)] public float height;
        [Min(0f)] public float radius;

        public override string ToString() {
            return $"{nameof(CharacterCapsuleSize)}(h: {height}, r: {radius})";
        }
    }
    
}
