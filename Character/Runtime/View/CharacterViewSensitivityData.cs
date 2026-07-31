using System;
using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Character.View {
    
    [Serializable]
    public sealed class CharacterViewSensitivityData : IActorData {
        
        [Header("Mouse")]
        public LabelValue mouseSensitivityId;
        public LabelValue mouseInvertYId;
        public Vector2 mouseSensitivityBase;
        [Min(0f)] public float mouseSensitivity0;
        [Min(0f)] public float mouseSensitivity1;
       
        [Header("Gamepad")]
        public LabelValue gamepadSensitivityId;
        public LabelValue gamepadInvertYId;
        public Vector2 gamepadSensitivityBase;
        [Min(0f)] public float gamepadSensitivity0;
        [Min(0f)] public float gamepadSensitivity1;
    }
    
}