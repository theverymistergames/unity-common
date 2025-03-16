using System;
using MisterGames.Logic.Rendering;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Rendering {
    
    [Serializable]
    public sealed class SetLampWeightProgressAction : ITweenProgressAction {
        
        public LampBehaviour lamp;
        [Min(0f)] public float startValue;
        [Min(0f)] public float endValue;
        
        public void OnProgressUpdate(float progress) {
            lamp.Weight = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}