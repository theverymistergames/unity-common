using System.Collections.Generic;

namespace MisterGames.Character.Inventory {

    public interface IInventoryStorage {

        IReadOnlyDictionary<InventoryItemAsset, int> Items { get; }

        int AddItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel);

        int RemoveItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel);

        void Clear();
    }

}
