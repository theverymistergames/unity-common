using System;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionInDirectView : IInteractCondition {

        public bool shouldBeInDirectView;

        public bool IsMatch((IInteractiveUser, IInteractive) context, float startTime) {
            var (user, interactive) = context;
            return shouldBeInDirectView == user.IsInDirectView(interactive, out _);
        }
    }

}
