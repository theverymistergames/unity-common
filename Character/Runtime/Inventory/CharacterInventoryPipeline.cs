using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    public sealed class CharacterInventoryPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private InventoryItemStack[] _addItems;
        [SerializeField] [HideInInspector] private InventoryStorage _storage;

        public IInventory Inventory => GetOrCreateInventoryInstance();
        private Inventory _inventoryInstance;

        public void OnAwake(IActor actor) {
            GetOrCreateInventoryInstance();
        }

        public void OnTerminate(IActor actor) {
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

            for (int i = 0; i < _addItems.Length; i++) {
                var stack = _addItems[i];
                _storage.AddItems(stack.asset, stack.count);
            }

            _inventoryInstance = new Inventory(_storage);
            return _inventoryInstance;
        }

        private void SetEnabled(bool isEnabled) {
            if (_inventoryInstance != null) _inventoryInstance.IsEnabled = isEnabled;
        }
    }

}
