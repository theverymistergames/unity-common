using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Blueprints;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Apply Async Actions", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeApplyAsyncActions :
        BlueprintNode,
        IBlueprintEnter,
        IDependencyResolver,
        IDependencyContainer,
        IDependencyBucket
    {
        [SerializeField] private AsyncActionAsset[] _applyActions;
        [SerializeField] private AsyncActionAsset[] _releaseActions;

        private CancellationTokenSource _terminateCts;

        private readonly List<Type> _dependencies = new List<Type>();
        private int _dependencyPortIterator;

        public override Port[] CreatePorts() {
            var ports = new List<Port> {
                Port.Enter("Apply"),
                Port.Enter("Release"),
                Port.Exit("On Applied"),
                Port.Exit("On Released"),
            };

            _dependencies.Clear();

            if (_applyActions == null || _releaseActions == null) return ports.ToArray();

            for (int i = 0; i < _applyActions.Length; i++) {
                if (_applyActions[i] is IDependency dep) dep.OnSetupDependencies(this);
            }

            for (int i = 0; i < _releaseActions.Length; i++) {
                if (_releaseActions[i] is IDependency dep) dep.OnSetupDependencies(this);
            }

            for (int i = 0; i < _dependencies.Count; i++) {
                ports.Add(Port.DynamicInput(type: _dependencies[i]));
            }

            return ports.ToArray();
        }

        public override void OnInitialize(IBlueprintHost host) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = null;
        }

        public IDependencyBucket CreateBucket(object source) {
            return this;
        }

        public IDependencyBucket Add<T>() where T : class {
            _dependencies.Add(typeof(T));
            return this;
        }

        public T Resolve<T>() where T : class {
            return Ports[_dependencyPortIterator++].Get<T>();
        }

        public async void OnEnterPort(int port) {
            switch (port) {
                case 0: {
                    _dependencyPortIterator = 4;

                    for (int i = 0; i < _applyActions.Length; i++) {
                        var action = _applyActions[i];
                        if (action is IDependency dep) dep.OnResolveDependencies(this);

                        await action.Apply(this, _terminateCts.Token);
                    }

                    Ports[2].Call();

                    break;
                }

                case 1: {
                    _dependencyPortIterator = 4;

                    for (int i = 0; i < _releaseActions.Length; i++) {
                        var action = _releaseActions[i];
                        if (action is IDependency dep) dep.OnResolveDependencies(this);

                        await action.Apply(this, _terminateCts.Token);
                    }

                    Ports[3].Call();

                    break;
                }
            }
        }
    }

}
