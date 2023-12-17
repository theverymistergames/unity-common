using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    public sealed class CharacterInventoryPipeline : CharacterPipelineBase, ICharacterInventoryPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private InventoryItemStack[] _addItems;
        [SerializeField] [HideInInspector] private InventoryStorage _storage;

        public IInventory Inventory => GetOrCreateInventoryInstance();
        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private Inventory _inventoryInstance;

        private void Awake() {
            GetOrCreateInventoryInstance();
        }

        private void OnDestroy() {
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
                _storage.AddItems(stack.asset, stack.data.count);
            }

            _inventoryInstance = new Inventory(_storage);
            return _inventoryInstance;
        }

        private void SetEnabled(bool isEnabled) {
            if (_inventoryInstance != null) _inventoryInstance.IsEnabled = isEnabled;
        }
    }

}
