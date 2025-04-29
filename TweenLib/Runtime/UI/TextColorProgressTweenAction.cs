using System;
using MisterGames.Tweens;
using TMPro;
using UnityEngine;

namespace MisterGames.TweenLib.UI {
    
    [Serializable]
    public sealed class TextColorProgressTweenAction : ITweenProgressAction {
        
        public TMP_Text text;
        public Color color0;
        public Color color1;
        
        public void OnProgressUpdate(float progress) {
            text.color = Color.Lerp(color0, color1, progress);
        }
    }
    
}