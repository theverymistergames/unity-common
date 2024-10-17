using System;
using MisterGames.Actors;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterViewData : IActorData {
        
        public Vector2 sensitivity = new Vector2(0.15f, 0.15f);
        public float viewSmoothing = 20f;
        [Min(0f)] public float fov = 70f;
        public ViewAxisClamp horizontalClamp;
        public ViewAxisClamp verticalClamp = new() {
            absolute = true,
            mode = ClampMode.Full, 
            bounds = new Vector2(-90f, 90f)
        };
    }
    
    
}