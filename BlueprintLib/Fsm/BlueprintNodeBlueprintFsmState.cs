using System;
using MisterGames.Blueprints;
using MisterGames.Fsm.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeBlueprintFsmState : BlueprintNode, IBlueprintEnter, IFsmTransitionCallback {

        private bool _isEnteredState;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
            Port.Input<IFsmTransitionBase>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple)
        };

        public void OnEnterPort(int port) {
            if (port != 0 || _isEnteredState) return;

            _isEnteredState = true;

            Ports[1].Call();

            var links = Ports[3].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IFsmTransitionBase>()?.Arm(this);
            }
        }

        public void OnTransitionRequested() {
            var links = Ports[3].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IFsmTransitionBase>()?.Disarm();
            }

            Ports[2].Call();

            _isEnteredState = false;
        }
    }

}
