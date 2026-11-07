using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib.Rendering {

    [Serializable]
    public sealed class SetParticlesColorProgressAction : ITweenProgressAction {

        public ParticleSystem particleSystem;
        public Color color0 = Color.white;
        public Color color1 = Color.white;

        public void OnProgressUpdate(float progress) {
            var main = particleSystem.main;
            main.startColor = Color.Lerp(color0, color1, progress);
        }
    }

}
