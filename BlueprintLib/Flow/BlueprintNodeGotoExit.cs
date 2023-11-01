using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGotoExit :
        BlueprintSource<BlueprintNodeGotoExit2>,
        BlueprintSources.IInternalLink<BlueprintNodeGotoExit2>,
        BlueprintSources.IHashLink<BlueprintNodeGotoExit2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Goto (exit)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeGotoExit2 : IBlueprintNode, IBlueprintInternalLink, IBlueprintHashLink {

        [SerializeField] private string _label;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Enter().Hide(true));
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (port == 1) {
                index = 0;
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
    [BlueprintNodeMeta(Name = "Goto (exit)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeGotoExit : BlueprintNode, IBlueprintPortLinker, IBlueprintNodeLinker {
        
        [SerializeField] private string _label;

        public int LinkerNodeHash => string.IsNullOrWhiteSpace(_label) ? 0 : _label.GetHashCode();
        public int LinkerNodePort => 1;

        public override Port[] CreatePorts() => new[] {
            Port.Exit(),
            Port.Enter().Hide(true),
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
