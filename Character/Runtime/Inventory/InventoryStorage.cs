using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public sealed class InventoryStorage : IInventoryStorage {

        [SerializeField] private Map<InventoryItemAsset, int> _items;

        public IReadOnlyDictionary<InventoryItemAsset, int> Items => _items;

        public InventoryStorage() {
            _items = new Map<InventoryItemAsset, int>();
        }

        public int AddItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel) {
            if (count <= 0 || asset == null) {
                return 0;
            }

            _items.TryGetValue(asset, out int existent);
            _items[asset] = existent + count;

            return count;
        }

        public int RemoveItems(InventoryItemAsset asset, int count, InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel) {
            if (count <= 0 || asset == null || !_items.ContainsKey(asset)) {
                return 0;
            }

            ref int existent = ref _items.Get(asset);

            if (existent >= count) {
                existent -= count;
                if (existent <= 0) _items.Remove(asset);
            }
            else {
                switch (policy) {
                    case InventoryItemStackOverflowPolicy.Cancel:
                        count = 0;
                        break;

                    case InventoryItemStackOverflowPolicy.AsManyAsPossible:
                        count = existent;
                        _items.Remove(asset);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
                }
            }

            return count;
        }

        public int RemoveAllItemsOf(InventoryItemAsset asset) {
            if (asset == null || !_items.TryGetValue(asset, out int existent)) {
                return 0;
            }

            _items.Remove(asset);

            return existent;
        }

        public void Clear() {
            _items.Clear();
        }
    }

}
