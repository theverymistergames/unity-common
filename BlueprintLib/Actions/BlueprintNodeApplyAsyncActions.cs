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
    public class BlueprintSourceApplyAsyncAction :
        BlueprintSource<BlueprintNodeApplyAsyncAction>,
        BlueprintSources.IEnter<BlueprintNodeApplyAsyncAction>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Apply Async Action", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeApplyAsyncAction : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private AsyncActionAsset _action;

        private CancellationTokenSource _terminateCts;
        private BlueprintPortDependencyResolver _dependencies;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Apply"));
            meta.AddPort(id, Port.Exit("On Applied"));

            if (_action is not IDependency dep) return;

            _dependencies ??= new BlueprintPortDependencyResolver();
            _dependencies.Reset();

            dep.OnSetupDependencies(_dependencies);
            for (int i = 0; i < _dependencies.Count; i++) {
                meta.AddPort(id, Port.DynamicInput(type: _dependencies[i]));
            }
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _dependencies = null;

            _terminateCts?.Cancel();
            _terminateCts?.Dispose();
            _terminateCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            Apply(blueprint, token, _terminateCts.Token).Forget();
        }

        private async UniTaskVoid Apply(IBlueprint blueprint, NodeToken token, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) return;

            if (_action is IDependency dep) {
                _dependencies ??= new BlueprintPortDependencyResolver();
                _dependencies.Setup(blueprint, token, 2);

                dep.OnResolveDependencies(_dependencies);
            }

            await _action.Apply(source: blueprint, cancellationToken);

            if (cancellationToken.IsCancellationRequested) return;

            blueprint.Call(token, 1);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
