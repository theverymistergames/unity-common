using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {
    
    [Serializable]
    [BlueprintNode(Name = "Counter", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeCounter : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<int> {

        [SerializeField] private int _value;
        [SerializeField] private Optional<int> _lowerBound = Optional<int>.Create(0);
        [SerializeField] private Optional<int> _upperBound;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Add"));
            meta.AddPort(id, Port.Enter("Set"));
            meta.AddPort(id, Port.Enter("Reset"));
            meta.AddPort(id, Port.Input<int>());
            meta.AddPort(id, Port.Exit("On Changed"));
            meta.AddPort(id, Port.Output<int>());
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _value = ClampValue(_value);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            int value = _value;
            
            _value = port switch {
                0 => ClampValue(blueprint.Read(token, 3, 0)),
                1 => ClampValue(_value + blueprint.Read(token, 3, 1)),
                2 => ClampValue(0),
                _ => _value
            };
            
            if (value != _value) blueprint.Call(token, 4);
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 5 ? _value : default;
        }

        private int ClampValue(int value) {
            if (_lowerBound.HasValue) value = Math.Max(_lowerBound.Value, value);
            if (_upperBound.HasValue) value = Math.Min(_upperBound.Value, value);
            
            return value;
        }
    }

}
