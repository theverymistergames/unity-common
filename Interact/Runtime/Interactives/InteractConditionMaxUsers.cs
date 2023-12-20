using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxUsers : IInteractCondition {

        [Min(0)] public int maxUsers;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return interactive.Users.Count <= maxUsers;
        }
    }

}
