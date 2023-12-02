using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceEnter :
        BlueprintSource<BlueprintNodeEnter>,
        BlueprintSources.IInternalLink<BlueprintNodeEnter>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Enter", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeEnter : IBlueprintNode, IBlueprintInternalLink {

        [SerializeField] private string _port;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Enter(_port).External(true).Hide(true));
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

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

}
