using System;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class ProgressTweenActionSoundAttenuation : ITweenProgressAction {

        public Transform transform;
        public HashId attachId;
        public float startValue;
        public float endValue;
        
        public void OnProgressUpdate(float progress) {
            var handle = AudioPool.Main?.GetAudioHandle(transform, attachId) ?? default;
            handle.AttenuationMul = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}