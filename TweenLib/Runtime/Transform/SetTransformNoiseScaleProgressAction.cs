using System;
using MisterGames.Logic.Transforms;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {
    
    [Serializable]
    public sealed class SetTransformNoiseScaleProgressAction : ITweenProgressAction {

        public TransformNoise transformNoise;
        public float noiseScaleStart;
        public float noiseScaleEnd;
        
        public void OnProgressUpdate(float progress) {
            transformNoise.NoiseScale = Mathf.Lerp(noiseScaleStart, noiseScaleEnd, progress); 
        }
    }
}