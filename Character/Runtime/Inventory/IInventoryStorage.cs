using System.Collections.Generic;

namespace MisterGames.Character.Inventory {

    public interface IInventoryStorage {

        IReadOnlyDictionary<InventoryItemAsset, InventoryItemStackData> Items { get; }

        int AddItems(InventoryItemAsset asset, int count);

        int RemoveItems(InventoryItemAsset asset, int count);

        int RemoveAllItemsOf(InventoryItemAsset asset);

        void Clear();
    }

}
