using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Iterate", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeIterate :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput,
        IBlueprintOutput<int>,
        IBlueprintConnectionCallback 
    {
        [SerializeField] private int _count;
        
        private int _arrayPointer;
        private Array _arrayCache;
        private LinkIterator _links;
        private int _index;
        private int _currentCount;

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
                    }
                }
            }

            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.DynamicInput("Elements", inputDataType).Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Exit("On Iteration"));
            meta.AddPort(id, Port.DynamicOutput(outputDataType == null ? "Element" : null, outputDataType));
            meta.AddPort(id, Port.Output<int>("Index"));
            meta.AddPort(id, Port.Input<int>("Count"));
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _arrayCache = null;
            _links = default;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _index = 0;
            _arrayPointer = 0;
            _links = blueprint.GetLinks(token, 1);
            _currentCount = blueprint.Read(token, 5, _count);

            bool hasLinks = _links.MoveNext();
            
            // No linked ports: nothing to iterate.
            if (!hasLinks && _currentCount < 0) return;

            // First link has array data type, prepare to read array.
            _arrayCache = hasLinks && _links.Read<Array>() is { Length: > 0 } a ? a : null;

            // Iterate through array and element links.
            while (_count < 0 || _index < _count) {
                // "On Iteration" port call.
                // Connected node must read "Element" output port when the port is called,
                // so method GetPortValue is called to retrieve element value.
                blueprint.Call(token, 2);
                
                // Already retrieved array, has next element, continue iterate through array.
                if (_arrayCache != null && _arrayPointer + 1 < _arrayCache.Length) {
                    _arrayPointer++;
                    _index++;
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

                _index++;
            }
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            // Has cached array: return current array element
            if (_arrayCache is T[] array) {
                return array[_arrayPointer];
            }

            // Has cached array with base type: return current array element
            if (_arrayCache != null) {
                return _arrayCache.GetValue(_arrayPointer) is T t ? t : default;
            }

            // Return current linked port value
            return _links.Read<T>();
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 4 ? _index : 0;
        }

        public void OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
            if (port is 1 or 3) meta.InvalidateNode(id, invalidateLinks: false);
        }
    }

}
