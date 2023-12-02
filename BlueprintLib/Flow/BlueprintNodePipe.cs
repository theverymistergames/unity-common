using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourcePipe :
        BlueprintSource<BlueprintNodePipe>,
        BlueprintSources.IInternalLink<BlueprintNodePipe>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Pipe", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodePipe : IBlueprintNode, IBlueprintInternalLink {

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

}
