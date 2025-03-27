using System;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class SetMixerParamProgressAction : ITweenProgressAction {

        public AudioMixer mixer;
        public string param;
        public float startValue;
        public float endValue;
        
        public void OnProgressUpdate(float progress) {
            mixer.SetFloat(param, Mathf.Lerp(startValue, endValue, progress));
        }
    }
    
}