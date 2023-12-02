using System;
using MisterGames.Blueprints;
using MisterGames.Common.Conditions;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Fsm State", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmState : IBlueprintNode, IBlueprintEnter, ITransitionCallback {

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

}
