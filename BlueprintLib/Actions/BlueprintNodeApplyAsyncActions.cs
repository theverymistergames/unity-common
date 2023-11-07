using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Actions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceApplyAsyncActions :
        BlueprintSource<BlueprintNodeApplyAsyncActions2>,
        BlueprintSources.IEnter<BlueprintNodeApplyAsyncActions2> {}

    [Serializable]
    [BlueprintNode(Name = "Apply Async Actions", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeApplyAsyncActions2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] private AsyncActionAsset[] _actions;

        private CancellationTokenSource _terminateCts;
        private BlueprintPortDependencyResolver _dependencies;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Exit("On Applied"));

            _dependencies ??= new BlueprintPortDependencyResolver();
            _dependencies.Reset();

            if (_actions == null) return;

            for (int i = 0; i < _actions.Length; i++) {
                if (_actions[i] is IDependency dep) dep.OnSetupDependencies(_dependencies);
            }

            for (int i = 0; i < _dependencies.Count; i++) {
                meta.AddPort(id, Port.DynamicInput(type: _dependencies[i]));
            }
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            _dependencies ??= new BlueprintPortDependencyResolver();
            _dependencies.Setup(blueprint, token, 2);

            Apply(blueprint, token, _terminateCts.Token).Forget();
        }

        private async UniTaskVoid Apply(IBlueprint blueprint, NodeToken token, CancellationToken cancellationToken) {
            for (int i = 0; i < _actions.Length; i++) {
                if (cancellationToken.IsCancellationRequested) break;

                var action = _actions[i];
                if (action is IDependency dep) dep.OnResolveDependencies(_dependencies);

                await action.Apply(source: _actions, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) return;

            blueprint.Call(token, 1);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Apply Async Actions", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeApplyAsyncActions :
        BlueprintNode,
        IBlueprintEnter,
        IDependencyResolver,
        IDependencyContainer,
        IDependencyBucket,
        IBlueprintAssetValidator
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

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
    }

}
