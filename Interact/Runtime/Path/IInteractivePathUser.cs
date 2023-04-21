using MisterGames.Interact.Interactives;

namespace MisterGames.Interact.Path {

    public interface IInteractivePathUser {
        void OnAttachedToPath(IInteractiveUser user, IInteractivePath path, float t);
        void OnDetachedFromPath(IInteractiveUser user, IInteractivePath path);
    }

}
