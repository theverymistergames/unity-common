using System;
using MisterGames.Common.Audio;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class ProgressTweenActionSoundVolume : ITweenProgressAction {

        public Transform transform;
        public HashId attachId;
        public float startValue;
        public float endValue;
        
        public void OnProgressUpdate(float progress) {
            var handle = AudioPool.Main?.GetAudioHandle(transform, attachId) ?? default;
            handle.Volume = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}