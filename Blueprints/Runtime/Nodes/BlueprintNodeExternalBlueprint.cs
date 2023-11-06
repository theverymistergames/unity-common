using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceExternalBlueprint :
        BlueprintSource<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IEnter<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IOutput<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICompilable<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "External Blueprint", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeExternalBlueprint :
        IBlueprintNode,
        IBlueprintEnter2,
        IBlueprintOutput2,
        IBlueprintCompilable
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
            var runner = blueprint.GetBlackboard(token).Get<BlueprintRunner2>(_runner);

#if UNITY_EDITOR
            _isValidExternalBlueprint = SubgraphValidator2.ValidateExternalBlueprint(blueprint.Host.Runner, runner, _blueprint);
            if (!_isValidExternalBlueprint) return;
#endif

            _externalBlueprint = runner.GetOrCompileBlueprint();
            _externalBlueprint.Bind(token.node, token.caller, blueprint);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return;
#endif

            _externalBlueprint.Unbind(token.node);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return;
#endif

            _externalBlueprint.CallRoot(token.node, port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
#if UNITY_EDITOR
            if (!_isValidExternalBlueprint) return default;
#endif

            return _externalBlueprint.ReadRoot<T>(token.node, port);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
#if UNITY_EDITOR
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);
#endif

            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public void Compile(NodeId id, BlueprintCompileData data) { }
    }

}
