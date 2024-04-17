using System;
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
            bool enabled = progress <= enableThreshold == enabledBeforeThreshold;

            switch (target) {
                case GameObject go:
                    go.SetActive(enabled);
                    break;
                
                case Behaviour bhv:
                    bhv.enabled = enabled;
                    break;
                
                case Collider col:
                    col.enabled = enabled;
                    break;
            }
        }
    }

}
