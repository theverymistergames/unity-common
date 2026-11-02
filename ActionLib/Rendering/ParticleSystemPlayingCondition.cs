using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Rendering {

    [Serializable]
    public sealed class ParticleSystemPlayingCondition : IActorCondition {

        public ParticleSystem particleSystem;

        public bool IsMatch(IActor context) {
            return particleSystem != null && particleSystem.isPlaying;
        }
    }

}
