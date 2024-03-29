﻿using System;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceExit :
        BlueprintSource<BlueprintNodeExit>,
        BlueprintSources.IInternalLink<BlueprintNodeExit>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Exit", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeExit : IBlueprintNode, IBlueprintInternalLink {

        [SerializeField] private string _port;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Exit(_port).External(true).Hide(true));
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

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: false, notify: false);
        }
    }

}
