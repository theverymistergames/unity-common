using System;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct FloatParameter {

        public float multiplier;
        public float addRandom;
        public AnimationCurve curve;

        public static FloatParameter Default() => new FloatParameter {
            multiplier = 0f,
            addRandom = 0f,
            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
        };
    }

}
