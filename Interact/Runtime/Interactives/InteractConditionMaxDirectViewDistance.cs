using System;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionMaxDirectViewDistance : IInteractCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return user.IsInDirectView(interactive, out float distance) &&
                   distance <= maxDistance;
        }
    }

}
