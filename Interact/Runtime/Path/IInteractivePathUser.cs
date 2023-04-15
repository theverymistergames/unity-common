namespace MisterGames.Interact.Path {

    public interface IInteractivePathUser {
        void OnAttachedToPath(IInteractivePath path, float t);
        void OnDetachedFromPath();
    }

}
