using System;
using MisterGames.Logic.Transforms;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class SetTransformNoiseAmplitudeProgressAction : ITweenProgressAction {

        public TransformNoise transformNoise;
        public float noiseAmplitudeStart;
        public float noiseAmplitudeEnd;
        
        public void OnProgressUpdate(float progress) {
            transformNoise.NoiseAmplitude = Mathf.Lerp(noiseAmplitudeStart, noiseAmplitudeEnd, progress); 
        }
    }
}