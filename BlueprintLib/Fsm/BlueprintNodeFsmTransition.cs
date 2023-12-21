using System;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Fsm Transition 2", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition2 :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput<IBlueprintFsmTransition>,
        IBlueprintFsmTransition
    {
        [SerializeField] private bool _checkImmediatelyAfterArmed;
        [SerializeField] private bool _defaultCondition;

        private IBlueprintFsmState _state;
        private IBlueprint _blueprint;
        private NodeToken _token;
        private bool _isArmed;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IBlueprintFsmTransition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Enter("Check"));
            meta.AddPort(id, Port.Input<bool>("Condition"));
            meta.AddPort(id, Port.Exit("On Enter"));
            meta.AddPort(id, Port.Exit("On Exit"));
            meta.AddPort(id, Port.Exit("On Transit"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = blueprint;
            _token = token;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _state = null;
            _blueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;

            if (port == 1) {
                CheckConditionAndNotify();
                return;
            }
        }

        public IBlueprintFsmTransition GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        public bool Arm(IBlueprintFsmState state) {
            if (_isArmed) return false;

            _isArmed = true;

            _state = state;
            _blueprint?.Call(_token, 3);

            return _checkImmediatelyAfterArmed && CheckConditionAndNotify();
        }

        public void Disarm() {
            _state = null;

            if (!_isArmed) return;

            _isArmed = false;
            _blueprint?.Call(_token, 4);
        }

        private bool CheckConditionAndNotify() {
            if (!_isArmed) return false;

            bool condition = _blueprint.Read(_token, 2, _defaultCondition);
            if (!condition || !_state.TryTransit(this)) return false;

            _blueprint.Call(_token, 5);
            return true;
        }
    }

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        IBlueprintNode,
        IBlueprintOutput<ITransition>,
        ITransition,
        ITransitionCallback,
        IBlueprintFsmTransitionCallbacks
    {
        [SerializeField] private bool _checkImmediatelyAfterArmed;
        [SerializeReference] [SubclassSelector] private ITransition _transition;

        public bool IsMatched => _transition.IsMatched;

        private bool _isConditionArmed;
        private ITransitionCallback _stateCallback;

        private NodeToken _token;
        private IBlueprint _blueprint;
        private BlueprintPortDependencyResolver _dependencies;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<ITransition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single));
            meta.AddPort(id, Port.Exit("On Transit"));

            if (_transition is not IDependency dep) return;

            _dependencies ??= new BlueprintPortDependencyResolver();
            _dependencies.Reset();

            dep.OnSetupDependencies(_dependencies);
            for (int i = 0; i < _dependencies.Count; i++) {
                meta.AddPort(id, Port.DynamicInput(type: _dependencies[i]));
            }
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _dependencies = null;

            if (_stateCallback != null) _transition?.Disarm();
            _stateCallback = null;
        }

        public ITransition GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 0 ? this : default;
        }

        public void Arm(ITransitionCallback callback) {
            if (_stateCallback != null) return;

            if (_transition is IDependency dep) {
                _dependencies ??= new BlueprintPortDependencyResolver();
                _dependencies.Setup(_blueprint, _token, 2);

                dep.OnResolveDependencies(_dependencies);
            }

            _stateCallback = callback;

            if (!_isConditionArmed) {
                _transition?.Arm(this);
                _isConditionArmed = true;
            }

            if (_checkImmediatelyAfterArmed && IsMatched) _stateCallback.OnTransitionMatch(this);
        }

        public void Disarm() {
            if (_isConditionArmed) {
                _transition?.Disarm();
                _isConditionArmed = false;
            }

            _stateCallback = null;
        }

        public void OnTransitionMatch(ITransition match) {
            _stateCallback?.OnTransitionMatch(this);
        }

        public void OnTransitionFired() {
            _blueprint.Call(_token, 1);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
