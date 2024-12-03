using System;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Event", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeEvent : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<int>, IEventListener {

        [HideLabel]
        [SerializeField] private EventReference _event;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Raise"));
            meta.AddPort(id, Port.Exit("On Raised"));
            meta.AddPort(id, Port.Output<int>("Raise Count"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
            _event.Subscribe(this);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _event.Unsubscribe(this);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            if (port == 0) _event.Raise();
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 2 ? _event.GetCount() : default;
        }

        public void OnEventRaised(EventReference e) {
            _blueprint.Call(_token, 1);
        }
    }

}
