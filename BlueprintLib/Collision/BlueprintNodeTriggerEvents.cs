using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Nodes;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Trigger Events", Category = "Collision", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeTriggerEvents :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintStartCallback,
        IBlueprintOutput<GameObject>,
        IBlueprintOutput<Collider>
    {
        [SerializeField] private bool _autoSetTriggerAtStart;

        private Trigger _trigger;
        private Collider _collider;
        private IBlueprint _blueprint;

        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Trigger"));
            meta.AddPort(id, Port.Input<Trigger>());
            meta.AddPort(id, Port.Exit("On Triggered"));
            meta.AddPort(id, Port.Output<GameObject>());
            meta.AddPort(id, Port.Output<Collider>());
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _collider = null;
            _trigger = null;
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            _blueprint = blueprint;

            if (!_autoSetTriggerAtStart) return;

            _token = token;
            SetupTrigger();
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _token = token;
            SetupTrigger();
        }

        GameObject IBlueprintOutput<GameObject>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 3 ? _collider.gameObject : default;
        }

        Collider IBlueprintOutput<Collider>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 4 ? _collider : default;
        }

        private void SetupTrigger() {
            if (_trigger != null) _trigger.OnTriggered -= OnTriggered;

            _trigger = _blueprint.Read<Trigger>(_token, 1);
            _trigger.OnTriggered += OnTriggered;
        }

        private void OnTriggered(Collider go) {
            _collider = go;
            _blueprint.Call(_token, 2);
        }
    }

}
