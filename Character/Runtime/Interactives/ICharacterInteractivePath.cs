using MisterGames.Character.Core;
using MisterGames.Interact.Interactives;
using MisterGames.Interact.Path;

namespace MisterGames.Character.Interactives {

    public delegate void CharacterInteractivePathEvent(
        float t,
        ICharacterAccess characterAccess,
        IInteractiveUser user,
        IInteractive interactive
    );

    public interface ICharacterInteractivePath {

        event CharacterInteractivePathEvent OnAttach;
        event CharacterInteractivePathEvent OnDetach;

        IInteractivePath Path { get; }

        void AttachToPath(
            ICharacterAccess characterAccess,
            IInteractiveUser user,
            IInteractive interactive
        );

        void DetachFromPath(
            ICharacterAccess characterAccess,
            IInteractiveUser user,
            IInteractive interactive
        );
    }

}
