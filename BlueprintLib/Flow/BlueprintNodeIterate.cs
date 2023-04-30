using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
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
        private int _iterationLinkIndex;
        private int _iterationLinkSubIndex;

        private readonly struct CallPortOnDispose : IDisposable {

            private readonly RuntimePort _port;
            private readonly bool _isAllowed;

            public CallPortOnDispose(RuntimePort port, bool isAllowed) {
                _port = port;
                _isAllowed = isAllowed;
            }

            public void Dispose() {
                if (_isAllowed) _port.Call();
            }
        }

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            Port.DynamicInput("Elements").Capacity(PortCapacity.Multiple),
            Port.Exit("On Iteration"),
            Port.DynamicOutput("Element"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var links = Ports[1].links;
            if (links.Count == 0) return;

            _iterationLinkIndex = 0;
            _iterationLinkSubIndex = 0;

            Ports[2].Call();
        }

        public R GetOutputPortValue<R>(int port) {
            var links = Ports[1].links;

            while (true) {
                if (_iterationLinkIndex > links.Count - 1) return default;

                var link = links[_iterationLinkIndex];

                if (link.node is IBlueprintOutput<R[]> outputRs && outputRs.GetOutputPortValue(link.port) is {} rs) {
                    if (_iterationLinkSubIndex > rs.Length - 1) {
                        _iterationLinkIndex++;
                        _iterationLinkSubIndex = 0;
                        continue;
                    }

                    return ReturnNextArrayElementAndContinue(rs);
                }

                if (link.node is IBlueprintOutput<R> outputR) {
                    return ReturnNextElementAndContinue(outputR.GetOutputPortValue(link.port));
                }

                if (link.node is IBlueprintOutput output) {
                    if (output.GetOutputPortValue<R[]>(link.port) is {} dynamicRs) {
                        if (_iterationLinkSubIndex > dynamicRs.Length - 1) {
                            _iterationLinkIndex++;
                            _iterationLinkSubIndex = 0;
                            continue;
                        }

                        return ReturnNextArrayElementAndContinue(dynamicRs);
                    }

                    return ReturnNextElementAndContinue(output.GetOutputPortValue<R>(link.port));
                }

                _iterationLinkIndex++;
                _iterationLinkSubIndex = 0;
            }
        }

        private R ReturnNextElementAndContinue<R>(R element) {
            bool hasNext = HasNext<R>(
                nextIndex: _iterationLinkIndex + 1, nextSubIndex: 0,
                out _iterationLinkIndex, out _iterationLinkSubIndex
            );

            using var iterateCall = new CallPortOnDispose(Ports[2], hasNext);
            return element;
        }

        private R ReturnNextArrayElementAndContinue<R>(IReadOnlyList<R> elements) {
            var element = elements[_iterationLinkSubIndex];

            bool hasNext = HasNext<R>(
                nextIndex: _iterationLinkIndex, nextSubIndex: _iterationLinkSubIndex + 1,
                out _iterationLinkIndex, out _iterationLinkSubIndex
            );

            using var iterateCall = new CallPortOnDispose(Ports[2], hasNext);
            return element;
        }

        private bool HasNext<R>(int nextIndex, int nextSubIndex, out int index, out int subIndex) {
            var links = Ports[1].links;

            index = nextIndex;
            subIndex = nextSubIndex;

            while (true) {
                if (index > links.Count - 1) return false;

                var link = links[index];

                if (link.node is IBlueprintOutput<R[]> outputRs && outputRs.GetOutputPortValue(link.port) is {} rs) {
                    if (subIndex > rs.Length - 1) {
                        index++;
                        subIndex = 0;
                        continue;
                    }

                    return true;
                }

                if (link.node is IBlueprintOutput<R>) {
                    subIndex = 0;
                    return true;
                }

                if (link.node is IBlueprintOutput output) {
                    if (output.GetOutputPortValue<R[]>(link.port) is {} dynamicRs) {
                        if (subIndex > dynamicRs.Length - 1) {
                            index++;
                            subIndex = 0;
                            continue;
                        }

                        return true;
                    }

                    subIndex = 0;
                    return true;
                }

                index++;
                subIndex = 0;
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
