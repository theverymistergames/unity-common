using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public sealed class InventoryStorage : IInventoryStorage {

        [SerializeField] private Map<InventoryItemAsset, InventoryItemStackData> _items;

        public IReadOnlyDictionary<InventoryItemAsset, InventoryItemStackData> Items => _items;

        public InventoryStorage() {
            _items = new Map<InventoryItemAsset, InventoryItemStackData>();
        }

        public int AddItems(InventoryItemAsset asset, int count) {
            if (count <= 0 || asset == null) {
                return 0;
            }

            if (_items.ContainsKey(asset)) {
                ref var data = ref _items.Get(asset);
                data.count += count;
            }
            else {
                _items.Add(asset, new InventoryItemStackData { count = count });
            }

            return count;
        }

        public int RemoveItems(InventoryItemAsset asset, int count) {
            if (count <= 0 || asset == null || !_items.ContainsKey(asset)) {
                return 0;
            }

            ref var data = ref _items.Get(asset);

            if (data.count > count) {
                data.count -= count;
            }
            else {
                count = data.count;
                _items.Remove(asset);
            }

            return count;
        }

        public int RemoveAllItemsOf(InventoryItemAsset asset) {
            if (asset == null || !_items.TryGetValue(asset, out var data)) {
                return 0;
            }

            _items.Remove(asset);
            return data.count;
        }

        public void Clear() {
            _items.Clear();
        }
    }

}
