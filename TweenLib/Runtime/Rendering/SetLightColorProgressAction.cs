using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Rendering {

    [Serializable]
    public sealed class SetLightColorProgressAction : ITweenProgressAction {

        public Light light;
        public Color color0 = Color.white;
        public Color color1 = Color.white;

        public void OnProgressUpdate(float progress) {
            light.color = Color.Lerp(color0, color1, progress);
        }
    }

}
