using System;
using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Validation;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceSubgraph :
        BlueprintSource<BlueprintNodeSubgraph2>,
        BlueprintSources.ICompilable<BlueprintNodeSubgraph2>,
        BlueprintSources.ICreateSignaturePorts<BlueprintNodeSubgraph2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Subgraph", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeSubgraph2 : IBlueprintNode, IBlueprintCompilable, IBlueprintCreateSignaturePorts {

        [SerializeField] private BlueprintAsset2 _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            PortExtensions.FetchExternalPorts(meta, id, _blueprint);
        }

        public bool HasSignaturePorts(NodeId id) => true;

        public void Compile(NodeId id, SubgraphCompileData data) {
#if UNITY_EDITOR
            if (_blueprint == null) return;
#endif

            _blueprint.CompileSubgraph(data);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
#if UNITY_EDITOR
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);
#endif

            if (_blueprint != null) meta.SetSubgraph(id, _blueprint);
            else meta.RemoveSubgraph(id);

            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }

}
