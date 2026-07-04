using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    public sealed class CharacterInventoryPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private InventoryItemStack[] _addItems;
        
        public IInventory Inventory => GetOrCreateInventoryInstance();
        private Inventory _inventoryInstance;
        
        public void OnAwake(IActor actor) {
            GetOrCreateInventoryInstance();
        }

        public void OnDestroyed(IActor actor) {
            _inventoryInstance = null;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private Inventory GetOrCreateInventoryInstance() {
            if (_inventoryInstance != null) return _inventoryInstance;

            var storage = new InventoryStorage();
            
            for (int i = 0; i < _addItems.Length; i++) {
                var stack = _addItems[i];
                storage.AddItems(stack.asset, stack.count);
            }

            _inventoryInstance = new Inventory(storage);
            return _inventoryInstance;
        }

        private void SetEnabled(bool isEnabled) {
            if (_inventoryInstance != null) _inventoryInstance.IsEnabled = isEnabled;
        }
    }

}
