using System;
using MisterGames.Logic.Transforms;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Logic {
    
    [Serializable]
    public sealed class SetWeightMulActionRunnerProgressAction : ITweenProgressAction {
        
        public WeightActionRunner weightActionRunner;
        [Min(0f)] public float startValue;
        [Min(0f)] public float endValue;
        
        public void OnProgressUpdate(float progress) {
            weightActionRunner.WeightMul = Mathf.Lerp(startValue, endValue, progress);
        }
    }
    
}