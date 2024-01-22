using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourcePipeData :
        BlueprintSource<BlueprintNodePipeData>,
        BlueprintSources.IInternalLink<BlueprintNodePipeData>,
        BlueprintSources.IConnectionCallback<BlueprintNodePipeData>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Pipe (data)", Category = "Flow", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodePipeData : IBlueprintNode, IBlueprintInternalLink, IBlueprintConnectionCallback {

        [SerializeField] private LayoutOptions _layout;

        private enum LayoutOptions {
            Default,
            Inverted,
            Left,
            LeftInverted,
            Right,
            RightInverted,
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type dataType = null;
            var outCapacity = PortCapacity.Default;

            int inputIndex = 0;
            int outputIndex = 1;

            if (_layout is LayoutOptions.LeftInverted or LayoutOptions.RightInverted) {
                inputIndex = 1;
                outputIndex = 0;
            }

            if (meta.TryGetLinksFrom(id, inputIndex, out int l) ||
                meta.TryGetLinksTo(id, outputIndex, out l)
            ) {
                var link = meta.GetLink(l);
                dataType = meta.GetPort(link.id, link.port).DataType;
            }

            if (meta.TryGetLinksFrom(id, inputIndex, out l)) {
                var link = meta.GetLink(l);
                if (!meta.GetPort(link.id, link.port).IsMultiple()) outCapacity = PortCapacity.Single;
            }

            var input = Port.DynamicInput(dataType == null ? "In" : null, dataType).Layout(GetLayout(true, _layout));
            var output = Port.DynamicOutput("Out", dataType).Capacity(outCapacity).Layout(GetLayout(false, _layout));

            if (_layout is LayoutOptions.LeftInverted or LayoutOptions.RightInverted) {
                meta.AddPort(id, output);
                meta.AddPort(id, input);
            }
            else {
                meta.AddPort(id, input);
                meta.AddPort(id, output);
            }
        }

        private static PortLayout GetLayout(bool input, LayoutOptions options) {
            if (input) {
                return options is LayoutOptions.Inverted or LayoutOptions.Right or LayoutOptions.RightInverted
                    ? PortLayout.Right
                    : PortLayout.Left;
            }

            return options is LayoutOptions.Inverted or LayoutOptions.Left or LayoutOptions.LeftInverted
                ? PortLayout.Left
                : PortLayout.Right;
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (port == 0) {
                index = 1;
                count = 1;
                return;
            }

            if (port == 1) {
                index = 0;
                count = 1;
                return;
            }

            index = -1;
            count = 0;
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            meta.InvalidateNode(id, invalidateLinks: false);
        }
    }

}
