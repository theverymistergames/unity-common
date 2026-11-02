using System;
using MisterGames.Tweens;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class ExplosionSimulationProgressAction : ITweenProgressAction {

        public ExplosionSimulation simulation;

        public void OnProgressUpdate(float progress) {
            simulation.SetProgress(progress);
        }
    }

}
