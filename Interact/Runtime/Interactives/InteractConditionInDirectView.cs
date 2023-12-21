using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionInDirectView : IInteractCondition {

        public bool shouldBeInDirectView;

        public bool IsMatched((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return shouldBeInDirectView == user.IsInDirectView(interactive, out _);
        }
    }

}
