using System;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class ProgressModulatorNoise : IProgressModulator {
        
        public float seed;
        public float scale = 1f;
        public bool clamp01 = true;
        public float speedMul = 1f;
        public AnimationCurve speedCurve = AnimationCurve.Constant(0f, 1f, 1f);

        public float Modulate(float progress) {
            float p = 0.5f + (Mathf.PerlinNoise1D(progress * speedCurve.Evaluate(progress) * speedMul + seed) - 0.5f) * scale;
            return clamp01 ? Mathf.Clamp01(p) : p;
        }
    }
    
}