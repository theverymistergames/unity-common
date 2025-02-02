using System;
using MisterGames.Common.Data;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Animations {
    
    [Serializable]
    public sealed class ProgressTweenActionAnimatorFloat : ITweenProgressAction {

        public Animator animator;
        public HashId parameter;
        public float startValue;
        public float endValue;
        
        public void OnProgressUpdate(float progress) {
            animator.SetFloat(parameter, Mathf.Lerp(startValue, endValue, progress));
        }
    }
    
}