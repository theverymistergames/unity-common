using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Nodes;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Trigger Events", Category = "Collision", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeTriggerEvents2 :
        IBlueprintNode,
        IBlueprintEnter2,
        IBlueprintStartCallback,
        IBlueprintOutput2<GameObject>
    {
        [SerializeField] private bool _autoSetTriggerAtStart;

        private Trigger _trigger;
        private GameObject _go;
        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Set Trigger"));
            meta.AddPort(id, Port.Input<Trigger>());
            meta.AddPort(id, Port.Exit("On Triggered"));
            meta.AddPort(id, Port.Output<GameObject>());
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            if (!_autoSetTriggerAtStart) return;

            _token = token;
            _blueprint = blueprint;
            _trigger = blueprint.Read<Trigger>(token, 1);

            _trigger.OnTriggered -= OnTriggered;
            _trigger.OnTriggered += OnTriggered;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0)  return;

            _token = token;
            _blueprint = blueprint;
            _trigger = blueprint.Read<Trigger>(token, 1);

            _trigger.OnTriggered -= OnTriggered;
            _trigger.OnTriggered += OnTriggered;
        }

        public GameObject GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 3 ? _go : default;
        }

        private void OnTriggered(GameObject go) {
            _go = go;
            _blueprint.Call(_token, 2);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Trigger Events", Category = "Collision", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeTriggerEvents : BlueprintNode, IBlueprintEnter, IBlueprintStart, IBlueprintOutput<GameObject> {

        [SerializeField] private bool _autoSetTriggerAtStart;

        private Trigger _trigger;
        private GameObject _go;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Set Trigger"),
            Port.Input<Trigger>(),
            Port.Exit("On Triggered"),
            Port.Output<GameObject>(),
        };

        public void OnStart() {
            if (!_autoSetTriggerAtStart) return;

            _trigger = Ports[1].Get<Trigger>();

            _trigger.OnTriggered -= OnTriggered;
            _trigger.OnTriggered += OnTriggered;
        }

        public void OnEnterPort(int port) {
            if (port != 0)  return;
            
            _trigger = Ports[1].Get<Trigger>();

            _trigger.OnTriggered -= OnTriggered;
            _trigger.OnTriggered += OnTriggered;
        }

        public GameObject GetOutputPortValue(int port) {
            return port == 3 ? _go : default;
        }

        private void OnTriggered(GameObject go) {
            _go = go;
            Ports[2].Call();
        }
    }

}
