using System.Collections.Generic;

namespace MisterGames.Character.Inventory {

    public sealed class Inventory : IInventory {

        public IReadOnlyDictionary<InventoryItemAsset, InventoryItemStackData> Items => _storage.Items;
        public bool IsEnabled { get; set; }
        private readonly IInventoryStorage _storage;

        public Inventory(IInventoryStorage storage) {
            _storage = storage;
        }

        public int AddItems(InventoryItemAsset asset, int count) {
            return _storage.AddItems(asset, count);
        }

        public int RemoveItems(InventoryItemAsset asset, int count) {
            return _storage.RemoveItems(asset, count);
        }

        public int RemoveAllItemsOf(InventoryItemAsset asset) {
            return _storage.RemoveAllItemsOf(asset);
        }

        public void Clear() {
            _storage.Clear();
        }
    }

}
