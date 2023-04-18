using System;
using MisterGames.Blueprints;
using MisterGames.Common.Conditions;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState : BlueprintNode, IBlueprintEnter, IConditionCallback {

        private bool _isEnteredState;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Enter("Exit"),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
            Port.Input<ICondition>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
        };

        public override void OnDeInitialize() {
            TryExit(notify: false);
        }

        public void OnEnterPort(int port) {
            switch (port) {
                case 0:
                    TryEnter();
                    break;

                case 1:
                    TryExit();
                    break;
            }
        }

        private void TryEnter(bool notify = true) {
            if (_isEnteredState) return;

            _isEnteredState = true;

            if (notify) Ports[2].Call();

            var links = Ports[4].links;
            for (int i = 0; i < links.Count; i++) {
                var condition = links[i].Get<ICondition>();
                if (condition == null) continue;

                condition.Arm(this);
                if (condition.IsMatched) return;
            }
        }

        private bool TryExit(bool notify = true) {
            if (!_isEnteredState) return false;

            var links = Ports[4].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<ICondition>()?.Disarm();
            }

            _isEnteredState = false;

            if (notify) Ports[3].Call();

            return true;
        }

        public void OnConditionMatch(ICondition match) {
            if (TryExit()) match.OnFired();
        }
    }

}
