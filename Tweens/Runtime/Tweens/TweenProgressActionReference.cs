using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class TweenProgressActionReference : ITweenProgressAction {

        public TweenProgressActionContainer tweenProgressActionContainer;
        [MinMaxSlider(0f, 1f)] public Vector2 remapProgress = new(0f, 1f);
        
        public void OnProgressUpdate(float progress) {
            tweenProgressActionContainer.OnProgressUpdate(Mathf.LerpUnclamped(remapProgress.x, remapProgress.y, progress));
        }
    }
    
}