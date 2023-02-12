using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintPortLinker, IBlueprintAssetValidator {

        [SerializeField] [Range(1, 32)] private int _exits = 1;

        public override Port[] CreatePorts() {
            var ports = new Port[1 + _exits];

            ports[0] = Port.Enter();
            for (int p = 1; p < ports.Length; p++) {
                ports[p] = Port.Exit();
            }

            return ports;
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }

        public int GetLinkedPorts(int port, out int count) {
            if (port == 0) {
                count = _exits;
                return 1;
            }

            if (port > 0 && port <= _exits) {
                count = 1;
                return 0;
            }

            count = 0;
            return -1;
        }
    }

}
