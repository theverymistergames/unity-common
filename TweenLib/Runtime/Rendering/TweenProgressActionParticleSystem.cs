using System;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenProgressActionParticleSystem : ITweenProgressAction {

        public ParticleSystem[] particleSystems;
        public bool startedBeforeThreshold;
        [Range(0f, 1f)] public float startThreshold;
        public ParticleSystemStopBehavior onStop;
        
        public void OnProgressUpdate(float progress) {
            bool start = progress <= startThreshold == startedBeforeThreshold;

            for (int i = 0; i < particleSystems.Length; i++) {
                var particleSystem = particleSystems[i];
                if (particleSystem.isPlaying == start) continue;
                
                if (start) particleSystem.Play();
                else particleSystem.Stop(withChildren: true, onStop);
            }
        }
    }

}
