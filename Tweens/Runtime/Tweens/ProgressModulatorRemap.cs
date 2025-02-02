using System;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class ProgressModulatorRemap : IProgressModulator {
        
        public float startValue;
        public float endValue;
        public bool clamp01;

        public float Modulate(float progress) {
            return clamp01 
                ? Mathf.Lerp(startValue, endValue, progress) 
                : Mathf.LerpUnclamped(startValue, endValue, progress);
        }
    }
    
}