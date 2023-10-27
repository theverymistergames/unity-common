using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public class BlueprintSourceInput :
        BlueprintSource<BlueprintNodeInput2>,
        BlueprintSources.IInternalLink<BlueprintNodeInput2>,
        BlueprintSources.IConnectionCallback<BlueprintNodeInput2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Input", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeInput2 : IBlueprintNode, IBlueprintInternalLink, IBlueprintConnectionCallback, IBlueprintCloneable {

        [SerializeField] private string _port;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;

            if (meta.TryGetLinksTo(id, 0, out int i)) {
                var link = meta.GetLink(i);
                var linkedPort = meta.GetPort(link.id, link.port);

                dataType = linkedPort.DataType;
            }

            meta.AddPort(id, Port.DynamicOutput(type: dataType));
            meta.AddPort(id, Port.DynamicInput(_port, type: dataType).External(true).Hide(true));
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
