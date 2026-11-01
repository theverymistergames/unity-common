using System;
using UnityEngine;

namespace MisterGames.Common.Inputs {

    [Serializable]
    public sealed class GamepadHidColorAnimation {

        public GamepadHidColorKey[] keys;

        public GamepadHidColorState Evaluate(float t) {
            if (keys == null || keys.Length == 0) return default;

            t = Mathf.Clamp01(t);

            int last = keys.Length - 1;
            if (last == 0 || t <= keys[0].progress) return keys[0].state;
            if (t >= keys[last].progress) return keys[last].state;

            int i = 0;
            while (i < last - 1 && keys[i + 1].progress <= t) i++;

            var a = keys[i];
            var b = keys[i + 1];

            float span = b.progress - a.progress;
            float localT = span > 0f ? (t - a.progress) / span : 1f;
            var discrete = localT < 0.5f ? a.state : b.state;

            return new GamepadHidColorState(
                Color.Lerp(a.state.color, b.state.color, localT),
                Mathf.Lerp(a.state.intensity, b.state.intensity, localT),
                discrete.lightOptions
            );
        }
    }

}
