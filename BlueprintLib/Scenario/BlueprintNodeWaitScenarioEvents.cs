using System;
using MisterGames.BlueprintLib;
using MisterGames.Blueprints;
using MisterGames.Scenario.Core;
using UnityEngine;

namespace MisterGames.Scenario.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Wait Scenario Events", Category = "Scenario", Color = BlueprintLibColors.Node.Scenario)]
    public sealed class BlueprintNodeWaitScenarioEvents : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private ScenarioEvent[] _events;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit(),
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
            CallPort(1);
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
