using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Runtime;
using UnityEngine;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using MisterGames.Blueprints.Validation;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceExternalBlueprint :
        BlueprintSource<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IEnter<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IOutput<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICreateSignaturePorts<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "External Blueprint", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeExternalBlueprint :
        IBlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput,
        IBlueprintCreateSignaturePorts
    {
        [BlackboardProperty("_blackboard")]
        [SerializeField] private int _runner;
        [SerializeField] private BlueprintAsset _blueprint;

        private RuntimeBlueprint _externalBlueprint;

#if UNITY_EDITOR
        private bool _isValidExternalBlueprint;
#endif

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            PortExtensions.FetchExternalPorts(meta, id, _blueprint);
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            var runner = blueprint.GetBlackboard(root).Get<BlueprintRunner>(_runner);

#if UNITY_EDITOR
            _isValidExternalBlueprint = SubgraphValidator.ValidateExternalBlueprint(blueprint.Host, runner, _blueprint);
            if (!_isValidExternalBlueprint) return;
            runner.RegisterClient(blueprint.Host);
#endif

            _externalBlueprint = runner.GetOrCompileBlueprint();
            _externalBlueprint.Bind(token.node, token.caller, blueprint);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return;
            var runner = blueprint.GetBlackboard(root).Get<BlueprintRunner>(_runner);
            if (runner != null) runner.UnregisterClient(blueprint.Host);
#endif

            _externalBlueprint.Unbind(token.node);
            _externalBlueprint = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return;
#endif

            _externalBlueprint.Call(new NodeToken(_externalBlueprint.root, caller: token.node), port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return default;
#endif

            return _externalBlueprint.Read<T>(new NodeToken(_externalBlueprint.root, caller: token.node), port);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
#if UNITY_EDITOR
            SubgraphValidator.ValidateSubgraphAsset(meta, ref _blueprint);
#endif

            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public bool HasSignaturePorts(NodeId id) => true;
    }

}
