using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceIterate :
        BlueprintSource<BlueprintNodeIterate2>,
        BlueprintSources.IEnter<BlueprintNodeIterate2>,
        BlueprintSources.IOutput<BlueprintNodeIterate2>,
        BlueprintSources.IConnectionCallback<BlueprintNodeIterate2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Iterate", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public struct BlueprintNodeIterate2 :
        IBlueprintNode,
        IBlueprintEnter2,
        IBlueprintOutput2,
        IBlueprintConnectionCallback
    {
        private int _arrayPointer;
        private Array _arrayCache;
        private LinkIterator _links;
        private IBlueprint _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            Type inputDataType = null;
            Type outputDataType = null;

            if (meta.TryGetLinksTo(id, 3, out int l)) {
                var link = meta.GetLink(l);
                var linkedPort = meta.GetPort(link.id, link.port);

                inputDataType = linkedPort.DataType;
                outputDataType = linkedPort.DataType;
            }
            else {
                for (meta.TryGetLinksFrom(id, 1, out l); l >= 0; meta.TryGetNextLink(l, out l)) {
                    var link = meta.GetLink(l);
                    var linkedPort = meta.GetPort(link.id, link.port);

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

            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.DynamicInput("Elements", inputDataType).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Exit("On Iteration"));
            meta.AddPort(id, Port.DynamicOutput(outputDataType == null ? "Element" : null, outputDataType));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = null;
            _arrayCache = null;
            _links = default;
        }

        public void OnEnterPort(NodeToken token, int port) {
            if (port != 0) return;

            _links = _blueprint.GetLinks(token, 1);

            // No linked ports: nothing to iterate.
            if (!_links.MoveNext()) return;

            // First link has array data type, prepare to read array.
            if (_links.Read<Array>() is { Length: > 0 } a) {
                _arrayPointer = 0;
                _arrayCache = a;
            }
            else {
                _arrayPointer = 0;
                _arrayCache = null;
            }

            // Iterate through array and element links.
            while (true) {
                // "On Iteration" port call.
                // Connected node must read "Element" output port when the port is called,
                // so method GetPortValue is called to retrieve element value.
                _blueprint.Call(token, 2);

                // Already retrieved array, has next element, continue iterate through array.
                if (_arrayCache != null && _arrayPointer + 1 < _arrayCache.Length) {
                    _arrayPointer++;
                    continue;
                }

                // Array is not retrieved or totally consumed: reset cached array.
                _arrayCache = null;
                _arrayPointer = 0;

                // No linked ports left: stop iteration.
                if (!_links.MoveNext()) break;

                // Next link has array data type, prepare to read array.
                if (_links.Read<Array>() is { Length: > 0 } array) {
                    _arrayPointer = 0;
                    _arrayCache = array;
                }
            }
        }

        public T GetPortValue<T>(NodeToken token, int port) {
            // Has cached array: return current array element
            if (_arrayCache is T[] array) return array[_arrayPointer];

            // Return current linked port value
            return _links.Read<T>();
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port is 1 or 3) meta.InvalidateNode(id, invalidateLinks: false);
        }
    }

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
