using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Iterate", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeIterate : BlueprintNode, IBlueprintEnter, IBlueprintOutput

#if UNITY_EDITOR
        , IBlueprintPortDecorator
        , IBlueprintPortLinksListener
#endif

    {
        private int _index;
        private int _subIndex;
        private Array _arrayCache;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            Port.DynamicInput("Elements").Capacity(PortCapacity.Multiple),
            Port.Exit("On Iteration"),
            Port.DynamicOutput("Element"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            _index = 0;
            _subIndex = 0;
            _arrayCache = null;

            while (MoveNext()) {
                Ports[2].Call();
            }
        }

        public R GetOutputPortValue<R>(int port) {
            if (_arrayCache is R[] array && _subIndex < array.Length) return array[_subIndex++];

            var links = Ports[1].links;
            return _index < links.Count ? links[_index++].Get<R>() : default;
        }

        private bool MoveNext() {
            var links = Ports[1].links;

            while (true) {
                if (_index > links.Count - 1) {
                    _arrayCache = null;
                    return false;
                }

                if (_arrayCache != null) {
                    if (_subIndex < _arrayCache.Length) return true;

                    _index++;
                    _subIndex = 0;
                    _arrayCache = null;
                    continue;
                }

                var link = links[_index];

                if (link.node is IBlueprintOutput<Array> outputRs) {
                    if (outputRs.GetOutputPortValue(link.port) is {} array && _subIndex < array.Length) {
                        _arrayCache = array;
                        return true;
                    }

                    _index++;
                    _subIndex = 0;
                    _arrayCache = null;
                    continue;
                }

                if (link.node is IBlueprintOutput output &&
                    output.GetOutputPortValue<Array>(link.port) is {} dynamicArray
                ) {
                    if (_subIndex < dynamicArray.Length) {
                        _arrayCache = dynamicArray;
                        return true;
                    }

                    _index++;
                    _subIndex = 0;
                    _arrayCache = null;
                    continue;
                }

                _subIndex = 0;
                _arrayCache = null;
                return true;
            }
        }

#if UNITY_EDITOR
        public void DecoratePorts(BlueprintAsset blueprint, int nodeId, Port[] ports) {
            var outputLinks = blueprint.BlueprintMeta.GetLinksToNodePort(nodeId, 3);

            Type inputDataType = null;
            Type outputDataType = null;

            if (outputLinks.Count == 0) {
                var elementsLinks = blueprint.BlueprintMeta.GetLinksFromNodePort(nodeId, 1);
                for (int i = 0; i < elementsLinks.Count; i++) {
                    var link = elementsLinks[i];
                    var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

                    if (linkedPort.DataType is { IsArray: false }) {
                        inputDataType = linkedPort.DataType;
                        outputDataType = linkedPort.DataType;
                        break;
                    }

                    if (linkedPort.DataType is { IsArray: true }) {
                        inputDataType = linkedPort.DataType.GetElementType();
                        outputDataType = null;
                    }
                }
            }
            else {
                var link = outputLinks[0];
                var linkedPort = blueprint.BlueprintMeta.NodesMap[link.nodeId].Ports[link.portIndex];

                inputDataType = linkedPort.DataType;
                outputDataType = linkedPort.DataType;
            }

            ports[1] = Port.DynamicInput("Elements", inputDataType).Capacity(PortCapacity.Multiple);
            ports[3] = Port.DynamicOutput(outputDataType == null ? "Element" : null, outputDataType);
        }

        public void OnPortLinksChanged(BlueprintAsset blueprint, int nodeId, int portIndex) {
            if (portIndex is 1 or 3) blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: false);
        }
#endif
    }

}
