using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConstraintAll : IInteractionConstraint {

        [SerializeReference] [SubclassSelector] public IInteractionConstraint[] constraints;

        public bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive) {
            for (int i = 0; i < constraints.Length; i++) {
                if (!constraints[i].IsAllowedInteraction(user, interactive)) return false;
            }

            return true;
        }
    }

}
