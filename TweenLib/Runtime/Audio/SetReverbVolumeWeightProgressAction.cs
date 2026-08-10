using System;
using MisterGames.Common.Audio;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class SetReverbVolumeWeightProgressAction : ITweenProgressAction {

        public ReverbVolume reverbVolume;
        [Range(0f, 1f)] public float startValue;
        [Range(0f, 1f)] public float endValue;
        
        public void OnProgressUpdate(float progress) {
            reverbVolume.Weight = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}