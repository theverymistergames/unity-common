using System;
using MisterGames.BlueprintLib;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.Scenario.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Wait Scenario Events", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeWaitScenarioEvents2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private ScenarioEvent[] _events;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Exit("On Emit All"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            UnsubscribeAll();
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _token = token;

            if (AllEventsDone()) {
                Finish();
                return;
            }

            SubscribeToNotEmittedEvents();
        }

        private void SubscribeToNotEmittedEvents() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                if (evt.IsEmitted) continue;

                evt.OnEmit -= CheckFinish;
                evt.OnEmit += CheckFinish;
            }
        }

        private void UnsubscribeAll() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                evt.OnEmit -= CheckFinish;
            }
        }

        private void CheckFinish() {
            if (AllEventsDone()) Finish();
        }

        private void Finish() {
            UnsubscribeAll();
            _blueprint.Call(_token, 1);
        }

        private bool AllEventsDone() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                if (!evt.IsEmitted) return false;
            }

            return true;
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Wait Scenario Events", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeWaitScenarioEvents : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private ScenarioEvent[] _events;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            Port.Exit("On Emit All"),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            if (AllEventsDone()) {
                Finish();
                return;
            }

            SubscribeToNotEmittedEvents();
        }

        private void SubscribeToNotEmittedEvents() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                if (evt.IsEmitted) continue;
                
                evt.OnEmit -= CheckFinish;
                evt.OnEmit += CheckFinish;
            }
        }

        private void UnsubscribeAll() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                evt.OnEmit -= CheckFinish;
            }
        }

        private void CheckFinish() {
            if (AllEventsDone()) Finish();
        }

        private void Finish() {
            UnsubscribeAll();
            Ports[1].Call();
        }

        private bool AllEventsDone() {
            for (int i = 0; i < _events.Length; i++) {
                var evt = _events[i];
                if (!evt.IsEmitted) return false;
            }

            return true;
        }
    }

}
