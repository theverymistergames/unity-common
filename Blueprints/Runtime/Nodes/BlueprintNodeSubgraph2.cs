using System;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceSubgraph :
        BlueprintSource<BlueprintNodeSubgraph2>,
        BlueprintSources.ICompiled<BlueprintNodeSubgraph2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Subgraph", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeSubgraph2 : IBlueprintNode, IBlueprintCompiled {

        [SerializeField] private BlueprintAsset2 _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            PortExtensions.FetchExternalPorts(meta, id, _blueprint);
        }

        public void Compile(NodeId id, BlueprintCompileData data) {
            _blueprint.CompileSubgraph(data);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);

            if (_blueprint != null) meta.SetSubgraph(id, _blueprint);
            else meta.RemoveSubgraph(id);

            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
