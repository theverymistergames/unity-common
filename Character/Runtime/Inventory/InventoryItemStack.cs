using System;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public struct InventoryItemStack {
        public InventoryItemAsset asset;
        public InventoryItemStackData data;
    }

}
