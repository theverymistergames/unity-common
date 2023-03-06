using System;
using MisterGames.Blueprints;
using MisterGames.Fsm.Core;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState : BlueprintNode, IBlueprintEnter, IFsmTransitionCallback {

        private IFsmTransition[] _transitions;
        private bool _isEnteredState;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
            Port.InputArray<IFsmTransition>("Transitions")
        };

        public void OnEnterPort(int port) {
            if (port != 0 || _isEnteredState) return;

            _isEnteredState = true;

            CallExitPort(1);

            _transitions = ReadInputArrayPort(3, Array.Empty<IFsmTransition>());
            for (int i = 0; i < _transitions.Length; i++) {
                _transitions[i].Arm(this);
            }
        }

        public void OnTransitionRequested() {
            for (int i = 0; i < _transitions.Length; i++) {
                _transitions[i].Disarm();
            }

            CallExitPort(2);

            _isEnteredState = false;
            _transitions = null;
        }
    }

}
