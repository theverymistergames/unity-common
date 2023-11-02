using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace Core {

    [Serializable]
    public class BlueprintSourceGoto :
        BlueprintSource<BlueprintNodeGoto2>,
        BlueprintSources.IInternalLink<BlueprintNodeGoto2>,
        BlueprintSources.IHashLink<BlueprintNodeGoto2>,
        BlueprintSources.ICloneable { }

    [Serializable]
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

}
