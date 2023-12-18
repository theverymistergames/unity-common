using System;
using MisterGames.BlueprintLib;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.Scenario.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Wait Scenario Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeWaitScenarioEvent : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<int> {

        [SerializeField] private ScenarioEvent _events;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Subscribe"));
            meta.AddPort(id, Port.Enter("Unsubscribe"));
            meta.AddPort(id, Port.Input<ScenarioEvent>());
            meta.AddPort(id, Port.Exit("On Emit"));
            meta.AddPort(id, Port.Output<int>("Emit Count"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;

            if (port == 0) {
                var evt = blueprint.Read(token, 2, _events);
                evt.OnEmit -= OnEmit;
                evt.OnEmit += OnEmit;
                return;
            }

            if (port == 1) {
                var evt = blueprint.Read(token, 2, _events);
                evt.OnEmit -= OnEmit;
                return;
            }
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 4) return default;

            var evt = blueprint.Read(token, 2, _events);
            return evt.EmitCount;
        }

        private void OnEmit() {
            _blueprint.Call(_token, 3);
        }
    }

}
