using System;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public struct InventoryItemStack {
        public InventoryItemAsset asset;
        [Min(0)] public int count;
    }

}
