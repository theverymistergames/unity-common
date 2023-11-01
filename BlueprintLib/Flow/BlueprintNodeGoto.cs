using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGoto :
        BlueprintSource<BlueprintNodeGoto2>,
        BlueprintSources.IInternalLink<BlueprintNodeGoto2>,
        BlueprintSources.IHashLink<BlueprintNodeGoto2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Goto", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeGoto2 : IBlueprintNode, IBlueprintInternalLink, IBlueprintHashLink {

        [SerializeField] private string _label;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit().Hide(true));
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (port == 0) {
                index = 1;
                count = 1;
                return;
            }

            index = -1;
            count = 0;
        }

        public void GetLinkedPort(NodeId id, out int hash, out int port) {
            hash = string.IsNullOrWhiteSpace(_label) ? 0 : _label.GetHashCode();
            port = 1;
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Goto", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeGoto : BlueprintNode, IBlueprintPortLinker, IBlueprintNodeLinker {

        [SerializeField] private string _label;

        public int LinkerNodeHash => string.IsNullOrWhiteSpace(_label) ? 0 : _label.GetHashCode();
        public int LinkerNodePort => 1;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit().Hide(true),
        };

        public int GetLinkedPorts(int port, out int count) {
            if (port == 0) {
                count = 1;
                return 1;
            }

            if (port == 1) {
                count = 1;
                return 0;
            }

            count = 0;
            return -1;
        }
    }

}
