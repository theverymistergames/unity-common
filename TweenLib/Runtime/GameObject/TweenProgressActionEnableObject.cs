using System;
using MisterGames.Common.GameObjects;
using MisterGames.Tweens;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionEnableObject : ITweenProgressAction {

        public Object target;
        public bool enabledBeforeThreshold;
        [Range(0f, 1f)] public float enableThreshold;
        
        public void OnProgressUpdate(float progress) {
            target.SetEnabled(progress <= enableThreshold == enabledBeforeThreshold);
        }
    }

}
