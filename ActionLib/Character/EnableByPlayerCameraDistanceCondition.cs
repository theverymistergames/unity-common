using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Behaviours;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class EnableByPlayerCameraDistanceCondition : IActorCondition {

        public EnableByPlayerCameraDistance enableByDistance;
        public bool isActivated = true;

        public bool IsMatch(IActor context) {
            return enableByDistance.IsActivated == isActivated;
        }
    }

}
