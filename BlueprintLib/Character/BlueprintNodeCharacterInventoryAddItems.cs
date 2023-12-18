using System;
using MisterGames.Blueprints;
using MisterGames.Character.Core;
using MisterGames.Character.Inventory;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceCharacterInventoryAddItems :
        BlueprintSource<BlueprintNodeCharacterInventoryAddItems>,
        BlueprintSources.IEnter<BlueprintNodeCharacterInventoryAddItems>,
        BlueprintSources.IOutput<BlueprintNodeCharacterInventoryAddItems, int>,
        BlueprintSources.IOutput<BlueprintNodeCharacterInventoryAddItems, bool>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Add Inventory Items", Category = "Character", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeCharacterInventoryAddItems :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<int>,
        IBlueprintOutput<bool>
    {
        [SerializeField] private InventoryItemAsset _asset;
        [SerializeField] [Min(0)] private int _count;
        [SerializeField] private InventoryItemStackOverflowPolicy _overflowPolicy;

        private int _lastAddedCount;
        private int _lastRequestedAddCount;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<CharacterAccess>());
            meta.AddPort(id, Port.Input<InventoryItemAsset>("Item Asset"));
            meta.AddPort(id, Port.Input<int>("Count"));
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Output<int>("Added count"));
            meta.AddPort(id, Port.Output<bool>("Success"));
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var characterAccess = blueprint.Read<CharacterAccess>(token, port: 1);
            var inventory = characterAccess.GetPipeline<ICharacterInventoryPipeline>().Inventory;

            var asset = blueprint.Read(token, port: 2, _asset);
            int count = blueprint.Read(token, port: 3, _count);

            _lastRequestedAddCount = count;
            _lastAddedCount = inventory.AddItems(asset, count, _overflowPolicy);

            blueprint.Call(token, port: 4);
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastAddedCount,
            _ => default,
        };

        bool IBlueprintOutput<bool>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            6 => _lastAddedCount >= _lastRequestedAddCount,
            _ => default,
        };
    }

}
