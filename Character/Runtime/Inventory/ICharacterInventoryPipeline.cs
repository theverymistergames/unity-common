using MisterGames.Character.Core;

namespace MisterGames.Character.Inventory {

    public interface ICharacterInventoryPipeline : ICharacterPipeline {
        IInventory Inventory { get; }
    }

}
