using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxUsers : IInteractCondition {

        [Min(0)] public int maxUsers;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return interactive.Users.Count <= maxUsers;
        }
    }

}
