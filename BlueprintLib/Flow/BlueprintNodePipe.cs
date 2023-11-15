using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourcePipe :
        BlueprintSource<BlueprintNodePipe2>,
        BlueprintSources.IInternalLink<BlueprintNodePipe2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodePipe2 : IBlueprintNode, IBlueprintInternalLink {

        [SerializeField] [Range(1, 32)] private int _exits;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            _exits = 1;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            for (int p = 0; p < _exits; p++) {
                meta.AddPort(id, Port.Exit());
            }
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (port == 0) {
                index = 1;
                count = _exits;
                return;
            }

            index = -1;
            count = 0;
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodePipe : BlueprintNode, IBlueprintPortLinker

#if UNITY_EDITOR
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] [Range(1, 32)] private int _exits = 1;

        public override Port[] CreatePorts() {
            var ports = new Port[1 + _exits];

            ports[0] = Port.Enter();
            for (int p = 1; p < ports.Length; p++) {
                ports[p] = Port.Exit();
            }

            return ports;
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

#if UNITY_EDITOR
        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
#endif
    }

}
