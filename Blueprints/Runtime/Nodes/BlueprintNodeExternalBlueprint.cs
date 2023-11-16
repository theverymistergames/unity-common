using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using UnityEngine;

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
        IBlueprintEnter2,
        IBlueprintOutput2,
        IBlueprintCreateSignaturePorts
    {
        [BlackboardProperty("_blackboard")]
        [SerializeField] private int _runner;
        [SerializeField] private BlueprintAsset2 _blueprint;

        private RuntimeBlueprint2 _externalBlueprint;

#if UNITY_EDITOR
        private bool _isValidExternalBlueprint;
#endif

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            PortExtensions.FetchExternalPorts(meta, id, _blueprint);
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            var runner = blueprint.GetBlackboard(token.caller).Get<BlueprintRunner2>(_runner);

#if UNITY_EDITOR
            _isValidExternalBlueprint = SubgraphValidator2.ValidateExternalBlueprint(blueprint.Host.Runner, runner, _blueprint);
            if (!_isValidExternalBlueprint) return;
            runner.RegisterClient(blueprint.Host.Runner);
#endif

            _externalBlueprint = runner.GetOrCompileBlueprint();
            _externalBlueprint.Bind(token.node, token.caller, blueprint);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return;
            var runner = blueprint.GetBlackboard(token.caller).Get<BlueprintRunner2>(_runner);
            if (runner != null) runner.UnregisterClient(blueprint.Host.Runner);
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
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);
#endif

            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public bool HasSignaturePorts(NodeId id) => true;
    }

}
