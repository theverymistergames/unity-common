using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using MisterGames.Tweens;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionEnableObject : ITweenProgressAction {

        public Object target;
        [FormerlySerializedAs("enabledBeforeThreshold")] 
        public Mode mode;
        [Range(0f, 1f)] public float enableThreshold;
        [VisibleIf(nameof(mode), 2, CompareMode.GreaterOrEqual)]
        [Range(0f, 1f)] public float enableThreshold1;

        public enum Mode {
            EnableAfterThreshold,
            EnableBeforeThreshold,
            EnableInsideThresholds,
            EnableOutsideThresholds,
        }
        
        public void OnProgressUpdate(float progress) {
            bool enable = mode switch {
                Mode.EnableAfterThreshold => progress >= enableThreshold,
                Mode.EnableBeforeThreshold => progress <= enableThreshold,
                Mode.EnableInsideThresholds => progress >= Mathf.Min(enableThreshold, enableThreshold1) &&
                                               progress <= Mathf.Max(enableThreshold, enableThreshold1),
                Mode.EnableOutsideThresholds => progress <= Mathf.Min(enableThreshold, enableThreshold1) &&
                                                progress >= Mathf.Max(enableThreshold, enableThreshold1),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            target.SetEnabled(enable);
        }
    }

}
