using System;
using MisterGames.Logic.Interactives;
using MisterGames.Tweens;
using UnityEngine;
using UnityEngine.Rendering;

namespace MisterGames.TweenLib.Logic {
    
    [Serializable]
    public sealed class SetVolumeWeightProgressAction : ITweenProgressAction {
        
        public Volume volume;
        [Min(0f)] public float startValue;
        [Min(0f)] public float endValue;
        
        public void OnProgressUpdate(float progress) {
            volume.weight = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}