using System;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<IBlueprintFsmTransition>,
        IBlueprintFsmTransition
    {
        [SerializeField] private bool _checkImmediatelyAfterArmed;
        [SerializeField] private bool _defaultCondition;

        private IBlueprintFsmState _state;
        private IBlueprint _blueprint;
        private NodeToken _token;
        private bool _isArmed;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IBlueprintFsmTransition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Enter("Check"));
            meta.AddPort(id, Port.Input<bool>("Condition"));
            meta.AddPort(id, Port.Exit("On Enter"));
            meta.AddPort(id, Port.Exit("On Exit"));
            meta.AddPort(id, Port.Exit("On Transit"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _state = null;
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;

            if (port == 1) {
                CheckConditionAndNotify();
                return;
            }
        }

        public IBlueprintFsmTransition GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        public bool Arm(IBlueprintFsmState state) {
            if (_isArmed) return false;

            _isArmed = true;

            _state = state;
            _blueprint?.Call(_token, 3);

            return _checkImmediatelyAfterArmed && CheckConditionAndNotify();
        }

        public void Disarm() {
            _state = null;

            if (!_isArmed) return;

            _isArmed = false;
            _blueprint?.Call(_token, 4);
        }

        private bool CheckConditionAndNotify() {
            if (!_isArmed) return false;

            bool condition = _blueprint.Read(_token, 2, _defaultCondition);
            if (!condition || !_state.TryTransit(this)) return false;

            _blueprint.Call(_token, 5);
            return true;
        }
    }

}
