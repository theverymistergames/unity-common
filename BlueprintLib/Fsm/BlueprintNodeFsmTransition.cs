using System;
using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNodeMeta(Name = "Fsm Transition", Category = "Fsm", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeFsmTransition :
        BlueprintNode,
        IBlueprintOutput<ITransition>,
        ITransition,
        ITransitionCallback,
        IBlueprintFsmTransitionCallbacks,
        IBlueprintAssetValidator,
        IDependencyResolver,
        IDependencyContainer
    {
        [SerializeField] private bool _checkImmediatelyAfterArmed;
        [SerializeReference] [SubclassSelector] private ITransition _transition;

        public bool IsMatched => _transition.IsMatched;

        private ITransitionCallback _stateCallback;
        private bool _isConditionArmed;

        private readonly List<Type> _dependencies = new List<Type>();
        private int _dependencyPortIterator;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Output<ITransition>("Self").Layout(PortLayout.Left).Capacity(PortCapacity.Single),
                Port.Exit("On Transit"),
            };

            if (_transition is IDependency dep) {
                _dependencies.Clear();
                dep.OnAddDependencies(this);

                for (int i = 0; i < _dependencies.Count; i++) {
                    ports.Add(Port.DynamicInput(type: _dependencies[i]));
                }

                _dependencies.Clear();
            }

            return ports.ToArray();
        }

        public override void OnDeInitialize() {
            if (_stateCallback == null) return;

            _transition?.Disarm();
            _stateCallback = null;
        }

        public ITransition GetOutputPortValue(int port) {
            return port == 0 ? this : default;
        }

        public void AddDependency<T>(object source) {
            _dependencies.Add(typeof(T));
        }

        public T ResolveDependency<T>() {
            return Ports[_dependencyPortIterator++].Get<T>();
        }

        public void Arm(ITransitionCallback callback) {
            if (_stateCallback != null) return;

            if (_transition is IDependency dep) {
                _dependencyPortIterator = 2;
                dep.OnResolveDependencies(this);
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
            Ports[1].Call();
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
    }

}
