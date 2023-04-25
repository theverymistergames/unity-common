using MisterGames.Character.Core;
using MisterGames.Interact.Interactives;

namespace MisterGames.Character.Interactives {

    public interface ICharacterInteractionPipeline : ICharacterPipeline {
        IInteractiveUser InteractiveUser { get; }
    }

}
