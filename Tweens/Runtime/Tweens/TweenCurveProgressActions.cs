using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {
    
    [Serializable]
    public sealed class TweenCurveProgressActions : ITweenProgressAction {

        public Effect[] effects;
        
        [Serializable]
        public struct Effect {
            public AnimationCurve progressCurve;
            [SerializeReference] [SubclassSelector] public ITweenProgressAction progressAction;
        }
        
        public void OnProgressUpdate(float progress) {
            for (int i = 0; i < effects.Length; i++) {
                ref var effect = ref effects[i];
                effect.progressAction?.OnProgressUpdate(effect.progressCurve?.Evaluate(progress) ?? progress);
            }
        }
    }
    
}