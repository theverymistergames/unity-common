using System.Collections.Generic;

namespace MisterGames.Character.Inventory {

    public interface IInventory {

        bool IsEnabled { get; set; }

        IReadOnlyDictionary<InventoryItemAsset, int> Items { get; }

        int AddItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel);

        int RemoveItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel);

        bool ContainsItems(InventoryItemAsset asset);

        void Clear();

    }

}
