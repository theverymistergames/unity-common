using System;
using MisterGames.Blueprints;
using MisterGames.Common.Conditions;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState2 : IBlueprintNode, IBlueprintEnter2, ITransitionCallback {

        private IBlueprint _blueprint;
        private NodeToken _token;
        private bool _isEnteredState;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Enter"));
            meta.AddPort(id, Port.Enter("Exit"));
            meta.AddPort(id, Port.Exit("On Enter"));
            meta.AddPort(id, Port.Exit("On Exit"));
            meta.AddPort(id, Port.Input<ITransition>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple));
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            TryExit(notify: false);

            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _blueprint = blueprint;
            _token = token;

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

            if (notify) _blueprint.Call(_token, 2);

            var links = _blueprint.GetLinks(_token, 4);
            while (links.MoveNext()) {
                if (links.Read<ITransition>() is not {} transition) continue;

                transition.Arm(this);
                if (transition.IsMatched) return;
            }
        }

        private bool TryExit(bool notify = true) {
            if (!_isEnteredState) return false;

            var links = _blueprint.GetLinks(_token, 4);
            while (links.MoveNext()) {
                links.Read<ITransition>()?.Disarm();
            }

            _isEnteredState = false;

            if (notify) _blueprint.Call(_token, 3);

            return true;
        }

        public void OnTransitionMatch(ITransition match) {
            if (TryExit() && match is IBlueprintFsmTransitionCallbacks callbacks) callbacks.OnTransitionFired();
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState : BlueprintNode, IBlueprintEnter, ITransitionCallback {

        private bool _isEnteredState;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Enter"),
            Port.Enter("Exit"),
            Port.Exit("On Enter"),
            Port.Exit("On Exit"),
            Port.Input<ITransition>("Transitions").Layout(PortLayout.Right).Capacity(PortCapacity.Multiple),
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
                var transition = links[i].Get<ITransition>();
                if (transition == null) continue;

                transition.Arm(this);
                if (transition.IsMatched) return;
            }
        }

        private bool TryExit(bool notify = true) {
            if (!_isEnteredState) return false;

            var links = Ports[4].links;
            for (int i = 0; i < links.Count; i++) {
                links[i].Get<ITransition>()?.Disarm();
            }

            _isEnteredState = false;

            if (notify) Ports[3].Call();

            return true;
        }

        public void OnTransitionMatch(ITransition match) {
            if (TryExit() && match is IBlueprintFsmTransitionCallbacks callbacks) callbacks.OnTransitionFired();
        }
    }

}
