using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceGotoExit :
        BlueprintSource<BlueprintNodeGotoExit>,
        BlueprintSources.IInternalLink<BlueprintNodeGotoExit>,
        BlueprintSources.IHashLink<BlueprintNodeGotoExit>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Goto (exit)", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeGotoExit : IBlueprintNode, IBlueprintInternalLink, IBlueprintHashLink {

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

        public bool TryGetLinkedPort(NodeId id, out int hash, out int port) {
            hash = string.IsNullOrWhiteSpace(_label) ? 0 : _label.GetHashCode();
            port = 1;
            return true;
        }
    }

}
