using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceExternalBlueprint : BlueprintSource<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICompiled<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IEnter<BlueprintNodeExternalBlueprint>,
        BlueprintSources.IOutput<BlueprintNodeExternalBlueprint>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "External Blueprint", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeExternalBlueprint :
        IBlueprintNode,
        IBlueprintCompiled,
        IBlueprintEnter2,
        IBlueprintOutput2,
        IBlueprintCloneable
    {
        [BlackboardProperty("_blackboard")]
        [SerializeField] private int _runner;
        [SerializeField] private BlueprintAsset2 _blueprint;

        private IBlueprint _rootBlueprint;
        private RuntimeBlueprint2 _runtimeBlueprint;
        private NodeId _root;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            PortExtensions.FetchExternalPorts(meta, id, _blueprint);
        }

        public void OnInitialize(IBlueprint blueprint, NodeId id) {
            _rootBlueprint = blueprint;

            var runner = blueprint.Host.Blackboard.Get<BlueprintRunner2>(_runner);
            if (runner == null) return;

            _runtimeBlueprint = runner.GetOrCompileBlueprint();
            _runtimeBlueprint.Bind(id);
            _root = _runtimeBlueprint.Root;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeId id) {
            _runtimeBlueprint.Unbind(id);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeId id, int port) {
            if (blueprint == _rootBlueprint) _runtimeBlueprint.Call(_root, port);
            else _rootBlueprint.Call(id, port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeId id, int port) {
            return blueprint == _rootBlueprint
                ? _runtimeBlueprint.Read<T>(_root, port)
                : _rootBlueprint.Read<T>(id, port);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);
            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public void Compile(NodeId id, BlueprintCompileData data) { }
    }

}
