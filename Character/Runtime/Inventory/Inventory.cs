using System.Collections.Generic;

namespace MisterGames.Character.Inventory {

    public sealed class Inventory : IInventory {

        public IReadOnlyDictionary<InventoryItemAsset, InventoryItemStackData> Items => _storage.Items;
        public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }

        private readonly IInventoryStorage _storage;
        private bool _isEnabled;

        public Inventory(IInventoryStorage storage) {
            _storage = storage;
        }

        public int AddItems(InventoryItemAsset asset, int count) {
            if (!_isEnabled) return 0;
            return _storage.AddItems(asset, count);
        }

        public int RemoveItems(InventoryItemAsset asset, int count) {
            if (!_isEnabled) return 0;
            return _storage.RemoveItems(asset, count);
        }

        public int RemoveAllItemsOf(InventoryItemAsset asset) {
            if (!_isEnabled) return 0;
            return _storage.RemoveAllItemsOf(asset);
        }

        public void Clear() {
            _storage.Clear();
        }
    }

}
