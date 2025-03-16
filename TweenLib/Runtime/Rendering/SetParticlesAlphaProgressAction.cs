using System;
using MisterGames.Logic.Rendering;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Rendering {
    
    [Serializable]
    public sealed class SetParticlesAlphaProgressAction : ITweenProgressAction {
        
        public ParticleAlphaController particleAlphaController;
        [Range(0f, 1f)] public float startValue;
        [Range(0f, 1f)] public float endValue;
        
        public void OnProgressUpdate(float progress) {
            particleAlphaController.SetAlpha(Mathf.Lerp(startValue, endValue, progress));
        }
    }
    
}