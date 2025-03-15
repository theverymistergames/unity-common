using System;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class ProgressModulatorOscillated : IProgressModulator {

        public OscillatedCurve curve;

        public float Modulate(float progress) {
            return curve.Evaluate(progress);
        }
    }
    
}