using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Common.Easing {
    
    [Serializable]
    public struct OscillatedCurve {
        
        public AnimationCurve curve0;
        public AnimationCurve curve1;
        public float scale0;
        public float scale1;
        [Min(0f)] public float oscillateFreq0;
        [Min(0f)] public float oscillateFreq1;
        [Range(0f, 1f)] public float oscillateThr;

        public float Evaluate(float t) {
            float v0 = (curve0?.Evaluate(t) ?? 0f) * scale0;
            float v1 = (curve1?.Evaluate(t) ?? 0f) * scale1;
            
            float osc = NumberExtensions.Oscillate(t, oscillateFreq0, oscillateFreq1, oscillateThr);
            float w = (osc + 1f) * 0.5f;
            
            return w * v0 + (1f - w) * v1;
        }
    }
    
}