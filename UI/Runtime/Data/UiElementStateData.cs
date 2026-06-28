using System;
using UnityEngine;

namespace MisterGames.UI.Data {
    
    [Serializable]
    public struct UiElementStateData {
        [Min(0f)] public float duration;
        public AnimationCurve curve;
        public Color imageColor;
        public Color textColor;
        [Min(0f)] public float scale;
    }
    
}