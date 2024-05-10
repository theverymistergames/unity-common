using System;
using MisterGames.Actors;
using MisterGames.Blueprints;
using MisterGames.Character.Inventory;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCharacterInventoryRemoveItems :
        BlueprintSource<BlueprintNodeCharacterInventoryRemoveItems>,
        BlueprintSources.IEnter<BlueprintNodeCharacterInventoryRemoveItems>,
        BlueprintSources.IOutput<BlueprintNodeCharacterInventoryRemoveItems, int>,
        BlueprintSources.IOutput<BlueprintNodeCharacterInventoryRemoveItems, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = " Remove Inventory Items", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeCharacterInventoryRemoveItems :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<int>,
        IBlueprintOutput<bool>
    {
        [SerializeField] private InventoryItemAsset _asset;
        [SerializeField] [Min(0)] private int _count;
        [SerializeField] private InventoryItemStackOverflowPolicy _overflowPolicy;

        private int _lastRequestedRemoveCount;
        private int _lastRemovedCount;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<IActor>());
            meta.AddPort(id, Port.Input<InventoryItemAsset>("Item Asset"));
            meta.AddPort(id, Port.Input<int>("Count"));
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Output<int>("Removed count"));
            meta.AddPort(id, Port.Output<bool>("Success"));
        }
        
        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var characterAccess = blueprint.Read<IActor>(token, port: 1);
            var inventory = characterAccess.GetComponent<CharacterInventoryPipeline>().Inventory;

            var asset = blueprint.Read(token, port: 2, _asset);
            int count = blueprint.Read(token, port: 3, _count);

            _lastRequestedRemoveCount = count;
            _lastRemovedCount = inventory.RemoveItems(asset, count, _overflowPolicy);

            blueprint.Call(token, port: 4);
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastRemovedCount,
            _ => default,
        };

        bool IBlueprintOutput<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            6 => _lastRemovedCount >= _lastRequestedRemoveCount,
            _ => default,
        };
    }

}
