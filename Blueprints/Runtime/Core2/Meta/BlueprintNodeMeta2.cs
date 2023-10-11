using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintNodeMeta2 {

        public int factoryId;


        public int nodeId;


        public Vector2 position;


        public Port[] ports;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetNodeAddress() {
            return BlueprintNodeAddress.Pack(factoryId, nodeId);
        }

        public void RecreatePorts(BlueprintAsset blueprint) {
            //var ports = _node.CreatePorts();
            //if (_node is IBlueprintPortDecorator decorator) decorator.DecoratePorts(blueprint, _nodeId, ports);
            //_ports = ports;
        }

        public void OnValidateNode(BlueprintAsset blueprint) {
            //_node.OnValidate();
            //if (_node is IBlueprintAssetValidator validator) validator.ValidateBlueprint(blueprint, _nodeId);
        }
    }

}
