using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.ActionLib.Rendering {

    [Serializable]
    public sealed class QualityTierCondition : IActorCondition {

        [Min(0)] public int[] tiers;

        public bool IsMatch(IActor context) {
            return tiers != null && tiers.Contains(QualitySettings.GetQualityLevel());
        }
    }

}
