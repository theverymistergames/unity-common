using System;
using MisterGames.Logic.Rendering;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Rendering {

    [Serializable]
    public sealed class SetLightIntensityNoiseMulProgressAction : ITweenProgressAction {

        public LightIntensityNoise lightIntensityNoise;
        [Min(0f)] public float startValue;
        [Min(0f)] public float endValue = 1f;

        public void OnProgressUpdate(float progress) {
            lightIntensityNoise.IntensityMul = Mathf.Lerp(startValue, endValue, progress);
        }
    }

}
