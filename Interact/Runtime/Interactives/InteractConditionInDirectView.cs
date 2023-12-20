using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionInDirectView : IInteractCondition {

        public bool shouldBeInDirectView;

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            return shouldBeInDirectView == user.IsInDirectView(interactive, out _);
        }
    }

}
