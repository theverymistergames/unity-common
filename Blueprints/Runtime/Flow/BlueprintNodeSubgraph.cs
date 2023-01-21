﻿using System;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.External {

    [Serializable]
    [BlueprintNodeMeta(Name = "Subgraph", Category = "Flow", Color = BlueprintColors.Node.Flow)]
    public sealed class BlueprintNodeSubgraph :
        BlueprintNode,
        IBlueprintHost,
        IBlueprintEnter,
        IBlueprintOutput,
        IBlueprintValidatedNode,
        IBlueprintCompiledNode
    {
        [SerializeField] private BlueprintAsset _blueprintAsset;

        public BlueprintRunner Runner => _host.Runner;
        public RuntimeBlackboard Blackboard => _runtimeBlackboard;

        private RuntimeBlueprint _runtimeBlueprint;
        private RuntimeBlackboard _runtimeBlackboard;
        private IBlueprintHost _host;

        public override Port[] CreatePorts() {
            if (_blueprintAsset == null) return Array.Empty<Port>();

            var blueprintMeta = _blueprintAsset.BlueprintMeta;
            var nodesMap = blueprintMeta.NodesMap;
            var externalPortLinksMap = blueprintMeta.ExternalPortLinksMap;

            int portIndex = 0;
            int portsCount = externalPortLinksMap.Count;
            var ports = portsCount > 0 ? new Port[portsCount] : Array.Empty<Port>();

            foreach (var links in externalPortLinksMap.Values) {
                var link = links[0];
                ports[portIndex++] = nodesMap[link.nodeId].Ports[link.portIndex].SetExternal(false);
            }

            return ports;
        }

        public override void OnInitialize(IBlueprintHost host) {
            _host = host;

            _runtimeBlackboard = host.Runner.CompileBlackboardOf(_blueprintAsset);
            _runtimeBlueprint.Initialize(this);
        }

        public override void OnDeInitialize() {
            _runtimeBlueprint.DeInitialize();
        }

        public void OnEnterPort(int port) {
            CallPort(port);
        }

        public T GetPortValue<T>(int port) {
            return ReadPort<T>(port);
        }

        public void Compile(BlueprintNodeMeta nodeMeta) {
            _runtimeBlueprint = _blueprintAsset.CompileSubgraph(this, nodeMeta);
        }

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            _blueprintAsset = BlueprintValidation.ValidateBlueprintAssetForSubgraph(ownerAsset, _blueprintAsset);

            if (_blueprintAsset == null) ownerAsset.BlueprintMeta.RemoveSubgraphReference(nodeId);
            else ownerAsset.BlueprintMeta.SetSubgraphReference(nodeId, _blueprintAsset);

            ownerAsset.BlueprintMeta.InvalidateNode(nodeId);
        }
    }

}