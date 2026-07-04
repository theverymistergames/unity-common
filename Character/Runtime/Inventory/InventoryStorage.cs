using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    public sealed class InventoryStorage : IInventoryStorage {
        
        public IReadOnlyDictionary<InventoryItemAsset, int> Items => _items;
        private readonly Dictionary<InventoryItemAsset, int> _items = new();

        public int AddItems(
            InventoryItemAsset asset,
            int count,
            InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel) 
        {
            if (count <= 0 || asset == null) {
                return 0;
            }

            _items.TryGetValue(asset, out int existent);
            _items[asset] = existent + count;

            return count;
        }

        public int RemoveItems(
            InventoryItemAsset asset,
            int count,
            InventoryItemStackOverflowPolicy policy = InventoryItemStackOverflowPolicy.Cancel) 
        {
            if (count <= 0 || 
                asset == null || 
                !_items.TryGetValue(asset, out int existent)) 
            {
                return 0;
            }

            if (existent >= count) {
                existent -= count;
                
                if (existent <= 0) _items.Remove(asset);
                else _items[asset] = existent;
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
            if (asset == null || !_items.Remove(asset, out int existent)) {
                return 0;
            }

            return existent;
        }

        public void Clear() {
            _items.Clear();
        }
    }

}
