using System;
using MisterGames.BlueprintLib.Fsm;
using MisterGames.Blueprints;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState : BlueprintNode, IBlueprintEnter, IBlueprintFsmTransitionCallback {

        private bool _isEnteredState;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Input<IBlueprintFsmTransition>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
        };

        public override void OnDeInitialize() {
            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IBlueprintFsmTransition>()?.Disarm();
            }
        }

        public void OnEnterPort(int port) {
            if (port != 0 || _isEnteredState) return;

            _isEnteredState = true;

            Ports[2].Call();

            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IBlueprintFsmTransition>()?.Arm(this);
            }
        }

        public void OnTransitionRequested() {
            var links = Ports[1].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<IBlueprintFsmTransition>()?.Disarm();
            }

            Ports[3].Call();

            _isEnteredState = false;
        }
    }

}
