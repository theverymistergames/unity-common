using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Collisions.Triggers;
using UnityEngine;

namespace MisterGames.BlueprintLib {

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
