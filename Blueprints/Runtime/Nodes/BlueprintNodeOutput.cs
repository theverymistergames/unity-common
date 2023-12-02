using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceOutput :
        BlueprintSource<BlueprintNodeOutput>,
        BlueprintSources.IInternalLink<BlueprintNodeOutput>,
        BlueprintSources.IConnectionCallback<BlueprintNodeOutput>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Output", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeOutput : IBlueprintNode, IBlueprintInternalLink, IBlueprintConnectionCallback {

        [SerializeField] private string _port;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;

            if (meta.TryGetLinksFrom(id, 0, out int i)) {
                var link = meta.GetLink(i);
                var linkedPort = meta.GetPort(link.id, link.port);

                dataType = linkedPort.DataType;
            }

            meta.AddPort(id, Port.DynamicInput(type: dataType));
            meta.AddPort(id, Port.DynamicOutput(_port, type: dataType).External(true).Hide(true));
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

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port == 0) meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

}
