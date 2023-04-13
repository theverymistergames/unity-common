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
            Port.Input<ICondition>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
        };

        public override void OnDeInitialize() {
            _isEnteredState = false;

            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<ICondition>()?.Disarm();
            }
        }

        public void OnEnterPort(int port) {
            if (port != 0 || _isEnteredState) return;

            _isEnteredState = true;

            Ports[2].Call();

            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<ICondition>()?.Arm(this);
            }
        }

        public void OnConditionMatch() {
            if (!_isEnteredState) return;

            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<ICondition>()?.Disarm();
            }

            Ports[3].Call();

            _isEnteredState = false;
        }
    }

}
